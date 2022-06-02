using System;
using UnityEngine;

namespace MirageXR
{

    /// <summary>
    /// The serializable custom class in which the gathered data will be stored,
    /// one instance for each frame (sensor updateInterval decides on framerate).
    /// </summary>
    [Serializable]
    public class HololensSensorDataFrame
    {
        public float TimeStamp;

        public Vector3 HandPosition1;
        public Vector3 HandPosition2;

        public Vector3 HeadPosition;
        public Vector3 GazeDirection;

        public bool CastHit;

        public string leftHand;
        public string rightHand;

        public int attentionLevel;

        public HololensSensorDataFrame(Vector3 hP, Vector3 gD, bool cH, Vector3 haP1, Vector3 haP2, float tS)
        {

            HeadPosition = hP;
            GazeDirection = gD;
            CastHit = cH;
            HandPosition1 = haP1;
            HandPosition2 = haP2;

            // this.attentionLevel = attentionLevel;
            // this.leftHand = leftHand;
            // this.rightHand = rightHand;

            TimeStamp = tS;

        } // Constructor HololensSensorDataFrame()


        } // SaveData Class

} // namespace