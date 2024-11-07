using LearningExperienceEngine;
using System.Collections;
using System.Reflection;
using MirageXR;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SetStartingPositionTests
    {
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
            yield return new WaitForSeconds(0.5f);

            target.transform.position = targetPosition;
            target.transform.eulerAngles = targetEulers;

            LearningExperienceEngine.EventManager.ResetPlayer();
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
            yield return new WaitForSeconds(0.5f);

            target.transform.position = targetPosition;
            target.transform.eulerAngles = targetEulers;

            LearningExperienceEngine.EventManager.ResetPlayer();
            Vector3 expectedEulers = new Vector3(targetEulers.x, targetEulers.y, 0);
            Assert.AreEqual(expectedEulers, go.transform.eulerAngles);
        }

        [UnityTest]
        public IEnumerator OnWorkplaceLoaded_PositionUpdated()
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

            LearningExperienceEngine.EventManager.WorkplaceLoaded();

            Assert.AreEqual(targetPosition, go.transform.position);
        }

        [UnityTest]
        public IEnumerator OnWorkplaceLoaded_RotationUpdated()
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

            LearningExperienceEngine.EventManager.WorkplaceLoaded();

            Vector3 expectedEulers = new Vector3(targetEulers.x, targetEulers.y, 0);
            Assert.AreEqual(expectedEulers, go.transform.eulerAngles);
        }

        private static void CallPrivateMethod(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(obj, parameters);
        }

        private static T GenerateGameObjectWithComponent<T>(string name, bool activated = true) where T : MonoBehaviour
        {
            var go = new GameObject(name);
            go.SetActive(activated);
            return go.AddComponent<T>();
        }
    }
}
