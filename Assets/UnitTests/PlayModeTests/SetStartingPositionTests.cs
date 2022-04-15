using System.Collections;
using System.Collections.Generic;
using MirageXR;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class SetStartingPositionTests
    {
        [SetUp]
        public void SetUp()
        {
            SceneManager.LoadScene("TestScene", LoadSceneMode.Additive);
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.UnloadSceneAsync("TestScene");
        }

        [UnityTest]
        public IEnumerator Start_SetsPositionToUserViewport()
        {
            Vector3 targetPosition = new Vector3(1, 2, 3);
            Vector3 targetEulers = new Vector3(0, 90, 180);

            GameObject target = new GameObject("UserViewportObj");
            target.tag = "UserViewport";
            target.transform.position = targetPosition;
            target.transform.eulerAngles = targetEulers;

            GameObject go = new GameObject("FakeMenu");
            go.AddComponent<SetStartingPosition>();
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            // let Unity initialization magic methods run, e.g. Start
            yield return null;

            Assert.AreEqual(targetPosition, go.transform.position);
        }

        [UnityTest]
        public IEnumerator Start_SetRotationToUserViewport()
        {

            Vector3 targetPosition = new Vector3(1, 2, 3);
            Vector3 targetEulers = new Vector3(0, 90, 0);

            GameObject target = new GameObject("UserViewportObj");
            target.tag = "UserViewport";
            target.transform.position = targetPosition;
            target.transform.eulerAngles = targetEulers;

            GameObject go = new GameObject("FakeMenu");
            go.AddComponent<SetStartingPosition>();
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            // let Unity initialization magic methods run, e.g. Start
            yield return null;

            Assert.AreEqual(targetEulers, go.transform.eulerAngles);
        }

        [UnityTest]
        public IEnumerator Start_ZRotationStaysZero()
        {
            Vector3 targetPosition = new Vector3(1, 2, 3);
            Vector3 targetEulers = new Vector3(0, 90, 180);

            GameObject target = new GameObject("UserViewportObj");
            target.tag = "UserViewport";
            target.transform.position = targetPosition;
            target.transform.eulerAngles = targetEulers;

            GameObject go = new GameObject("FakeMenu");
            go.AddComponent<SetStartingPosition>();
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            // let Unity initialization magic methods run, e.g. Start
            yield return null;

            Assert.AreEqual(0, go.transform.eulerAngles.z);
        }

        [UnityTest]
        public IEnumerator OnPlayerReset_PositionUpdated()
        {
            Vector3 targetPosition = new Vector3(1, 2, 3);
            Vector3 targetEulers = new Vector3(0, 90, 180);

            GameObject target = new GameObject("UserViewportObj");
            target.tag = "UserViewport";
            target.transform.position = Vector3.zero;
            target.transform.rotation = Quaternion.identity;

            GameObject go = new GameObject("FakeMenu");
            go.AddComponent<SetStartingPosition>();
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            // let Unity initialization magic methods run, e.g. Start
            yield return null;

            target.transform.position = targetPosition;
            target.transform.eulerAngles = targetEulers;

            EventManager.PlayerReset();

            Assert.AreEqual(targetPosition, go.transform.position);
        }

        [UnityTest]
        public IEnumerator OnPlayerReset_RotationUpdated()
        {
            Vector3 targetPosition = new Vector3(1, 2, 3);
            Vector3 targetEulers = new Vector3(0, 90, 180);

            GameObject target = new GameObject("UserViewportObj");
            target.tag = "UserViewport";
            target.transform.position = Vector3.zero;
            target.transform.rotation = Quaternion.identity;

            GameObject go = new GameObject("FakeMenu");
            go.AddComponent<SetStartingPosition>();
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            // let Unity initialization magic methods run, e.g. Start
            yield return null;

            target.transform.position = targetPosition;
            target.transform.eulerAngles = targetEulers;

            EventManager.PlayerReset();

            Vector3 expectedEulers = new Vector3(targetEulers.x, targetEulers.y, 0);
            Assert.AreEqual(expectedEulers, go.transform.eulerAngles);
        }

        [UnityTest]
        public IEnumerator OnWorkplaceParsed_PositionUpdated()
        {
            Vector3 targetPosition = new Vector3(1, 2, 3);
            Vector3 targetEulers = new Vector3(0, 90, 180);

            GameObject target = new GameObject("UserViewportObj");
            target.tag = "UserViewport";
            target.transform.position = Vector3.zero;
            target.transform.rotation = Quaternion.identity;

            GameObject go = new GameObject("FakeMenu");
            go.AddComponent<SetStartingPosition>();
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            // let Unity initialization magic methods run, e.g. Start
            yield return null;

            target.transform.position = targetPosition;
            target.transform.eulerAngles = targetEulers;

            EventManager.WorkplaceParsed();

            Assert.AreEqual(targetPosition, go.transform.position);
        }

        [UnityTest]
        public IEnumerator OnWorkplaceParsed_RotationUpdated()
        {
            Vector3 targetPosition = new Vector3(1, 2, 3);
            Vector3 targetEulers = new Vector3(0, 90, 180);

            GameObject target = new GameObject("UserViewportObj");
            target.tag = "UserViewport";
            target.transform.position = Vector3.zero;
            target.transform.rotation = Quaternion.identity;

            GameObject go = new GameObject("FakeMenu");
            go.AddComponent<SetStartingPosition>();
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            // let Unity initialization magic methods run, e.g. Start
            yield return null;

            target.transform.position = targetPosition;
            target.transform.eulerAngles = targetEulers;

            EventManager.WorkplaceParsed();

            Vector3 expectedEulers = new Vector3(targetEulers.x, targetEulers.y, 0);
            Assert.AreEqual(expectedEulers, go.transform.eulerAngles);
        }
    }
}