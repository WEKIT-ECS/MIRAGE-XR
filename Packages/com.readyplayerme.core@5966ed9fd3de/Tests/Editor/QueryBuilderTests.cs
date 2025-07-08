using NUnit.Framework;

namespace ReadyPlayerMe.Core.Tests
{
    public class QueryBuilderTests
    {
        private const string ATLAS_AND_MORPHS = "textureAtlas=1024&morphTargets=mouthSmile,ARKit";
        private const string QUALITY_LOW_MESH_LOD = "quality=low&lod=0";
        private const string LOW_QUALITY = "low";
        private const string ATLAS_1024 = "1024";
        private const string LOD_0 = "0";

        private readonly string[] morphTargetsDefault = { "mouthSmile", "ARKit" };

        [Test]
        public void Low_Quality_Lod_0()
        {
            var queryBuilder = new QueryBuilder();
            queryBuilder.AddKeyValue(AvatarAPIParameters.QUALITY, LOW_QUALITY);
            queryBuilder.AddKeyValue(AvatarAPIParameters.LOD, LOD_0);
            Assert.AreEqual(queryBuilder.Query, QUALITY_LOW_MESH_LOD);
        }

        [Test]
        public void Texture_Atlas_1024_MorphTargets()
        {
            var queryBuilder = new QueryBuilder();
            queryBuilder.AddKeyValue(AvatarAPIParameters.TEXTURE_ATLAS, ATLAS_1024);
            queryBuilder.AddKeyValue(AvatarAPIParameters.MORPH_TARGETS, AvatarConfigProcessor.CombineMorphTargetNames(morphTargetsDefault));
            Assert.AreEqual(queryBuilder.Query, ATLAS_AND_MORPHS);
        }
    }
}
