using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using PolySpatial.Samples;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Object = UnityEngine.Object;

namespace Samples.PolySpatial.SwiftUI.Scripts
{
    // This is a driver MonoBehaviour that connects to SwiftUISamplePlugin.swift via
    // C# DllImport. See MeshSamplePlugin.swift for more information.
    public class MeshSwiftUIDriver : MonoBehaviour
    {
        [SerializeField]
        List<GameObject> m_ObjectsToSpawn;

        [SerializeField]
        List<Material> m_ObjectMaterials;

        [SerializeField]
        Transform m_SpawnPosition;

        [SerializeField]
        ARMeshManager m_MeshManager;

        [SerializeField]
        Material m_OcclusionMaterial;

        [SerializeField]
        Material m_WireFrameMaterial;

        [SerializeField]
        Material m_TextureMaterial;

        [SerializeField]
        LoadLevelButton m_LoadLevelButton;

        [SerializeField]
        int m_MaxSpawnedObjects = 150;

        bool m_SpawningObjects = false;
        bool m_ShowMesh = true;
        List<GameObject> m_SpawnedObjects = new List<GameObject>();
        float m_RealTimeAtSpawn = 0.0f;

        const float k_SpawnDelay = 0.25f;

        void OnEnable()
        {
            OpenSwiftUIWindow("MeshSample");
            SetNativeCallback(CallbackFromNative);
        }

        void OnDisable()
        {
            SetNativeCallback(null);
            ForceCloseWindow();
        }

        public void ForceCloseWindow()
        {
            CloseSwiftUIWindow("MeshSample");
        }

        void Update()
        {
            if (m_SpawningObjects)
            {
                if (Time.realtimeSinceStartup >= m_RealTimeAtSpawn + k_SpawnDelay)
                {
                    if(m_SpawnedObjects.Count >= m_MaxSpawnedObjects)
                    {
                        return;
                    }

                    var objectIndex = m_SpawnedObjects.Count % 2;
                    var materialIndex = m_SpawnedObjects.Count % 3;
                    var newObject = Instantiate(m_ObjectsToSpawn[objectIndex], m_SpawnPosition.position, Quaternion.identity);
                    newObject.GetComponent<MeshRenderer>().material = m_ObjectMaterials[materialIndex];
                    m_SpawnedObjects.Add(newObject);
                    m_RealTimeAtSpawn = Time.realtimeSinceStartup;
                }
            }

        }

        delegate void CallbackDelegate(string command);

        // This attribute is required for methods that are going to be called from native code
        // via a function pointer.
        [MonoPInvokeCallback(typeof(CallbackDelegate))]
        static void CallbackFromNative(string command)
        {
            // MonoPInvokeCallback methods will leak exceptions and cause crashes; always use a try/catch in these methods
            try
            {
                Debug.Log("Callback from native: " + command);

                // This could be stored in a static field or a singleton.
                // If you need to deal with multiple windows and need to distinguish between them,
                // you could add an ID to this callback and use that to distinguish windows.
                var self = Object.FindFirstObjectByType<MeshSwiftUIDriver>();

                switch (command)
                {
                    case "closed":
                        break;
                    case "showmesh":
                        self.ToggleMesh();
                        break;
                    case "occlusionMat":
                        self.SetMeshMaterial(self.m_OcclusionMaterial);
                        break;
                    case "wireframeMat":
                        self.SetMeshMaterial(self.m_WireFrameMaterial);
                        break;
                    case "textureMat":
                        self.SetMeshMaterial(self.m_TextureMaterial);
                        break;
                    case "spawnObjects":
                        self.SpawnObjects();
                        break;
                    case "deleteObjects":
                        self.DeleteObjects();
                        break;
                    case "returnToMenu":
                        self.LoadMenuScene();
                        break;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        void SpawnObjects()
        {
            m_SpawningObjects = !m_SpawningObjects;
        }

        void DeleteObjects()
        {
            foreach (var obj in m_SpawnedObjects)
            {
                Destroy(obj);
            }
            m_SpawnedObjects.Clear();
        }

        void ToggleMesh()
        {
            m_ShowMesh = !m_ShowMesh;
            if (m_ShowMesh)
            {
                m_MeshManager.enabled = true;
            }
            else
            {
                m_MeshManager.enabled = false;
                foreach(var mesh in m_MeshManager.meshes)
                {
                    mesh.gameObject.SetActive(false);
                }
            }
        }

        void SetMeshMaterial(Material mat)
        {
            foreach (var mesh in m_MeshManager.meshes)
            {
                mesh.GetComponent<MeshRenderer>().material = mat;
            }
        }

        void LoadMenuScene()
        {
            m_LoadLevelButton.Press();
        }

#if UNITY_VISIONOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void SetNativeCallback(CallbackDelegate callback);

        [DllImport("__Internal")]
        static extern void OpenSwiftUIWindow(string name);

        [DllImport("__Internal")]
        static extern void CloseSwiftUIWindow(string name);
        #else
        static void SetNativeCallback(CallbackDelegate callback) {}
        static void OpenSwiftUIWindow(string name) {}
        static void CloseSwiftUIWindow(string name) {}
        #endif

    }
}
