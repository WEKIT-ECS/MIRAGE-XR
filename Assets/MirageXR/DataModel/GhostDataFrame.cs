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

        public Pose leftThumbTip;
        public Pose leftIndexTip;
        public Pose leftMiddleTip;
        public Pose leftRingTip;
        public Pose leftPinkyTip;

        public Pose rightThumbTip;
        public Pose rightIndexTip;
        public Pose rightMiddleTip;
        public Pose rightRingTip;
        public Pose rightPinkyTip;

    }
}