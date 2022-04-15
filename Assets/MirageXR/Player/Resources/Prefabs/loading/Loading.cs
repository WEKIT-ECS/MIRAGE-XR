using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class Loading : MonoBehaviour
    {

        public static Loading Instance;

        void Start()
        {
            if (!Instance)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);

            LoadingVisibility(false);

            transform.SetParent(Camera.main.transform);
        }


        public void LoadingVisibility(bool value)
        {
            gameObject.SetActive(value);
        }

    }

}
