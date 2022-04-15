using System;
using UnityEngine;

namespace MirageXR
{
    [Serializable]
    public struct GhostDataFrame
    {
        public Pose head;
        public Pose leftHand;
        public Pose rightHand;
        public Pose upperSpine;
        public Pose lowerSpine;
    }
}