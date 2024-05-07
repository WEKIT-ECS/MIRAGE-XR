using System.Collections;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using MirageXR;
using System;

namespace Tests
{
    public class TagLockTests
    {
        [UnityTest]
        public IEnumerator Tag_LockPosition_Test()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<UnitTestManager>();
            obj.AddComponent<TagLock>();
            obj.AddComponent<RadialView>();
            try
            {
                obj.GetComponent<TagLock>().LockPosition();
            }
            catch (NullReferenceException)
            {
            }

            yield return null;

            Assert.IsTrue(!obj.GetComponent<RadialView>().enabled);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Tag_ReleasePosition_Test()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<UnitTestManager>();
            obj.AddComponent<TagLock>();
            obj.AddComponent<RadialView>();
            try
            {
                obj.GetComponent<TagLock>().LockPosition();
            }
            catch (NullReferenceException)
            {

            }
            yield return null;
            try
            {
                obj.GetComponent<TagLock>().ReleasePosition();
            }
            catch (NullReferenceException)
            {}

            Assert.IsTrue(obj.GetComponent<RadialView>().enabled);
            yield return null;
        }

        [TearDown]
        public void AfterTest()
        {
            foreach (var gameobject in GameObject.FindObjectsOfType<UnitTestManager>())
                //destroy all file which are created for testing
                UnityEngine.Object.DestroyImmediate(gameobject);
        }

    }

}

