using MirageXR;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tests
{
    public class HololensSensorDataFrameTests
    {
        [Test]
        public void Constructor_AssignsParametersCorrectly()
        {
            Vector3 hp = new Vector3(1, 2, 3);
            Vector3 gD = new Vector3(4, 5, 6);
            bool cH = true;
            Vector3 haP1 = new Vector3(7, 8, 9);
            Vector3 haP2 = new Vector3(10, 11, 12);
            float tS = 1f;
            HololensSensorDataFrame hsdf = new HololensSensorDataFrame(hp, gD, cH, haP1, haP2, tS);
            Assert.AreEqual(hp, hsdf.HeadPosition);
            Assert.AreEqual(gD, hsdf.GazeDirection);
            Assert.AreEqual(cH, hsdf.CastHit);
            Assert.AreEqual(haP1, hsdf.HandPosition1);
            Assert.AreEqual(haP2, hsdf.HandPosition2);
            Assert.AreEqual(tS, hsdf.TimeStamp);
        }
    }
}
