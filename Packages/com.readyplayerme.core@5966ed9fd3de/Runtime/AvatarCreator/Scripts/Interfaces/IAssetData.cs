using UnityEngine.Scripting;

namespace ReadyPlayerMe.AvatarCreator
{
    public interface IAssetData
    {
        [Preserve]
        public string Id { get; set; }
        public AssetType AssetType { get; set; }
    }
}
