using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MirageXR.FileCache;

namespace Tests
{
    internal sealed class FakeClock : IClock
    {
        public DateTime UtcNow { get; set; } = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public void Advance(TimeSpan timeSpan)
        {
            UtcNow += timeSpan;
        }
    }

    /// <summary>
    /// Fake fetcher that writes <see cref="Content"/> to the destination and returns <see cref="Result"/>.
    /// If <see cref="Gate"/> is set, the fetch waits for it (cancellable) after writing a partial file.
    /// </summary>
    internal sealed class FakeFetcher : IRemoteFileFetcher
    {
        public List<FetchRequest> Requests { get; } = new List<FetchRequest>();

        public byte[] Content { get; set; } = { 1, 2, 3 };

        public FetchResult Result { get; set; } = FetchResult.Ok();

        public TaskCompletionSource<bool> Gate { get; set; }

        public async Task<FetchResult> FetchToFileAsync(FetchRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (Gate != null)
            {
                File.WriteAllBytes(request.DestinationPath, new byte[] { 42 }); // partial download
                var cancelled = new TaskCompletionSource<bool>();
                using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
                {
                    await Task.WhenAny(Gate.Task, cancelled.Task);
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            if (Result.Status == FetchStatus.Ok)
            {
                File.WriteAllBytes(request.DestinationPath, Content);
            }
            else
            {
                File.WriteAllBytes(request.DestinationPath, new byte[] { 0xEE }); // e.g. an error body
            }

            return Result;
        }
    }

    internal static class FileCacheTestUtils
    {
        public static string CreateTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "MirageXR-FileCacheTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        public static void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (IOException)
            {
                // best effort
            }
        }

        public static FileCacheOptions CreateOptions(string rootDirectory, long maxSizeBytes = 1000, TimeSpan? timeToLive = null)
        {
            return new FileCacheOptions("Test", rootDirectory, maxSizeBytes, timeToLive ?? TimeSpan.FromDays(7))
            {
                // saves happen explicitly (Dispose/Clear/Flush), so no background write races with the assertions
                ManifestSaveDelay = TimeSpan.FromHours(1),
            };
        }

        /// <summary>Writes a temp file of the given size into the cache and commits it.</summary>
        public static async Task<FileLease> CommitBytesAsync(IFileCache cache, string key, int size, byte fill = 1, string eTag = null)
        {
            var tempPath = await cache.CreateTempFilePathAsync(".bin");
            var bytes = new byte[size];
            for (var i = 0; i < size; i++)
            {
                bytes[i] = fill;
            }

            File.WriteAllBytes(tempPath, bytes);
            return await cache.CommitAsync(key, tempPath, eTag, null);
        }

        /// <summary>Polls until the condition holds (file deletions on lease release run asynchronously).</summary>
        public static async Task<bool> WaitUntilAsync(Func<bool> condition, int timeoutMilliseconds = 3000)
        {
            var stopwatch = Stopwatch.StartNew();
            while (!condition())
            {
                if (stopwatch.ElapsedMilliseconds > timeoutMilliseconds)
                {
                    return false;
                }

                await Task.Delay(10);
            }

            return true;
        }

        public static string[] GetTempFiles(string rootDirectory)
        {
            var tempDirectory = Path.Combine(rootDirectory, "tmp");
            return Directory.Exists(tempDirectory) ? Directory.GetFiles(tempDirectory) : new string[0];
        }
    }
}
