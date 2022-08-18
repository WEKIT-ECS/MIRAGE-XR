using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LittleBit : MonoBehaviour
{
    public bool HasPower
    {
        get; set;
    }

    public bool IsActive
    {
        get; set;
    }

    public abstract void Init();
}
