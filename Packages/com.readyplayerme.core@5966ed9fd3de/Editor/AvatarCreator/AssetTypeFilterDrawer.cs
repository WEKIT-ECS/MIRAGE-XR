using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator.Editor
{
    [CustomPropertyDrawer(typeof(AssetTypeFilterAttribute))]
    public class AssetTypeFilterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var assetTypeAttribute = attribute as AssetTypeFilterAttribute;

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                EditorGUI.BeginProperty(position, label, property);

                EditorGUI.BeginChangeCheck();

                // Get the current enum value
                var currentEnumValue = (AssetType) property.enumValueIndex;

                var filteredEnumNames = new List<string>();
                foreach (var enumName in Enum.GetNames(typeof(AssetType)))
                {
                    var enumFieldInfo = typeof(AssetType).GetField(enumName);
                    var enumAttribute = (AssetTypeFilterAttribute) Attribute.GetCustomAttribute(enumFieldInfo, typeof(AssetTypeFilterAttribute));
                    if (enumAttribute == null) continue;

                    var filter = (AssetFilter) Enum.Parse(typeof(AssetFilter), enumAttribute.filter.ToString());
                    if (filter == assetTypeAttribute?.filter)
                    {
                        filteredEnumNames.Add(enumName);
                    }
                }

                // Display the dropdown with filtered enum values
                var newIndex = EditorGUI.Popup(position, label.text, Array.IndexOf(filteredEnumNames.ToArray(), currentEnumValue.ToString()), filteredEnumNames.ToArray());

                // Set the new enum value if it has changed
                if (EditorGUI.EndChangeCheck())
                {
                    property.enumValueIndex = (int) Enum.Parse(typeof(AssetType), filteredEnumNames[newIndex]);
                }

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use AssetType with Enum.");
            }
        }

    }
}
