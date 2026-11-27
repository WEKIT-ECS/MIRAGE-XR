using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MirageXR.FileCache;
using NUnit.Framework;

namespace Tests
{
    public class ClearCacheControllerTests
    {
        [Test]
        public void SizeText_FormatsTotalSizeInMegabytes()
        {
            var service = new FakeFileCacheService { TotalSizeBytes = 3L * 1024 * 1024 / 2 };
            using var controller = new ClearCacheController(service);

            Assert.AreEqual("1.5 MB", controller.SizeText);
            service.TotalSizeBytes = 0;
            Assert.AreEqual("0.0 MB", controller.SizeText);
        }

        [Test]
        public async Task WithoutService_IsDisabledAndEmpty()
        {
            using var controller = new ClearCacheController(null);

            Assert.IsFalse(controller.CanClear);
            Assert.AreEqual(string.Empty, controller.SizeText);
            Assert.IsFalse(await controller.ClearAsync());
        }

        [Test]
        public async Task ClearAsync_IsBusyWhileClearingAndNotifies()
        {
            var service = new FakeFileCacheService();
            using var controller = new ClearCacheController(service);
            var states = new List<bool>();
            controller.StateChanged += () => states.Add(controller.IsClearing);

            var clearTask = controller.ClearAsync();

            Assert.IsTrue(controller.IsClearing);
            Assert.IsFalse(controller.CanClear);

            service.CompleteClear();
            Assert.IsTrue(await clearTask);

            Assert.IsFalse(controller.IsClearing);
            Assert.IsTrue(controller.CanClear);
            Assert.AreEqual(1, service.ClearCalls);
            CollectionAssert.AreEqual(new[] { true, true, false }, states); // start, Cleared event (still clearing), end
        }

        [Test]
        public async Task ClearAsync_WhileClearing_DoesNotClearTwice()
        {
            var service = new FakeFileCacheService();
            using var controller = new ClearCacheController(service);

            var first = controller.ClearAsync();
            var second = await controller.ClearAsync();
            service.CompleteClear();

            Assert.IsFalse(second);
            Assert.IsTrue(await first);
            Assert.AreEqual(1, service.ClearCalls);
        }

        [Test]
        public async Task ClearAsync_Failure_ReturnsFalseLogsAndResetsBusyState()
        {
            var service = new FakeFileCacheService();
            var log = new RecordingLog();
            using var controller = new ClearCacheController(service, log);

            var clearTask = controller.ClearAsync();
            service.FailClear(new InvalidOperationException("disk error"));

            Assert.IsFalse(await clearTask);
            Assert.IsFalse(controller.IsClearing);
            Assert.IsTrue(controller.CanClear);
            Assert.AreEqual(1, log.Warnings.Count);
            StringAssert.Contains("disk error", log.Warnings[0]);
        }

        [Test]
        public void CacheClearedElsewhere_RaisesStateChanged_UntilDisposed()
        {
            var service = new FakeFileCacheService();
            var controller = new ClearCacheController(service);
            var notifications = 0;
            controller.StateChanged += () => notifications++;

            service.RaiseCleared();
            controller.Dispose();
            service.RaiseCleared();

            Assert.AreEqual(1, notifications);
            Assert.IsFalse(controller.CanClear);
        }

        private sealed class FakeFileCacheService : IFileCacheService
        {
            private TaskCompletionSource<bool> _clear;

            public event Action Cleared;

            public CachedFileProvider AvatarModels => null;

            public CachedFileProvider EnvironmentModels => null;

            public CachedFileProvider Thumbnails => null;

            public long TotalSizeBytes { get; set; }

            public int ClearCalls { get; private set; }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            public async Task ClearAllAsync()
            {
                ClearCalls++;
                _clear = new TaskCompletionSource<bool>();
                await _clear.Task;
                Cleared?.Invoke();
            }

            public void CompleteClear()
            {
                _clear.SetResult(true);
            }

            public void FailClear(Exception exception)
            {
                _clear.SetException(exception);
            }

            public void RaiseCleared()
            {
                Cleared?.Invoke();
            }

            public void Flush()
            {
            }

            public void Dispose()
            {
            }
        }

        private sealed class RecordingLog : IFileCacheLog
        {
            public List<string> Warnings { get; } = new List<string>();

            public void Debug(string message)
            {
            }

            public void Warning(string message)
            {
                Warnings.Add(message);
            }
        }
    }
}
