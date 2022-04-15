using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MirageXR
{
    /// <summary>
    /// Positions a transform at the height where the application assumes the floor to be
    /// Can be used for debuggin and as a reference point for other scripts to place objects based on the floor height
    /// </summary>
    public class FakeFloor : MonoBehaviour
    {
        // Gets the floor height from the UIOrigin that places the aura and puts the attached transform on the found floor height
        private void Update()
        {
            transform.position = new Vector3(transform.position.x, UIOrigin.Instance.CurrentFloorYPosition(), transform.position.z);
        }
    }

}
