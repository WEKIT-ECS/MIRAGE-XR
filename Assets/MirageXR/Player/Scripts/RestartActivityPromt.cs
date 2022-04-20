using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MirageXR;
public class RestartActivityPromt : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    public void ResartActivity(){
        activityManager.ActivateFirstAction();
        Destroy(gameObject);
    }

    public void ContinueActivity()
    {
        Destroy(gameObject);
    }
}
