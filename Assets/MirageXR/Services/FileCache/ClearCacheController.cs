using System;
using System.Globalization;
using System.Threading.Tasks;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Platform-independent logic behind a "clear cache" settings row (mobile and spatial UI): provides the size text
    /// and the busy state and clears all caches. Views only forward the button click and render <see cref="SizeText"/>
    /// and <see cref="CanClear"/> whenever <see cref="StateChanged"/> is raised.
    /// </summary>
    public sealed class ClearCacheController : IDisposable
    {
        private readonly IFileCacheService _fileCacheService;
        private readonly IFileCacheLog _log;
        private bool _isDisposed;

        /// <param name="fileCacheService">The caches to report and clear; <c>null</c> disables the row.</param>
        public ClearCacheController(IFileCacheService fileCacheService, IFileCacheLog log = null)
        {
            _fileCacheService = fileCacheService;
            _log = log ?? NullFileCacheLog.Instance;

            if (_fileCacheService != null)
            {
                _fileCacheService.Cleared += OnCacheCleared;
            }
        }

        /// <summary>Raised when <see cref="SizeText"/>, <see cref="IsClearing"/> or <see cref="CanClear"/> may have changed.</summary>
        public event Action StateChanged;

        public bool IsClearing { get; private set; }

        public bool CanClear => _fileCacheService != null && !IsClearing && !_isDisposed;

        /// <summary>Current total size of all caches, e.g. "12.3 MB"; empty if there is no cache service.</summary>
        public string SizeText => _fileCacheService == null ? string.Empty : FormatSize(_fileCacheService.TotalSizeBytes);

        public static string FormatSize(long bytes)
        {
            return (bytes / (1024.0 * 1024.0)).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
        }

        /// <summary>
        /// Clears all caches. Never throws: returns <c>false</c> if clearing is not possible, already running or failed
        /// (the error is logged).
        /// </summary>
        public async Task<bool> ClearAsync()
        {
            if (!CanClear)
            {
                return false;
            }

            SetClearing(true);
            try
            {
                await _fileCacheService.ClearAllAsync();
                return true;
            }
            catch (Exception e)
            {
                _log.Warning($"[ClearCacheController] clearing the cache failed: {e}");
                return false;
            }
            finally
            {
                SetClearing(false);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            if (_fileCacheService != null)
            {
                _fileCacheService.Cleared -= OnCacheCleared;
            }

            StateChanged = null;
        }

        private void SetClearing(bool value)
        {
            IsClearing = value;
            StateChanged?.Invoke();
        }

        // also raised when another view (or code) cleared the caches
        private void OnCacheCleared()
        {
            StateChanged?.Invoke();
        }
    }
}
