using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MirageXR.FileCache
{
    /// <summary>
    /// <see cref="IFileCache"/> that stores files in <see cref="FileCacheOptions.RootDirectory"/> together with an
    /// <c>index.json</c> manifest. Every committed version gets a unique file name, so a new version never has to
    /// overwrite a file that is still being read.
    /// </summary>
    public sealed class FileCache : IFileCache, ILeaseOwner
    {
        private const string ManifestFileName = "index.json";
        private const string ManifestTempFileName = "index.json.tmp";
        private const string TempDirectoryName = "tmp";
        private const int ManifestVersion = 1;

        private static readonly JsonSerializerSettings ManifestJsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly FileCacheOptions _options;
        private readonly IClock _clock;
        private readonly IFileCacheLog _log;
        private readonly string _tempDirectory;
        private readonly string _manifestPath;
        private readonly object _lock = new object();
        private readonly SemaphoreSlim _manifestWriteLock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, CacheEntry> _entries = new Dictionary<string, CacheEntry>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _leaseCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> _deleteOnRelease = new HashSet<string>(StringComparer.Ordinal);

        private long _sizeBytes;
        private bool _isDirty;
        private bool _isSaveScheduled;
        private int _isDisposed;

        /// <param name="options">Configuration; the root directory must belong exclusively to this cache.</param>
        /// <param name="clock">Time source for TTL and LRU; defaults to the system time.</param>
        /// <param name="log">Log sink; defaults to no logging.</param>
        public FileCache(FileCacheOptions options, IClock clock = null, IFileCacheLog log = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _clock = clock ?? SystemClock.Instance;
            _log = log ?? NullFileCacheLog.Instance;
            _tempDirectory = Path.Combine(options.RootDirectory, TempDirectoryName);
            _manifestPath = Path.Combine(options.RootDirectory, ManifestFileName);
        }

        /// <inheritdoc/>
        public FileCacheOptions Options => _options;

        /// <inheritdoc/>
        public long SizeBytes
        {
            get
            {
                lock (_lock)
                {
                    return _sizeBytes;
                }
            }
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _entries.Count;
                }
            }
        }

        /// <inheritdoc/>
        public Task InitializeAsync()
        {
            return Task.Run(InitializeCore);
        }

        /// <inheritdoc/>
        public Task<FileLease> TryAcquireAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!_entries.TryGetValue(key, out var entry))
                    {
                        return null;
                    }

                    var path = GetFilePath(entry.FileName);
                    if (!File.Exists(path))
                    {
                        // the OS may purge cache folders at any time
                        _log.Debug($"[FileCache:{_options.Name}] file of '{key}' disappeared, dropping the entry");
                        _entries.Remove(key);
                        _sizeBytes -= entry.SizeBytes;
                        ScheduleSaveLocked();
                        return null;
                    }

                    entry.LastAccessUtc = _clock.UtcNow;
                    ScheduleSaveLocked();
                    return AcquireLocked(path, entry.Clone());
                }
            });
        }

        /// <inheritdoc/>
        public Task<string> CreateTempFilePathAsync(string fileExtension)
        {
            var fileName = Guid.NewGuid().ToString("N") + SanitizeExtension(fileExtension);
            return Task.Run(() =>
            {
                // the OS may have purged the folder since the initialization
                Directory.CreateDirectory(_tempDirectory);
                return Path.Combine(_tempDirectory, fileName);
            });
        }

        /// <inheritdoc/>
        public Task DiscardTempFileAsync(string tempFilePath)
        {
            if (tempFilePath == null)
            {
                throw new ArgumentNullException(nameof(tempFilePath));
            }

            return Task.Run(() =>
            {
                if (File.Exists(tempFilePath))
                {
                    TryDeleteFile(tempFilePath);
                }
            });
        }

        /// <inheritdoc/>
        public Task<FileLease> CommitAsync(string key, string tempFilePath, string eTag, string lastModified)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (tempFilePath == null)
            {
                throw new ArgumentNullException(nameof(tempFilePath));
            }

            return Task.Run(() =>
            {
                var size = new FileInfo(tempFilePath).Length;
                if (size > _options.MaxSizeBytes)
                {
                    _log.Warning($"[FileCache:{_options.Name}] '{key}' ({size} bytes) exceeds the cache size limit and is not cached");
                    lock (_lock)
                    {
                        _deleteOnRelease.Add(tempFilePath);
                        return AcquireLocked(tempFilePath, null);
                    }
                }

                var fileName = CreateFileName(key, Path.GetExtension(tempFilePath));
                var path = GetFilePath(fileName);
                File.Move(tempFilePath, path);

                var filesToDelete = new List<string>();
                FileLease lease;
                lock (_lock)
                {
                    if (_entries.TryGetValue(key, out var oldEntry))
                    {
                        RemoveEntryLocked(oldEntry, filesToDelete);
                    }

                    var now = _clock.UtcNow;
                    var entry = new CacheEntry
                    {
                        Key = key,
                        FileName = fileName,
                        SizeBytes = size,
                        LastAccessUtc = now,
                        ValidatedUtc = now,
                        ETag = eTag,
                        LastModified = lastModified,
                    };
                    _entries[key] = entry;
                    _sizeBytes += size;
                    lease = AcquireLocked(path, entry.Clone());
                    EvictLocked(filesToDelete);
                    ScheduleSaveLocked();
                }

                DeleteFiles(filesToDelete);
                return lease;
            });
        }

        /// <inheritdoc/>
        public void MarkValidated(FileLease lease, string eTag, string lastModified)
        {
            if (lease?.Entry == null)
            {
                return;
            }

            lock (_lock)
            {
                if (!_entries.TryGetValue(lease.Entry.Key, out var entry) || entry.FileName != lease.Entry.FileName)
                {
                    return; // the entry was replaced or removed in the meantime
                }

                var now = _clock.UtcNow;
                entry.ValidatedUtc = now;
                entry.LastAccessUtc = now;
                if (!string.IsNullOrEmpty(eTag))
                {
                    entry.ETag = eTag;
                }

                if (!string.IsNullOrEmpty(lastModified))
                {
                    entry.LastModified = lastModified;
                }

                ScheduleSaveLocked();
            }
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return Task.Run(() =>
            {
                var filesToDelete = new List<string>();
                lock (_lock)
                {
                    if (_entries.TryGetValue(key, out var entry))
                    {
                        RemoveEntryLocked(entry, filesToDelete);
                        ScheduleSaveLocked();
                    }
                }

                DeleteFiles(filesToDelete);
            });
        }

        /// <inheritdoc/>
        public Task ClearAsync()
        {
            return Task.Run(() =>
            {
                var filesToDelete = new List<string>();
                lock (_lock)
                {
                    foreach (var entry in _entries.Values.ToList())
                    {
                        RemoveEntryLocked(entry, filesToDelete);
                    }

                    _isDirty = true;
                }

                DeleteFiles(filesToDelete);
                SaveManifest();
                _log.Debug($"[FileCache:{_options.Name}] cleared");
            });
        }

        /// <inheritdoc/>
        public void Flush()
        {
            if (Volatile.Read(ref _isDisposed) != 0)
            {
                return;
            }

            try
            {
                SaveManifest();
            }
            catch (Exception e)
            {
                _log.Warning($"[FileCache:{_options.Name}] could not save the index on flush: {e.Message}");
            }
        }

        /// <summary>Writes pending index changes synchronously (used on shutdown). Later changes are not persisted.</summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            {
                return;
            }

            try
            {
                SaveManifest();
            }
            catch (Exception e)
            {
                _log.Warning($"[FileCache:{_options.Name}] could not save the index on dispose: {e.Message}");
            }
        }

        FileLease ILeaseOwner.AddLease(string path, CacheEntry entry)
        {
            lock (_lock)
            {
                return AcquireLocked(path, entry);
            }
        }

        void ILeaseOwner.ReleaseLease(string path)
        {
            bool delete = false;
            lock (_lock)
            {
                if (!_leaseCounts.TryGetValue(path, out var count))
                {
                    return;
                }

                if (count > 1)
                {
                    _leaseCounts[path] = count - 1;
                    return;
                }

                _leaseCounts.Remove(path);
                delete = _deleteOnRelease.Remove(path);
            }

            if (delete)
            {
                Task.Run(() => TryDeleteFile(path));
            }
        }

        private static string CreateFileName(string key, string extension)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
                var builder = new StringBuilder(hash.Length * 2 + 9 + extension.Length);
                foreach (var b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }

                // a unique suffix per version, so that a new version never overwrites a file that is still read
                builder.Append('_').Append(Guid.NewGuid().ToString("N"), 0, 8).Append(extension);
                return builder.ToString();
            }
        }

        private static string SanitizeExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return string.Empty;
            }

            var trimmed = extension.TrimStart('.');
            if (trimmed.Length == 0 || trimmed.Length > 10 || !trimmed.All(char.IsLetterOrDigit))
            {
                return string.Empty;
            }

            return "." + trimmed;
        }

        private static bool IsValidFileName(string fileName)
        {
            return !string.IsNullOrEmpty(fileName)
                && fileName == Path.GetFileName(fileName)
                && fileName != ManifestFileName
                && fileName != ManifestTempFileName;
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(_options.RootDirectory, fileName);
        }

        private FileLease AcquireLocked(string path, CacheEntry entry)
        {
            _leaseCounts.TryGetValue(path, out var count);
            _leaseCounts[path] = count + 1;
            return new FileLease(this, path, entry);
        }

        private bool IsLeasedLocked(string path)
        {
            return _leaseCounts.ContainsKey(path);
        }

        private void RemoveEntryLocked(CacheEntry entry, List<string> filesToDelete)
        {
            _entries.Remove(entry.Key);
            _sizeBytes -= entry.SizeBytes;
            var path = GetFilePath(entry.FileName);
            if (IsLeasedLocked(path))
            {
                _deleteOnRelease.Add(path);
            }
            else
            {
                filesToDelete.Add(path);
            }
        }

        private void RemoveUnusedLocked(List<string> filesToDelete)
        {
            if (_options.MaxUnusedAge is not { } maxUnusedAge)
            {
                return;
            }

            var threshold = _clock.UtcNow - maxUnusedAge;
            foreach (var entry in _entries.Values.Where(e => e.LastAccessUtc < threshold).ToList())
            {
                _log.Debug($"[FileCache:{_options.Name}] removing '{entry.Key}', unused since {entry.LastAccessUtc:u}");
                RemoveEntryLocked(entry, filesToDelete);
            }
        }

        private void EvictLocked(List<string> filesToDelete)
        {
            if (_sizeBytes <= _options.MaxSizeBytes)
            {
                return;
            }

            var candidates = _entries.Values
                .Where(e => !IsLeasedLocked(GetFilePath(e.FileName)))
                .OrderBy(e => e.LastAccessUtc)
                .ToList();

            foreach (var entry in candidates)
            {
                if (_sizeBytes <= _options.MaxSizeBytes)
                {
                    break;
                }

                _log.Debug($"[FileCache:{_options.Name}] evicting '{entry.Key}' ({entry.SizeBytes} bytes)");
                RemoveEntryLocked(entry, filesToDelete);
            }
        }

        private void ScheduleSaveLocked()
        {
            _isDirty = true;
            if (_isSaveScheduled)
            {
                return;
            }

            _isSaveScheduled = true;
            var delay = _options.ManifestSaveDelay;
            Task.Run(async () =>
            {
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay).ConfigureAwait(false);
                }

                if (Volatile.Read(ref _isDisposed) != 0)
                {
                    return;
                }

                try
                {
                    SaveManifest();
                }
                catch (Exception e)
                {
                    _log.Warning($"[FileCache:{_options.Name}] could not save the index: {e.Message}");
                }
            });
        }

        private void SaveManifest()
        {
            _manifestWriteLock.Wait();
            try
            {
                Manifest manifest;
                lock (_lock)
                {
                    _isSaveScheduled = false;
                    if (!_isDirty)
                    {
                        return;
                    }

                    _isDirty = false;
                    manifest = new Manifest
                    {
                        Version = ManifestVersion,
                        Entries = _entries.Values.Select(e => e.Clone()).ToList(),
                    };
                }

                try
                {
                    WriteManifest(manifest);
                }
                catch
                {
                    lock (_lock)
                    {
                        _isDirty = true;
                    }

                    throw;
                }
            }
            finally
            {
                _manifestWriteLock.Release();
            }
        }

        private void WriteManifest(Manifest manifest)
        {
            Directory.CreateDirectory(_options.RootDirectory);
            var tempPath = GetFilePath(ManifestTempFileName);
            using (var writer = new StreamWriter(tempPath, false, new UTF8Encoding(false)))
            {
                JsonSerializer.Create(ManifestJsonSettings).Serialize(writer, manifest);
            }

            if (File.Exists(_manifestPath))
            {
                File.Replace(tempPath, _manifestPath, null);
            }
            else
            {
                File.Move(tempPath, _manifestPath);
            }
        }

        private void InitializeCore()
        {
            Directory.CreateDirectory(_options.RootDirectory);

            // downloads interrupted by an app exit
            if (Directory.Exists(_tempDirectory))
            {
                DeleteFiles(Directory.GetFiles(_tempDirectory));
            }

            Directory.CreateDirectory(_tempDirectory);

            var manifest = ReadManifest(out var isManifestCorrupt);
            if (isManifestCorrupt)
            {
                _log.Warning($"[FileCache:{_options.Name}] the index is corrupt, clearing the cache");
            }

            var filesOnDisk = new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (var file in new DirectoryInfo(_options.RootDirectory).GetFiles())
            {
                if (file.Name != ManifestFileName)
                {
                    filesOnDisk[file.Name] = file.Length;
                }
            }

            var filesToDelete = new List<string>();
            lock (_lock)
            {
                _entries.Clear();
                _sizeBytes = 0;

                foreach (var entry in manifest?.Entries ?? Enumerable.Empty<CacheEntry>())
                {
                    if (entry?.Key == null
                        || !IsValidFileName(entry.FileName)
                        || _entries.ContainsKey(entry.Key)
                        || !filesOnDisk.TryGetValue(entry.FileName, out var actualSize))
                    {
                        _isDirty = true;
                        continue;
                    }

                    if (entry.SizeBytes != actualSize)
                    {
                        entry.SizeBytes = actualSize;
                        _isDirty = true;
                    }

                    _entries[entry.Key] = entry;
                    _sizeBytes += entry.SizeBytes;
                }

                var referencedFiles = new HashSet<string>(_entries.Values.Select(e => e.FileName), StringComparer.Ordinal);
                foreach (var fileName in filesOnDisk.Keys)
                {
                    if (!referencedFiles.Contains(fileName))
                    {
                        filesToDelete.Add(GetFilePath(fileName));
                    }
                }

                RemoveUnusedLocked(filesToDelete);
                EvictLocked(filesToDelete);
                if (filesToDelete.Count > 0 || isManifestCorrupt)
                {
                    _isDirty = true;
                }
            }

            DeleteFiles(filesToDelete);
            SaveManifest();
            _log.Debug($"[FileCache:{_options.Name}] initialized with {Count} entries ({SizeBytes} bytes)");
        }

        private Manifest ReadManifest(out bool isCorrupt)
        {
            isCorrupt = false;
            if (!File.Exists(_manifestPath))
            {
                return null;
            }

            try
            {
                using (var reader = new StreamReader(_manifestPath, Encoding.UTF8))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var manifest = JsonSerializer.Create(ManifestJsonSettings).Deserialize<Manifest>(jsonReader);
                    if (manifest != null && manifest.Version == ManifestVersion)
                    {
                        return manifest;
                    }
                }
            }
            catch (Exception e) when (e is JsonException || e is IOException)
            {
                _log.Debug($"[FileCache:{_options.Name}] could not read the index: {e.Message}");
            }

            // unreadable or unknown version: entries are dropped, their files are deleted as orphans
            isCorrupt = true;
            return null;
        }

        private void DeleteFiles(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                TryDeleteFile(path);
            }
        }

        private void TryDeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _log.Warning($"[FileCache:{_options.Name}] could not delete '{path}': {e.Message}");
            }
        }

        private sealed class Manifest
        {
            [JsonProperty("version")]
            public int Version { get; set; }

            [JsonProperty("entries")]
            public List<CacheEntry> Entries { get; set; }
        }
    }
}
