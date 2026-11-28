using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MirageXR.FileCache;
using NUnit.Framework;

namespace Tests
{
    public class CachedFileProviderTests
    {
        private const string Url = "https://example.org:8001/avatar/get?avatar_name=Test";
        private static readonly TimeSpan TimeToLive = TimeSpan.FromDays(7);

        private string _rootDirectory;
        private FakeClock _clock;
        private FakeFetcher _fetcher;
        private FileCache _cache;
        private CachedFileProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _rootDirectory = FileCacheTestUtils.CreateTempDirectory();
            _clock = new FakeClock();
            _fetcher = new FakeFetcher();
            _cache = new FileCache(FileCacheTestUtils.CreateOptions(_rootDirectory, 1000, TimeToLive), _clock);
            _cache.InitializeAsync().GetAwaiter().GetResult(); // runs on the thread pool, no deadlock
            _provider = new CachedFileProvider(_cache, _fetcher, 2, _clock);
        }

        [TearDown]
        public void TearDown()
        {
            _cache.Dispose();
            FileCacheTestUtils.DeleteDirectory(_rootDirectory);
        }

        [Test]
        public async Task Miss_DownloadsOnce_ThenServesFromCache()
        {
            _fetcher.Content = new byte[] { 1, 2, 3 };

            using (var file = await _provider.GetFileAsync(new CacheRequest(Url)))
            {
                Assert.IsNotNull(file);
                Assert.IsFalse(file.FromCache);
                Assert.AreEqual(new byte[] { 1, 2, 3 }, File.ReadAllBytes(file.Path));
            }

            _clock.Advance(TimeToLive - TimeSpan.FromMinutes(1));
            using (var file = await _provider.GetFileAsync(new CacheRequest(Url)))
            {
                Assert.IsTrue(file.FromCache);
                Assert.IsFalse(file.IsStale);
                Assert.AreEqual(new byte[] { 1, 2, 3 }, File.ReadAllBytes(file.Path));
            }

            Assert.AreEqual(1, _fetcher.Requests.Count, "a fresh entry must not cause a network request");
            Assert.IsFalse(_fetcher.Requests[0].IsConditional);
        }

        [Test]
        public async Task Stale_WithETag_SendsConditionalRequest_AndNotModifiedKeepsFile()
        {
            _fetcher.Result = FetchResult.Ok("\"v1\"", "Wed, 01 Jan 2026 00:00:00 GMT");
            (await _provider.GetFileAsync(new CacheRequest(Url))).Dispose();
            _clock.Advance(TimeToLive + TimeSpan.FromMinutes(1));
            _fetcher.Result = FetchResult.NotModified();

            using (var file = await _provider.GetFileAsync(new CacheRequest(Url)))
            {
                Assert.IsTrue(file.FromCache);
                Assert.IsFalse(file.IsStale);
                Assert.AreEqual(new byte[] { 1, 2, 3 }, File.ReadAllBytes(file.Path));
            }

            Assert.AreEqual(2, _fetcher.Requests.Count);
            Assert.AreEqual("\"v1\"", _fetcher.Requests[1].IfNoneMatch);
            Assert.AreEqual("Wed, 01 Jan 2026 00:00:00 GMT", _fetcher.Requests[1].IfModifiedSince);

            // the validation time was refreshed: no request within the next TTL
            _clock.Advance(TimeSpan.FromDays(1));
            (await _provider.GetFileAsync(new CacheRequest(Url))).Dispose();
            Assert.AreEqual(2, _fetcher.Requests.Count);
            Assert.IsEmpty(FileCacheTestUtils.GetTempFiles(_rootDirectory));
        }

        [Test]
        public async Task Stale_WithoutValidators_DownloadsAgain()
        {
            (await _provider.GetFileAsync(new CacheRequest(Url))).Dispose();
            _clock.Advance(TimeToLive + TimeSpan.FromMinutes(1));
            _fetcher.Content = new byte[] { 9, 9 };

            using (var file = await _provider.GetFileAsync(new CacheRequest(Url)))
            {
                Assert.IsFalse(file.FromCache);
                Assert.AreEqual(new byte[] { 9, 9 }, File.ReadAllBytes(file.Path));
            }

            Assert.AreEqual(2, _fetcher.Requests.Count);
            Assert.IsFalse(_fetcher.Requests[1].IsConditional);
            Assert.AreEqual(1, _cache.Count);
            Assert.AreEqual(2, _cache.SizeBytes);
        }

        [Test]
        public async Task Stale_FetchFails_ReturnsStaleFile()
        {
            (await _provider.GetFileAsync(new CacheRequest(Url))).Dispose();
            _clock.Advance(TimeToLive + TimeSpan.FromMinutes(1));
            _fetcher.Result = FetchResult.Failed("offline");

            using (var file = await _provider.GetFileAsync(new CacheRequest(Url)))
            {
                Assert.IsNotNull(file);
                Assert.IsTrue(file.FromCache);
                Assert.IsTrue(file.IsStale);
                Assert.AreEqual(new byte[] { 1, 2, 3 }, File.ReadAllBytes(file.Path));
            }

            Assert.IsEmpty(FileCacheTestUtils.GetTempFiles(_rootDirectory));
        }

