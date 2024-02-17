using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterView : MonoBehaviour
{
    [SerializeField] private GameObject registerPrefab;

    public void OnClickBack()
    {
        Destroy(registerPrefab);
    }
}
