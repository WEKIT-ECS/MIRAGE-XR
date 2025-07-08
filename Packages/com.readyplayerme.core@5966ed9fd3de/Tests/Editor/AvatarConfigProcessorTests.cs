using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ReadyPlayerMe.Core.Tests
{
    public class AvatarConfigProcessorTests
    {
        private const string TEXTURECHANNELS_EXPECTED_ALL = "&textureChannels=baseColor,normal,metallicRoughness,emissive,occlusion";
        private const string TEXTURECHANNELS_EXPECTED_NONE = "&textureChannels=none";
        private const string MORPHTARGETS_EXPECTED_DEFAULT = "mouthOpen,mouthSmile";
        private const string MORPHTARGETS_EXPECTED_NONE = "none";
        private const string AVATAR_QUERY_PARAMS_ACTUAL =
            "pose=T&lod=0&textureAtlas=none&textureSizeLimit=1024&textureChannels=baseColor,normal,metallicRoughness,emissive,occlusion&useHands=false&useDracoMeshCompression=false&useMeshOptCompression=false";
        private readonly string[] morphTargetsDefault = { "mouthOpen", "mouthSmile" };
        private readonly string[] morphTargetsNone = { "none" };
        private readonly TextureChannel[] textureChannelsAll =
        {
            TextureChannel.BaseColor,
            TextureChannel.Normal,
            TextureChannel.MetallicRoughness,
            TextureChannel.Emissive,
            TextureChannel.Occlusion
        };

        [Test]
        public void Process_Avatar_Configuration()
        {
            var avatarConfig = ScriptableObject.CreateInstance<AvatarConfig>();
            var queryParams = AvatarConfigProcessor.ProcessAvatarConfiguration(avatarConfig);
            Debug.Log($"{queryParams}");
            Assert.AreEqual(AVATAR_QUERY_PARAMS_ACTUAL, queryParams);
        }

        [Test]
        public void Process_Texture_Size_Limit_Is_Equal()
        {
            var size = 2;
            var processedSize = AvatarConfigProcessor.ProcessTextureSizeLimit(size);
            Assert.AreEqual(processedSize, size);
        }

        [Test]
        public void Process_Texture_Size_Limit_Is_Not_Equal()
        {
            var size = 1;
            var processedSize = AvatarConfigProcessor.ProcessTextureSizeLimit(size);
            Assert.AreNotEqual(processedSize, size);
        }

        [Test]
        public void Process_Texture_Channels_All()
        {
            var textureChanelParams = $"&{AvatarAPIParameters.TEXTURE_CHANNELS}=";
            textureChanelParams += AvatarConfigProcessor.ProcessTextureChannels(textureChannelsAll);
            Assert.AreEqual(textureChanelParams, TEXTURECHANNELS_EXPECTED_ALL);
        }

        [Test]
        public void Process_Texture_Channels_None()
        {
            var textureChanelParams = $"&{AvatarAPIParameters.TEXTURE_CHANNELS}=";
            textureChanelParams += AvatarConfigProcessor.ProcessTextureChannels(new List<TextureChannel>());
            Assert.AreEqual(textureChanelParams, TEXTURECHANNELS_EXPECTED_NONE);
        }

        [Test]
        public void Process_Morph_Targets_Default()
        {
            Assert.AreEqual(AvatarConfigProcessor.CombineMorphTargetNames(morphTargetsDefault), MORPHTARGETS_EXPECTED_DEFAULT);
        }

        [Test]
        public void Process_Morph_Targets_None()
        {
            Assert.AreEqual(AvatarConfigProcessor.CombineMorphTargetNames(morphTargetsNone), MORPHTARGETS_EXPECTED_NONE);
        }

        [Test]
        public void Process_Morph_Targets_Empty()
        {
            Assert.IsEmpty(AvatarConfigProcessor.CombineMorphTargetNames(new List<string>()));
        }
    }
}
