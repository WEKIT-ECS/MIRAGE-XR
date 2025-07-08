using System;
using System.ComponentModel;
using System.Linq;
using ReadyPlayerMe.Core;
using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    ///     This class is responsible for creating buttons to select body shapes and creating buttons for each shape.
    ///     It extends the functionality of SelectionElement and adds selectable bodyshapes and IDs for them.
    /// </summary>
    public class BodyShapeSelectionElement : SelectionElement
    {
        [SerializeField] private AssetBodyShape[] availableBodyshapes;


        public void LoadAndCreateButtons()
        {
            AddBodyShapeIds();
            CreateButtons(availableBodyshapes.ToArray(), (button, asset) =>
            {
                button.SetIcon(asset.image);
            });
        }

        public void SetAssetSelected(AvatarProperties avatarProperties)
        {
            if (!avatarProperties.Assets.ContainsKey(AssetType.BodyShape))
            {
                SetButtonSelected(BodyShape.Average.GetDescription());
                return;
            }
            var assetId = avatarProperties.Assets[AssetType.BodyShape] as string;
            if (string.IsNullOrEmpty(assetId))
            {
                Debug.Log($"Asset id is null or empty {assetId} on type BodyShape");
                return;
            }
            SetButtonSelected(assetId);
        }

        private void AddBodyShapeIds()
        {
            availableBodyshapes = availableBodyshapes.Select(bodyShape => new AssetBodyShape
            {
                bodyShape = bodyShape.bodyShape,
                AssetType = bodyShape.AssetType,
                image = bodyShape.image,
                Id = bodyShape.bodyShape.GetDescription()
            }).ToArray();
        }
    }
}
