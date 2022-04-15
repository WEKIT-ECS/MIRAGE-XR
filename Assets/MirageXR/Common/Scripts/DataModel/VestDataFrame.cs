using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Xml.Serialization;
using System.IO;

namespace MirageXR
{

    [Serializable]
    public class Imuframe
    {
        public float ax;
        public float ay;
        public float az;

        public float gx;
        public float gy;
        public float gz;

        public float mx;
        public float my;
        public float mz;

        public float q0;
        public float q1;
        public float q2;
        public float q3;
    }

    /// <summary>
    /// The serializable custom class in which the gathered data will be stored,
    /// one instance for each frame (sensor updateInterval decides on framerate).
    /// </summary>
    [Serializable]
    public class VestDataFrame
    {

        public string client;
        public int time;
        public List<Imuframe> imus;
        public int gsr;

        public int t;
        public int h;

    } // VestDataFrame Class

} // namespace
