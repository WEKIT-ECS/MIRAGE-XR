using NUnit.Framework;
using UnityEngine.TestTools;
using MirageXR;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Tests
{
    public class TappableTests
    {
        
        [UnityTest]
        public IEnumerator Tap_OnPointerEnter_Test()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<UnitTestManager>();
            obj.AddComponent<Tappable>();

            obj.GetComponent<Tappable>().OnPointerEnter(null);

            Assert.IsTrue(obj.GetComponent<Tappable>().IsSelected);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Tap_OnPointerExit_Test()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<UnitTestManager>();
            obj.AddComponent<Tappable>();

            obj.GetComponent<Tappable>().OnPointerExit(null);

            Assert.IsTrue(!obj.GetComponent<Tappable>().IsSelected);
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