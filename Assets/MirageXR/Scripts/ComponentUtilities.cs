using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public static class ComponentUtilities
    {
        public static T GetOrFetchComponent<T>(MonoBehaviour monoBehaviour, ref T field) where T : Component
        {
            if (field == null)
            {
                field = monoBehaviour.GetComponent<T>();
            }
            return field;
        }
    }
}
