using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempStart : MonoBehaviour
{
    [SerializeField] private GameObject tempStart;
    [Serializable]
    public class LoadObject
    {
        public GameObject _prefab;
        public Transform _pathToLoad;
    }

    [SerializeField] private LoadObject[] oldPrefab;
    [SerializeField] private LoadObject[] newPrefab;

    public void OldDesignClicked()
    {
        if (oldPrefab != null)
        {
            foreach (var obj in oldPrefab)
            {
                InstantiateObject(obj);
                Destroy(tempStart);
            }
        }
    }

    public void NewDesignClicked()
    {
        if (newPrefab != null)
        {
            foreach (var obj in newPrefab)
            {
                InstantiateObject(obj);
                Destroy(tempStart);
            }
        }
    }

    private static void InstantiateObject(LoadObject loadObject)
    {
        if (loadObject._pathToLoad)
        {
            Instantiate(loadObject._prefab, loadObject._pathToLoad);
        }
        else
        {
            Instantiate(loadObject._prefab);
        }
    }
}
