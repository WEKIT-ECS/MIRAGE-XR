using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Serves remote files through an <see cref="IFileCache"/>: fresh entries come from disk without network access,
    /// stale entries are revalidated (conditional request if validators are known), and offline/server errors fall
    /// back to stale entries. Concurrent requests for the same key share one download.
    /// </summary>
    /// <remarks>
    /// Orchestration continuations run on the caller's synchronization context (the Unity main thread), which
    /// <c>UnityWebRequest</c>-based fetchers require. Disk I/O is done by the cache on the thread pool.
    /// </remarks>
    public sealed class CachedFileProvider
    {
        private readonly IFileCache _cache;
        private readonly IRemoteFileFetcher _fetcher;
        private readonly IClock _clock;
        private readonly IFileCacheLog _log;
        private readonly SemaphoreSlim _downloadSlots;
        private readonly object _lock = new object();
        private readonly Dictionary<string, InFlightRequest> _inFlight = new Dictionary<string, InFlightRequest>(StringComparer.Ordinal);

        public CachedFileProvider(IFileCache cache, IRemoteFileFetcher fetcher, int maxConcurrentDownloads, IClock clock = null, IFileCacheLog log = null)
        {
            if (maxConcurrentDownloads < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxConcurrentDownloads));
            }

            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
            _clock = clock ?? SystemClock.Instance;
            _log = log ?? NullFileCacheLog.Instance;
            _downloadSlots = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);
        }

        public IFileCache Cache => _cache;

        private string Name => _cache.Options.Name;

        /// <summary>
        /// Returns the local file for <paramref name="request"/>, or <c>null</c> if it is neither cached nor
        /// downloadable. The caller owns the returned object and must dispose it after reading the file.
        /// </summary>
        public async Task<CachedFile> GetFileAsync(CacheRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            cancellationToken.ThrowIfCancellationRequested();

            InFlightRequest inFlight;
            lock (_lock)
            {
                if (!_inFlight.TryGetValue(request.Key, out inFlight))
                {
                    inFlight = new InFlightRequest();
                    _inFlight[request.Key] = inFlight;
                    inFlight.Task = ResolveAsync(request, inFlight.Cancellation.Token);
                }

                inFlight.Waiters++;
            }

            try
            {
                var resolution = await WaitAsync(inFlight.Task, cancellationToken);
                if (resolution == null)
                {
                    return null;
                }

                // every caller gets its own lease; the shared one is released when the last caller is done
                return new CachedFile(request.Key, resolution.Lease.Duplicate(), resolution.FromCache, resolution.IsStale);
            }
            finally
            {
                ReleaseWaiter(request.Key, inFlight);
            }
        }

        /// <summary>
        /// Gets the file and loads it with <paramref name="load"/> while the file is leased. A <c>null</c> result or an
        /// exception of <paramref name="load"/> counts as a corrupt file: the entry is removed and, if it came from the
        /// cache, downloaded and loaded once more.
        /// </summary>
        /// <returns>The loaded object, or <c>null</c> if the file could not be obtained or loaded.</returns>
        public async Task<T> LoadAsync<T>(CacheRequest request, Func<string, CancellationToken, Task<T>> load, CancellationToken cancellationToken = default)
            where T : class
        {
            if (load == null)
            {
                throw new ArgumentNullException(nameof(load));
            }

            for (var attempt = 0; ; attempt++)
            {
                bool fromCache;
                T result = null;
                using (var file = await GetFileAsync(request, cancellationToken))
                {
                    if (file == null)
                    {
                        return null;
                    }

                    fromCache = file.FromCache;
                    try
                    {
                        result = await load(file.Path, cancellationToken);
                    }
                    catch (Exception e) when (!(e is OperationCanceledException))
                    {
                        _log.Warning($"[FileCache:{Name}] loading '{request.Key}' failed: {e.Message}");
                    }
                }

                if (result != null)
                {
                    return result;
                }

                await _cache.RemoveAsync(request.Key);
                if (!fromCache || attempt > 0)
                {
                    return null;
                }

                _log.Warning($"[FileCache:{Name}] cached file of '{request.Key}' could not be loaded, downloading it again");
            }
        }

        private static async Task<T> WaitAsync<T>(Task<T> task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled || task.IsCompleted)
            {
                return await task;
            }

            var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
            {
                if (await Task.WhenAny(task, cancelled.Task) != task)
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            return await task;
        }

        private void ReleaseWaiter(string key, InFlightRequest inFlight)
        {
            lock (_lock)
            {
                inFlight.Waiters--;
                if (inFlight.Waiters > 0)
                {
                    return;
                }

                if (_inFlight.TryGetValue(key, out var current) && current == inFlight)
                {
                    _inFlight.Remove(key);
                }
            }

            if (!inFlight.Task.IsCompleted)
            {
                // nobody waits for the result anymore
                inFlight.Cancellation.Cancel();
            }

            inFlight.Task.ContinueWith(
                task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        task.Result?.Lease.Dispose();
                    }
                    else if (task.IsFaulted)
                    {
                        _ = task.Exception; // observed by the waiters or irrelevant once all of them left
                    }

                    inFlight.Cancellation.Dispose();
                },
                TaskScheduler.Default);
        }

        private async Task<Resolution> ResolveAsync(CacheRequest request, CancellationToken cancellationToken)
        {
            var cached = await _cache.TryAcquireAsync(request.Key);
            if (cached != null && _clock.UtcNow - cached.Entry.ValidatedUtc < _cache.Options.TimeToLive)
            {
                _log.Debug($"[FileCache:{Name}] hit '{request.Key}'");
                return new Resolution(cached, true, false);
            }

            string tempPath;
            try
            {
                tempPath = await _cache.CreateTempFilePathAsync(request.FileExtension);
            }
            catch
            {
                cached?.Dispose();
                throw;
            }

            try
            {
                var fetchRequest = new FetchRequest(request.Url, request.Headers, tempPath, cached?.Entry.ETag, cached?.Entry.LastModified);
                _log.Debug($"[FileCache:{Name}] {(cached == null ? "miss" : "stale")} '{request.Key}', fetching{(fetchRequest.IsConditional ? " (conditional)" : string.Empty)}");
                var result = await FetchAsync(fetchRequest, cancellationToken);

                if (result.Status == FetchStatus.Ok)
                {
                    var lease = await _cache.CommitAsync(request.Key, tempPath, result.ETag, result.LastModified);
                    cached?.Dispose();
                    return new Resolution(lease, false, false);
                }

                await _cache.DiscardTempFileAsync(tempPath);

                if (result.Status == FetchStatus.NotModified && cached != null)
                {
                    _log.Debug($"[FileCache:{Name}] not modified '{request.Key}'");
                    _cache.MarkValidated(cached, result.ETag, result.LastModified);
                    return new Resolution(cached, true, false);
                }

                var error = result.Status == FetchStatus.Failed ? result.Error : "unexpected 'not modified' response";
                if (cached != null)
                {
                    _log.Warning($"[FileCache:{Name}] could not revalidate '{request.Key}' ({error}), using the cached file");
                    return new Resolution(cached, true, true);
                }

                _log.Warning($"[FileCache:{Name}] could not download '{request.Key}': {error}");
                return null;
            }
            catch
            {
                await _cache.DiscardTempFileAsync(tempPath);
                cached?.Dispose();
                throw;
            }
        }

        private async Task<FetchResult> FetchAsync(FetchRequest request, CancellationToken cancellationToken)
        {
            await _downloadSlots.WaitAsync(cancellationToken);
            try
            {
                return await _fetcher.FetchToFileAsync(request, cancellationToken);
            }
            finally
            {
                _downloadSlots.Release();
            }
        }

        private sealed class InFlightRequest
        {
            public CancellationTokenSource Cancellation { get; } = new CancellationTokenSource();

            public Task<Resolution> Task { get; set; }

            public int Waiters { get; set; }
        }

        private sealed class Resolution
        {
            public Resolution(FileLease lease, bool fromCache, bool isStale)
            {
                Lease = lease;
                FromCache = fromCache;
                IsStale = isStale;
            }

            public FileLease Lease { get; }

            public bool FromCache { get; }

            public bool IsStale { get; }
        }
    }
}
