using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ReadyPlayerMe.Core.Tests
{
    public class AvatarDownloadTests
    {
        [TearDown]
        public void Cleanup()
        {
            TestUtils.DeleteAvatarDirectoryIfExists(TestAvatarData.DefaultAvatarUri.Guid, true);
            TestUtils.DeleteAvatarDirectoryIfExists(TestUtils.TEST_WRONG_GUID, true);
        }

        [Test]
        public async Task Download_Avatar_Into_File()
        {
            byte[] bytes;
            var avatarDownloader = new AvatarDownloader();

            try
            {
                bytes = await avatarDownloader.DownloadIntoFile(TestAvatarData.DefaultAvatarUri.ModelUrl, TestAvatarData.DefaultAvatarUri.LocalModelPath);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                throw;
            }

            Assert.NotNull(bytes);
            Assert.IsTrue(File.Exists(TestAvatarData.DefaultAvatarUri.LocalModelPath));
        }

        [Test]
        public async Task Download_Avatar_Into_Memory()
        {
            if (File.Exists(TestAvatarData.DefaultAvatarUri.LocalModelPath))
            {
                File.Delete(TestAvatarData.DefaultAvatarUri.LocalModelPath);
            }
            byte[] bytes;

            var avatarDownloader = new AvatarDownloader();
            try
            {
                bytes = await avatarDownloader.DownloadIntoMemory(TestAvatarData.DefaultAvatarUri.ModelUrl);
            }
            catch (CustomException exception)
            {
                Assert.Fail(exception.Message);
                throw;
            }

            Assert.NotNull(bytes);
            Assert.IsFalse(File.Exists(TestAvatarData.DefaultAvatarUri.LocalModelPath));
        }


        [Test]
        public async Task Fail_Download_Avatar_Into_File()
        {
            var avatarDownloader = new AvatarDownloader();

            try
            {
                await avatarDownloader
                    .DownloadIntoFile(TestAvatarData.WrongUri.ModelUrl, TestAvatarData.WrongUri.LocalModelPath);

            }
            catch (CustomException exception)
            {
                Assert.AreEqual(FailureType.ModelDownloadError, exception.FailureType);
                return;
            }

            Assert.Fail("Download into file should fail.");
        }

        [Test]
        public async Task Fail_Download_Avatar_Into_Memory()
        {
            var avatarDownloader = new AvatarDownloader();

            try
            {
                await avatarDownloader.DownloadIntoMemory(TestAvatarData.WrongUri.ModelUrl);
            }
            catch (CustomException exception)
            {
                Assert.AreEqual(FailureType.ModelDownloadError, exception.FailureType);
                return;
            }

            Assert.Fail("Download should fail for wrong uri.");
        }

        [Test]
        public async Task Check_Progress_Download_Avatar_Into_File()
        {
            var currentProgress = 0f;
            var cumulativeProgress = 0f;

            var avatarDownloader = new AvatarDownloader();
            avatarDownloader.ProgressChanged = progress =>
            {
                currentProgress = progress;
                cumulativeProgress += progress;
            };

            try
            {
                await avatarDownloader.DownloadIntoFile(TestAvatarData.DefaultAvatarUri.ModelUrl, TestAvatarData.DefaultAvatarUri.LocalModelPath);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                throw;
            }

            Assert.AreEqual(1, currentProgress);
            Assert.GreaterOrEqual(cumulativeProgress, 1);
        }
    }
}