        [Test]
        public async Task Miss_FetchFails_ReturnsNullAndCachesNothing()
        {
            _fetcher.Result = FetchResult.Failed("HTTP 500");

            var file = await _provider.GetFileAsync(new CacheRequest(Url));

            Assert.IsNull(file);
            Assert.AreEqual(0, _cache.Count);
            Assert.IsEmpty(FileCacheTestUtils.GetTempFiles(_rootDirectory));
        }

        [Test]
        public async Task ConcurrentRequestsForSameKey_ShareOneDownload()
        {
            _fetcher.Gate = new TaskCompletionSource<bool>();

            var first = _provider.GetFileAsync(new CacheRequest(Url));
            var second = _provider.GetFileAsync(new CacheRequest(Url));
            await FileCacheTestUtils.WaitUntilAsync(() => _fetcher.Requests.Count > 0);
            _fetcher.Gate.SetResult(true);
            var files = await Task.WhenAll(first, second);

            Assert.AreEqual(1, _fetcher.Requests.Count);
            Assert.IsNotNull(files[0]);
            Assert.IsNotNull(files[1]);
            Assert.AreEqual(files[0].Path, files[1].Path);

            // each caller holds its own lease: the file survives the first dispose
            await _cache.RemoveAsync(files[0].Key);
            files[0].Dispose();
            await Task.Delay(50);
            Assert.IsTrue(File.Exists(files[1].Path));
            files[1].Dispose();
            Assert.IsTrue(await FileCacheTestUtils.WaitUntilAsync(() => !File.Exists(files[1].Path)));
        }

        [Test]
        public async Task Cancellation_AbortsDownloadAndDeletesTempFile()
        {
            _fetcher.Gate = new TaskCompletionSource<bool>(); // never completes
            using (var cancellation = new CancellationTokenSource())
            {
                var task = _provider.GetFileAsync(new CacheRequest(Url), cancellation.Token);
                await FileCacheTestUtils.WaitUntilAsync(() => _fetcher.Requests.Count > 0);
                Assert.IsNotEmpty(FileCacheTestUtils.GetTempFiles(_rootDirectory));

                cancellation.Cancel();

                try
                {
                    await task;
                    Assert.Fail("expected an OperationCanceledException");
                }
                catch (OperationCanceledException)
                {
                }
            }

            Assert.IsTrue(await FileCacheTestUtils.WaitUntilAsync(() => FileCacheTestUtils.GetTempFiles(_rootDirectory).Length == 0));
            Assert.AreEqual(0, _cache.Count);
        }

        [Test]
        public async Task CacheKey_IgnoresHeaders()
        {
            var headersA = new Dictionary<string, string> { ["Authorization"] = "Bearer token-a" };
            var headersB = new Dictionary<string, string> { ["Authorization"] = "Bearer token-b" };

            (await _provider.GetFileAsync(new CacheRequest(Url, headersA))).Dispose();
            using (var file = await _provider.GetFileAsync(new CacheRequest(Url, headersB)))
            {
                Assert.IsTrue(file.FromCache);
            }

            Assert.AreEqual(1, _fetcher.Requests.Count);
            Assert.AreEqual("Bearer token-a", _fetcher.Requests[0].Headers["Authorization"]);
        }

        [Test]
        public void CacheKey_DiffersPerServerPort()
        {
            Assert.AreNotEqual(
                CacheRequest.CreateKey("https://example.org:8000/avatar/get?avatar_name=Test"),
                CacheRequest.CreateKey("https://example.org:8001/avatar/get?avatar_name=Test"));
            Assert.AreEqual(
                CacheRequest.CreateKey("HTTPS://Example.org:8001/avatar/get?avatar_name=Test"),
                CacheRequest.CreateKey(Url));
        }

        [Test]
        public async Task LoadAsync_CorruptCachedFile_IsRemovedAndDownloadedAgain()
        {
            (await _provider.GetFileAsync(new CacheRequest(Url))).Dispose();
            _fetcher.Content = new byte[] { 5 };
            var loadCalls = 0;

            var result = await _provider.LoadAsync(new CacheRequest(Url), (path, _) =>
            {
                loadCalls++;
                var bytes = File.ReadAllBytes(path);
                return Task.FromResult(bytes.Length == 1 ? "loaded" : null); // the cached 3-byte file is "corrupt"
            });

            Assert.AreEqual("loaded", result);
            Assert.AreEqual(2, loadCalls);
            Assert.AreEqual(2, _fetcher.Requests.Count);
        }

        [Test]
        public async Task LoadAsync_DownloadedFileFails_ReturnsNullWithoutRetryAndRemovesEntry()
        {
            var loadCalls = 0;

            var result = await _provider.LoadAsync<string>(new CacheRequest(Url), (path, _) =>
            {
                loadCalls++;
                throw new InvalidDataException("not a glTF");
            });

            Assert.IsNull(result);
            Assert.AreEqual(1, loadCalls);
            Assert.AreEqual(0, _cache.Count);
        }
    }
}
