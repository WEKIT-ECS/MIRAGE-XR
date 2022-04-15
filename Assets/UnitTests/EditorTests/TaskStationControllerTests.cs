using Microsoft.VisualStudio.TestTools.UnitTesting;
using MirageXR;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TaskStationControllerTests
    {
        readonly TaskStationController TSC = new TaskStationController();

        [UnityTest]
        public IEnumerator ConvertEulerAngles_Test()
        {
            PrivateObject PVConvertEulerAngles = new PrivateObject(TSC.GetType());
            Vector3 vec = (Vector3)PVConvertEulerAngles.Invoke("ConvertEulerAngles", new Vector3(-30f,190f,222f));
            NUnit.Framework.Assert.AreEqual(new Vector3(330f, -170f, -138f), vec);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ConvertSingleAxis_Test()
        {
            PrivateObject PVConvertSingleAxis = new PrivateObject(TSC.GetType());
            float angle = (float)PVConvertSingleAxis.Invoke("ConvertSingleAxis",200f);
            NUnit.Framework.Assert.AreEqual(-160f, angle);

            yield return null;
        }

        [TearDown]
        public void AfterTest()
        {
            foreach (var gameobject in GameObject.FindObjectsOfType<UnitTestManager>())
                //destroy all file which are created for testing
                Object.DestroyImmediate(gameobject);
        }

    }
}