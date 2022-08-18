using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class I3Button : LittleBit
{

    public override void Init()
    {
        if (IsActive)
        {
            HasPower = true;
        }
    }


    private void ToggleButton()
    {
        IsActive = !IsActive;
        HasPower = IsActive;
    }

}
