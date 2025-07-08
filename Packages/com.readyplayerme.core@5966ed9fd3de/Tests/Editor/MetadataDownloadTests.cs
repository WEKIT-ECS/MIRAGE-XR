using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ReadyPlayerMe.Core.Tests
{
    public class MetadataDownloadTests
    {
        private const string WRONG_JSON_URL =
            "https://gist.githubusercontent.com/srcnalt/2ca44ce804ac28ce8722a93dca3635c9/raw";

        private static async Task DownloadAndCheckMetadata(string url, BodyType bodyType, OutfitGender outfitGender, string skinTone = "")
        {
            AvatarMetadata metadata;

            var metadataDownloader = new MetadataDownloader();
            try
            {
                metadata = await metadataDownloader.Download(url);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                throw;
            }

            Assert.AreEqual(bodyType, metadata.BodyType);
            Assert.AreEqual(outfitGender, metadata.OutfitGender);
            //TODO check skinTone once it has been added to AvatarMetadata class 
            //Assert.AreEqual(skinTone, metadata.SkinTone);
        }

        [TearDown]
        public void Cleanup()
        {
            TestUtils.DeleteAvatarDirectoryIfExists(TestAvatarData.DefaultAvatarUri.Guid, true);
        }

        [Test]
        public async Task Download_Metadata_Into_File()
        {
            AvatarMetadata metadata;

            var metadataDownloader = new MetadataDownloader();
            try
            {
                var url = TestAvatarData.GetAvatarApiJsonUrl(BodyType.FullBody, OutfitGender.Feminine);
                metadata = await metadataDownloader.Download(url);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                throw;
            }

            metadata.SaveToFile(TestAvatarData.DefaultAvatarUri.Guid, TestUtils.TestJsonFilePath);

            Assert.AreEqual(true, File.Exists(TestUtils.TestJsonFilePath));
        }

        [Test]
        public async Task Download_Metadata_Into_Memory()
        {
            var metadataDownloader = new MetadataDownloader();
            try
            {
                var url = TestAvatarData.GetAvatarApiJsonUrl(BodyType.FullBody, OutfitGender.Feminine);
                await metadataDownloader.Download(url);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            Assert.Pass();
        }

        [Test]
        public async Task Fail_Download_Metadata_Into_Memory_With_Wrong_JSON()
        {
            var metadataDownloader = new MetadataDownloader();
            try
            {
                await metadataDownloader.Download(WRONG_JSON_URL);
            }
            catch (CustomException exception)
            {
                Assert.AreEqual(FailureType.MetadataParseError, exception.FailureType);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public async Task Downloaded_Metadata_UpdatedAt_Is_Not_Default_Value()
        {
            AvatarMetadata metadata;
            var metadataDownloader = new MetadataDownloader();
            try
            {
                var url = TestAvatarData.GetAvatarApiJsonUrl(BodyType.FullBody, OutfitGender.Feminine);
                metadata = await metadataDownloader.Download(url);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            Assert.AreNotEqual(default(DateTime), metadata.UpdatedAt);
        }


        [Test]
        public void Check_Is_Metadata_Updated()
        {
            var oldMetaData = new AvatarMetadata();
            var newMetadata = new AvatarMetadata();
            newMetadata.UpdatedAt = DateTime.Now;
            Assert.True(AvatarMetadata.IsUpdated(newMetadata, oldMetaData));
        }

        [Test]
        public void Check_Is_Metadata_Not_Updated()
        {
            var oldMetaData = new AvatarMetadata();
            var newMetadata = new AvatarMetadata();
            Assert.False(AvatarMetadata.IsUpdated(newMetadata, oldMetaData));
        }

        [Test]
        public void Save_Metadata_To_File()
        {
            var avatarMetadata = new AvatarMetadata();
            if (File.Exists(TestUtils.TestJsonFilePath))
            {
                File.Delete(TestUtils.TestJsonFilePath);
            }
            avatarMetadata.SaveToFile(TestAvatarData.DefaultAvatarUri.Guid, TestUtils.TestJsonFilePath);
            Assert.True(File.Exists(TestUtils.TestJsonFilePath));
        }

        [Test]
        public void Load_Metadata_From_File()
        {
            var avatarMetadata = new AvatarMetadata();
            avatarMetadata.SaveToFile(TestAvatarData.DefaultAvatarUri.Guid, TestUtils.TestJsonFilePath);
            AvatarMetadata metadata = AvatarMetadata.LoadFromFile(TestUtils.TestJsonFilePath);
            Assert.AreNotSame(new AvatarMetadata(), metadata);
        }

        [Test]
        public async Task Check_Metadata_Feminine_Full_Body()
        {
            var url = TestAvatarData.GetAvatarApiJsonUrl(BodyType.FullBody, OutfitGender.Feminine);
            await DownloadAndCheckMetadata(url, BodyType.FullBody, OutfitGender.Feminine);
        }

        [Test]
        public async Task Check_Metadata_Masculine_Full_Body()
        {
            var url = TestAvatarData.GetAvatarApiJsonUrl(BodyType.FullBody, OutfitGender.Masculine);
            await DownloadAndCheckMetadata(url, BodyType.FullBody, OutfitGender.Masculine);
        }

        [Test]
        public async Task Check_Metadata_Feminine_Half_Body()
        {
            var url = TestAvatarData.GetAvatarApiJsonUrl(BodyType.HalfBody, OutfitGender.Feminine);
            await DownloadAndCheckMetadata(url, BodyType.HalfBody, OutfitGender.Feminine);
        }

        [Test]
        public async Task Check_Metadata_Masculine_Half_Body()
        {
            var url = TestAvatarData.GetAvatarApiJsonUrl(BodyType.HalfBody, OutfitGender.Masculine);
            await DownloadAndCheckMetadata(url, BodyType.HalfBody, OutfitGender.Masculine);
        }

        [Test]
        public async Task Check_Models_Metadata_Feminine_Full_Body()
        {
            var url = TestAvatarData.GetAvatarModelsJsonUrl(BodyType.FullBody, OutfitGender.Feminine);
            await DownloadAndCheckMetadata(url, BodyType.FullBody, OutfitGender.Feminine);
        }

        [Test]
        public async Task Check_Models_Metadata_Masculine_Full_Body()
        {
            var url = TestAvatarData.GetAvatarModelsJsonUrl(BodyType.FullBody, OutfitGender.Masculine);
            await DownloadAndCheckMetadata(url, BodyType.FullBody, OutfitGender.Masculine);
        }

        [Test]
        public async Task Check_Models_Metadata_Feminine_Half_Body()
        {
            var url = TestAvatarData.GetAvatarModelsJsonUrl(BodyType.HalfBody, OutfitGender.Feminine);
            await DownloadAndCheckMetadata(url, BodyType.HalfBody, OutfitGender.Feminine);
        }

        [Test]
        public async Task Check_Models_Metadata_Masculine_Half_Body()
        {
            var url = TestAvatarData.GetAvatarModelsJsonUrl(BodyType.HalfBody, OutfitGender.Masculine);
            await DownloadAndCheckMetadata(url, BodyType.HalfBody, OutfitGender.Masculine);
        }
    }
}
