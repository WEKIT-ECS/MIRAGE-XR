using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class PlayerStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StateManager manager = TrackerManager.Instance.GetStateManager();
        manager.ReassociateTrackables();
        VuforiaBehaviour.Instance.enabled = true;
    }
}
