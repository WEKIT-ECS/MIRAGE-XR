using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MirageXR.FileCache;
using NUnit.Framework;

namespace Tests
{
    public class FileCacheTests
    {
        private string _rootDirectory;
        private FakeClock _clock;
        private readonly List<IFileCache> _caches = new List<IFileCache>();

        [SetUp]
        public void SetUp()
        {
            _rootDirectory = FileCacheTestUtils.CreateTempDirectory();
            _clock = new FakeClock();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var cache in _caches)
            {
                cache.Dispose();
            }

            _caches.Clear();
            FileCacheTestUtils.DeleteDirectory(_rootDirectory);
        }

        [Test]
        public async Task Commit_ThenAcquire_ReturnsCommittedFile()
        {
            var cache = await CreateCacheAsync();

            using (var lease = await FileCacheTestUtils.CommitBytesAsync(cache, "a", 10, 7, "etag-1"))
            {
                Assert.IsTrue(lease.IsCached);
                Assert.IsTrue(File.Exists(lease.Path));
            }

            using (var lease = await cache.TryAcquireAsync("a"))
            {
                Assert.IsNotNull(lease);
                Assert.AreEqual(Enumerable.Repeat((byte)7, 10).ToArray(), File.ReadAllBytes(lease.Path));
                Assert.AreEqual("etag-1", lease.Entry.ETag);
            }

            Assert.AreEqual(10, cache.SizeBytes);
            Assert.AreEqual(1, cache.Count);
            Assert.IsNull(await cache.TryAcquireAsync("unknown"));
        }

        [Test]
        public async Task Commit_OverLimit_EvictsLeastRecentlyUsed()
        {
            var cache = await CreateCacheAsync(maxSizeBytes: 25);
            (await FileCacheTestUtils.CommitBytesAsync(cache, "a", 10)).Dispose();
            _clock.Advance(TimeSpan.FromMinutes(1));
            (await FileCacheTestUtils.CommitBytesAsync(cache, "b", 10)).Dispose();
            _clock.Advance(TimeSpan.FromMinutes(1));
            (await cache.TryAcquireAsync("a")).Dispose(); // "a" is now more recently used than "b"
            _clock.Advance(TimeSpan.FromMinutes(1));

            (await FileCacheTestUtils.CommitBytesAsync(cache, "c", 10)).Dispose();

            Assert.IsNotNull(await AcquireAndReleaseAsync(cache, "a"));
            Assert.IsNull(await AcquireAndReleaseAsync(cache, "b"));
            Assert.IsNotNull(await AcquireAndReleaseAsync(cache, "c"));
            Assert.AreEqual(20, cache.SizeBytes);
        }

        [Test]
        public async Task Eviction_SkipsLeasedEntries()
        {
            var cache = await CreateCacheAsync(maxSizeBytes: 25);
            var leaseA = await FileCacheTestUtils.CommitBytesAsync(cache, "a", 10); // oldest, but in use
            _clock.Advance(TimeSpan.FromMinutes(1));
            (await FileCacheTestUtils.CommitBytesAsync(cache, "b", 10)).Dispose();
            _clock.Advance(TimeSpan.FromMinutes(1));

            (await FileCacheTestUtils.CommitBytesAsync(cache, "c", 10)).Dispose();

            Assert.IsTrue(File.Exists(leaseA.Path));
            leaseA.Dispose();
            Assert.IsNotNull(await AcquireAndReleaseAsync(cache, "a"));
            Assert.IsNull(await AcquireAndReleaseAsync(cache, "b"));
            Assert.IsNotNull(await AcquireAndReleaseAsync(cache, "c"));
        }

        [Test]
        public async Task Commit_LargerThanLimit_IsNotCachedAndDeletedOnRelease()
        {
            var cache = await CreateCacheAsync(maxSizeBytes: 5);

            var lease = await FileCacheTestUtils.CommitBytesAsync(cache, "big", 10);

            Assert.IsFalse(lease.IsCached);
            Assert.IsTrue(File.Exists(lease.Path));
            Assert.AreEqual(0, cache.Count);
            Assert.AreEqual(0, cache.SizeBytes);

            lease.Dispose();
            Assert.IsTrue(await FileCacheTestUtils.WaitUntilAsync(() => !File.Exists(lease.Path)));
        }

        [Test]
        public async Task Remove_LeasedFile_IsDeletedWhenReleased()
        {
            var cache = await CreateCacheAsync();
            var lease = await FileCacheTestUtils.CommitBytesAsync(cache, "a", 10);

            await cache.RemoveAsync("a");

            Assert.AreEqual(0, cache.Count);
            Assert.IsTrue(File.Exists(lease.Path), "a leased file must stay readable");
            lease.Dispose();
            Assert.IsTrue(await FileCacheTestUtils.WaitUntilAsync(() => !File.Exists(lease.Path)));
        }

        [Test]
        public async Task Commit_SameKey_ReplacesOldVersion()
        {
            var cache = await CreateCacheAsync();
            var oldLease = await FileCacheTestUtils.CommitBytesAsync(cache, "a", 10, 1);

            using (var newLease = await FileCacheTestUtils.CommitBytesAsync(cache, "a", 4, 2))
            {
                Assert.AreNotEqual(oldLease.Path, newLease.Path);
                Assert.AreEqual(new byte[] { 1 }, File.ReadAllBytes(oldLease.Path).Distinct().ToArray(), "the old version stays readable while leased");
            }

            Assert.AreEqual(1, cache.Count);
            Assert.AreEqual(4, cache.SizeBytes);
            oldLease.Dispose();
            Assert.IsTrue(await FileCacheTestUtils.WaitUntilAsync(() => !File.Exists(oldLease.Path)));
        }

