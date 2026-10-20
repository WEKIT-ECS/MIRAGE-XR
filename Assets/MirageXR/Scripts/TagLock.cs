using MirageXR;
using UnityEngine;

public class TagLock : MonoBehaviour 
{
    public void LockPosition()
    {
        var radial = GetComponent<RadialView>();
        if (radial != null)
        {
            radial.enabled = false;
        }
        Maggie.Ok();
    }

    public void ReleasePosition()
    {
        var radial = GetComponent<RadialView>();
        if (radial != null)
        {
            radial.enabled = true;
        }
        Maggie.Ok();
    }
}
