using System;
using ReadyPlayerMe.Core;
using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator
{
    [Serializable]
    public struct AssetBodyShape : IAssetData
    {
        public BodyShape bodyShape;
        public Texture image;
        public string Id { get; set; }
        public AssetType AssetType { get => AssetType.BodyShape; set { } }
    }
}
