using System;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using MirageXR.View;
using UnityEngine;

namespace MirageXR
{
    [CreateAssetMenu(fileName = "MirageXRAssetsBundle", menuName = "MirageXR/AssetsBundle", order = 1)]
    public class AssetsBundle : ScriptableObject
    {
        [Serializable]
        public class ContentAssetItem
        {
            public ContentType ContentType;
            public ContentView ContentView;
        }

        public ContentView DefaultContentView;
        [SerializeField] private ContentAssetItem[] _contentAssets;

        public ContentView GetContentView(ContentType contentType)
        {
            foreach (var item in _contentAssets)
            {
                if (item.ContentType == contentType)
                {
                    return item.ContentView;
                }
            }

            AppLog.LogError("No content view found for the content type: " + contentType);
            return DefaultContentView;
        }
    }
}
