using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTestManager : MonoBehaviour
{
    public bool ObjectExists(string name)
    {
        return GameObject.Find(name);
    }
}
