using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    [Serializable]
    public class HandsDataFrame
    {
        public Vector3 LeftHandPos;
        public Quaternion LeftHandRot;

        public Vector3 RightHandPos;
        public Quaternion RightHandRot;

    }
}