        [Test]
        public async Task Clear_RemovesFilesAndIndexEntries()
        {
            var cache = await CreateCacheAsync();
            (await FileCacheTestUtils.CommitBytesAsync(cache, "a", 10)).Dispose();
            (await FileCacheTestUtils.CommitBytesAsync(cache, "b", 10)).Dispose();

            await cache.ClearAsync();

            Assert.AreEqual(0, cache.Count);
            Assert.AreEqual(0, cache.SizeBytes);
            Assert.IsNull(await cache.TryAcquireAsync("a"));
            var remainingFiles = Directory.GetFiles(_rootDirectory).Select(Path.GetFileName).ToArray();
            CollectionAssert.AreEquivalent(new[] { "index.json" }, remainingFiles);

            var reopened = await CreateCacheAsync();
            Assert.AreEqual(0, reopened.Count);
        }

        [Test]
        public async Task Manifest_PersistsAcrossInstances()
        {
            var cache = await CreateCacheAsync();
            (await FileCacheTestUtils.CommitBytesAsync(cache, "a", 10, 3, "etag-a")).Dispose();
            cache.Dispose();

            var reopened = await CreateCacheAsync();

            Assert.AreEqual(1, reopened.Count);
            Assert.AreEqual(10, reopened.SizeBytes);
            using (var lease = await reopened.TryAcquireAsync("a"))
            {
                Assert.IsNotNull(lease);
                Assert.AreEqual("etag-a", lease.Entry.ETag);
                Assert.AreEqual(_clock.UtcNow, lease.Entry.ValidatedUtc);
            }
        }

        [Test]
        public async Task Initialize_ReconcilesMissingFilesOrphansAndTempFiles()
        {
            var cache = await CreateCacheAsync();
            string pathA;
            using (var lease = await FileCacheTestUtils.CommitBytesAsync(cache, "a", 10))
            {
                pathA = lease.Path;
            }

            (await FileCacheTestUtils.CommitBytesAsync(cache, "b", 10)).Dispose();
            cache.Dispose();

            File.Delete(pathA); // purged by the OS
            var orphanPath = Path.Combine(_rootDirectory, "orphan.bin");
            File.WriteAllBytes(orphanPath, new byte[] { 1 });
            var interruptedDownloadPath = Path.Combine(_rootDirectory, "tmp", "interrupted.bin");
            File.WriteAllBytes(interruptedDownloadPath, new byte[] { 1 });

            var reopened = await CreateCacheAsync();

            Assert.AreEqual(1, reopened.Count);
            Assert.AreEqual(10, reopened.SizeBytes);
            Assert.IsNull(await AcquireAndReleaseAsync(reopened, "a"));
            Assert.IsNotNull(await AcquireAndReleaseAsync(reopened, "b"));
            Assert.IsFalse(File.Exists(orphanPath));
            Assert.IsFalse(File.Exists(interruptedDownloadPath));
        }

        [Test]
        public async Task TryAcquire_FileDeletedAtRuntime_DropsEntry()
        {
            var cache = await CreateCacheAsync();
            string path;
            using (var lease = await FileCacheTestUtils.CommitBytesAsync(cache, "a", 10))
            {
                path = lease.Path;
            }

            File.Delete(path);

            Assert.IsNull(await cache.TryAcquireAsync("a"));
            Assert.AreEqual(0, cache.Count);
            Assert.AreEqual(0, cache.SizeBytes);
        }

        [Test]
        public async Task Initialize_CorruptManifest_StartsEmptyAndDeletesFiles()
        {
            var cache = await CreateCacheAsync();
            string path;
            using (var lease = await FileCacheTestUtils.CommitBytesAsync(cache, "a", 10))
            {
                path = lease.Path;
            }

            cache.Dispose();
            File.WriteAllText(Path.Combine(_rootDirectory, "index.json"), "{ this is not json");

            var reopened = await CreateCacheAsync();

            Assert.AreEqual(0, reopened.Count);
            Assert.IsFalse(File.Exists(path));
        }

        [Test]
        public async Task Initialize_OverLimit_EvictsLeastRecentlyUsed()
        {
            var cache = await CreateCacheAsync(maxSizeBytes: 100);
            (await FileCacheTestUtils.CommitBytesAsync(cache, "old", 10)).Dispose();
            _clock.Advance(TimeSpan.FromMinutes(1));
            (await FileCacheTestUtils.CommitBytesAsync(cache, "new", 10)).Dispose();
            cache.Dispose();

            var reopened = await CreateCacheAsync(maxSizeBytes: 15); // e.g. the limit was lowered in an update

            Assert.AreEqual(1, reopened.Count);
            Assert.IsNotNull(await AcquireAndReleaseAsync(reopened, "new"));
        }

        private async Task<IFileCache> CreateCacheAsync(long maxSizeBytes = 1000)
        {
            var cache = new FileCache(FileCacheTestUtils.CreateOptions(_rootDirectory, maxSizeBytes), _clock);
            _caches.Add(cache);
            await cache.InitializeAsync();
            return cache;
        }

        private static async Task<string> AcquireAndReleaseAsync(IFileCache cache, string key)
        {
            using (var lease = await cache.TryAcquireAsync(key))
            {
                return lease?.Path;
            }
        }
    }
}
