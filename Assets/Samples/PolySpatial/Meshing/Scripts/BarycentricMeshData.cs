using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace PolySpatial.Samples
{
    public class BarycentricMeshData : MonoBehaviour
    {
        [SerializeField]
        ARMeshManager m_MeshManager;

        public ARMeshManager meshManager
        {
            get => m_MeshManager;
            set => m_MeshManager = value;
        }

        [SerializeField]
        BarycentricDataBuilder m_DataBuilder;

        public BarycentricDataBuilder dataBuilder
        {
            get => m_DataBuilder;
            set => m_DataBuilder = value;
        }

        List<MeshFilter> m_AddedMeshes = new List<MeshFilter>();
        List<MeshFilter> m_UpdatedMeshes = new List<MeshFilter>();

        void OnEnable()
        {
            m_MeshManager.meshesChanged += MeshManagerOnMeshesChanged;
        }

        void OnDisable()
        {
            m_MeshManager.meshesChanged -= MeshManagerOnMeshesChanged;
        }

        void MeshManagerOnMeshesChanged(ARMeshesChangedEventArgs obj)
        {
            m_AddedMeshes = obj.added;
            m_UpdatedMeshes = obj.updated;


            foreach (MeshFilter filter in m_AddedMeshes)
            {
                m_DataBuilder.GenerateData(filter.mesh);
            }

            foreach (MeshFilter filter in m_UpdatedMeshes)
            {
                m_DataBuilder.GenerateData(filter.sharedMesh);
            }
        }
    }
}
