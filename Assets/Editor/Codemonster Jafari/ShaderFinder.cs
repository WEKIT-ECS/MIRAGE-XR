// -----------------------------------------------------------------------
// Copyright (c) 2023 Codemonster Jafari (Abbas Jafari)
// All rights reserved.
// 
// This software and associated documentation files (the "Software") are 
// provided for use in the project "The silent mystery" and associated works.
// Redistribution, modification, or use in other projects without express 
// written permission from the copyright holder is strictly prohibited.
// -----------------------------------------------------------------------


using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;


namespace Codemonster
{
    public class ShaderWindow : EditorWindow
    {
        private class MaterialShaderInfo
        {
            public Material Material;
            public string ShaderName;
        }

        private readonly List<MaterialShaderInfo> _infoList = new ();
        private Vector2 _scrollPos;
        private readonly HashSet<string> _addedShaderNames = new ();

        [MenuItem("Tools/Codemonster Jafari/Find Shaders in Project")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ShaderWindow));
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("This window shows all materials in the project and the shaders they use.");

            if (GUILayout.Button("Add Shaders to Addressables"))
            {
                AddShadersToAddressables();
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var info in _infoList)
            {
                EditorGUILayout.ObjectField("Material: ", info.Material, typeof(Material), false);
                EditorGUILayout.LabelField("Shader Name: ", info.ShaderName);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnEnable()
        {
            _infoList.Clear();

            string[] guides = AssetDatabase.FindAssets("t:material", null);
            foreach (var guid in guides)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (material != null)
                {
                    var info = new MaterialShaderInfo
                    {
                        Material = material,
                        ShaderName = material.shader.name
                    };

                    _infoList.Add(info);
                }
            }
        }

        private void AddShadersToAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var defaultGroup = settings.DefaultGroup;

            foreach (var info in _infoList)
            {
                if (info.Material.shader != null && !_addedShaderNames.Contains(info.ShaderName))
                {
                    var shaderPath = AssetDatabase.GetAssetPath(info.Material.shader);
                    var shaderGuid = AssetDatabase.AssetPathToGUID(shaderPath);

                    var alreadyInGroup = false;
                    foreach (var entry in defaultGroup.entries)
                    {
                        if (entry.guid == shaderGuid)
                        {
                            alreadyInGroup = true;
                            break;
                        }
                    }

                    if (!alreadyInGroup)
                    {
                        settings.CreateOrMoveEntry(shaderGuid, defaultGroup);
                        _addedShaderNames.Add(info.ShaderName);
                    }
                }
            }
        }
    }
}