// -----------------------------------------------------------------------
// Copyright (c) 2023 Codemonster Jafari (Abbas Jafari)
// All rights reserved.
// 
// This software and associated documentation files (the "Software") are 
// provided for use in the project "The silent mystery" and associated works.
// Redistribution, modification, or use in other projects without express 
// written permission from the copyright holder is strictly prohibited.
// -----------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Codemonster
{
    public class FindMaterialsByShader : EditorWindow
    {
        private Shader targetShader;
        private List<Material> foundMaterials = new List<Material>();

        [MenuItem("Tools/Codemonster Jafari/Find Materials by Shader")]
        public static void ShowWindow()
        {
            GetWindow<FindMaterialsByShader>("Find Materials by Shader");
        }

        private void OnGUI()
        {
            GUILayout.Label("Find Materials by Shader", EditorStyles.boldLabel);

            targetShader = EditorGUILayout.ObjectField("Target Shader:", targetShader, typeof(Shader), false) as Shader;

            if (GUILayout.Button("Find Materials"))
            {
                FindMaterials();
            }

            GUILayout.Label("Found Materials:");

            foreach (Material mat in foundMaterials)
            {
                EditorGUILayout.ObjectField(mat, typeof(Material), false);
            }
        }

        private void FindMaterials()
        {
            foundMaterials.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Material");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (mat.shader == targetShader)
                {
                    foundMaterials.Add(mat);
                }
            }
        }
    }

}