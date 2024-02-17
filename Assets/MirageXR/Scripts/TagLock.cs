using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using MirageXR;
using UnityEngine;

public class TagLock : MonoBehaviour {

    public void LockPosition()
    {
        GetComponent<RadialView>().enabled = false;
        Maggie.Ok();
    }

    public void ReleasePosition()
    {
        GetComponent<RadialView>().enabled = true;
        Maggie.Ok();
    }
}
