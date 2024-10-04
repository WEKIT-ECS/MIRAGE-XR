using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolySpatial.Samples
{
    public class SpawnObjects : MonoBehaviour
    {
        [SerializeField]
        SpatialUIButton m_Button;

        [SerializeField]
        List<GameObject> m_ObjectsToSpawn;

        [SerializeField]
        Transform m_SpawnPosition;

        void OnEnable()
        {
            if (m_Button)
            {
                m_Button.WasPressed += WasPressed;
            }
        }

        void OnDisable()
        {
            if (m_Button)
            {
                m_Button.WasPressed -= WasPressed;
            }
        }

        void WasPressed(string buttonText, MeshRenderer meshrenderer)
        {
            ForceSpawn();
        }

        public void ForceSpawn()
        {
            var randomObject = Random.Range(0, m_ObjectsToSpawn.Count);
            Instantiate(m_ObjectsToSpawn[randomObject], m_SpawnPosition.position, Quaternion.identity);
        }
    }
}
