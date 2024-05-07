using MirageXR;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PlayerUtilitiesTests
    {
        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }

        [Test]
        public void ParseStringToVector3_EmptyInput_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Utilities.ParseStringToVector3(""));
        }

        [Test]
        public void ParseStringToVector3_VectorWithInts_ReturnsVector()
        {
            const string input = "1 2 3";
            Vector3 expectedRes = new Vector3(1, 2, 3);
            Vector3 res = Utilities.ParseStringToVector3(input);

            Assert.AreEqual(expectedRes, res);
        }

        [Test]
        public void ParseStringToVector3_VectorWithFloats_ReturnsVector()
        {
            const string input = "1.2 2.3 3.4";
            Vector3 expectedRes = new Vector3(1.2f, 2.3f, 3.4f);
            Vector3 res = Utilities.ParseStringToVector3(input);

            Assert.AreEqual(expectedRes, res);
        }

        [Test]
        public void TryParseStringToVector3_EmptyString_ReturnsFalse()
        {
            const string input = "";
            bool res = Utilities.TryParseStringToVector3(input, out Vector3 vector);

            Assert.IsFalse(res);
        }

        [Test]
        public void TryParseStringToVector3_EmptyString_VectorDefault()
        {
            const string input = "";
            bool res = Utilities.TryParseStringToVector3(input, out Vector3 vector);

            Assert.AreEqual(Vector3.zero, vector);
        }

        [Test]
        public void TryParseStringToVector3_VectorWithInts_ReturnsTrue()
        {
            const string input = "1 2 3";
            bool res = Utilities.TryParseStringToVector3(input, out Vector3 vector);

            Assert.IsTrue(res);
        }

        [Test]
        public void TryParseStringToVector3_VectorWithInts_VectorCorrect()
        {
            const string input = "1 2 3";
            Vector3 expectedVector = new Vector3(1, 2, 3);
            bool res = Utilities.TryParseStringToVector3(input, out Vector3 vector);

            Assert.AreEqual(expectedVector, vector);
        }

        [Test]
        public void TryParseStringToVector3_VectorWithFloats_ReturnsTrue()
        {
            const string input = "1.2 2.3 3.4";
            bool res = Utilities.TryParseStringToVector3(input, out Vector3 vector);

            Assert.IsTrue(res);
        }

        [Test]
        public void TryParseStringToVector3_VectorWithFloats_VectorCorrect()
        {
            const string input = "1.2 2.3 3.4";
            Vector3 expectedVector = new Vector3(1.2f, 2.3f, 3.4f);
            bool res = Utilities.TryParseStringToVector3(input, out Vector3 vector);

            Assert.AreEqual(expectedVector, vector);
        }

        [Test]
        public void ParseStringToQuaternion_EmptyString_ThrowsArgumentException()
        {
            const string input = "";
            Assert.Throws<ArgumentException>(() => Utilities.ParseStringToQuaternion(input));
        }

        [Test]
        public void ParseStringToQuaternion_QuaternionWithFloats_ReturnsQuaternion()
        {
            const string input = "1.2 2.3 3.4 4.5";
            Quaternion expected = new Quaternion(1.2f, 2.3f, 3.4f, 4.5f);
            Quaternion result = Utilities.ParseStringToQuaternion(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TryParseStringToQuaternion_EmptyString_ReturnsFalse()
        {
            const string input = "";
            bool res = Utilities.TryParseStringToQuaternion(input, out Quaternion quat);

            Assert.IsFalse(res);
        }

        [Test]
        public void TryParseStringToQuaternion_EmptyString_DefaultQuaternion()
        {
            const string input = "";
            bool res = Utilities.TryParseStringToQuaternion(input, out Quaternion quat);

            Assert.AreEqual(Quaternion.identity, quat);
        }

        [Test]
        public void TryParseStringToQuaternion_QuaternionWithFloats_ReturnsTrue()
        {
            const string input = "1.2 2.3 3.4 4.5";
            bool res = Utilities.TryParseStringToQuaternion(input, out Quaternion quat);

            Assert.IsTrue(res);
        }

        [Test]
        public void TryParseStringToQuaternion_QuaternionWithFloats_CorrectQuaternion()
        {
            const string input = "1.2 2.3 3.4 4.5";
            Quaternion expected = new Quaternion(1.2f, 2.3f, 3.4f, 4.5f);
            bool res = Utilities.TryParseStringToQuaternion(input, out Quaternion quat);

            Assert.AreEqual(expected, quat);
        }

        [Test]
        public void CreateObject_ParentNull_IdSetAsGameObjectName()
        {
            const string id = "My ID";
            GameObject res = Utilities.CreateObject(id, (Transform)null);

            Assert.AreEqual(id, res.name);
        }

        [Test]
        public void CreateObject_ParentNull_NoParentSet()
        {
            const string id = "My ID";
            GameObject res = Utilities.CreateObject(id, (Transform)null);

            Assert.IsNull(res.transform.parent);
        }

        [Test]
        public void CreateObject_ParentGiven_ParentSet()
        {
            const string id = "My ID";
            GameObject parent = new GameObject("Parent");
            GameObject res = Utilities.CreateObject(id, parent.transform);

            Assert.AreEqual(parent.transform, res.transform.parent);
        }

        [Test]
        public void CreateObject_ParentNameDoesNotExist_ReturnsNull()
        {
            const string id = "My ID";

            LogAssert.Expect(LogType.Exception, new Regex(@".*Object.*not found.*"));

            GameObject res = Utilities.CreateObject(id, "this parent does not exist");

            Assert.IsNull(res);
        }

        [Test]
        public void CreateObject_ParentNameExists_SetsParent()
        {
            const string id = "My ID";
            GameObject parent = new GameObject("Parent");
            GameObject res = Utilities.CreateObject(id, parent.name);

            Assert.AreEqual(parent.transform, res.transform.parent);
        }

        [Test]
        public void FindDeepChild_NoChildren_ReturnsNull()
        {
            GameObject parent = new GameObject("Parent");

            Transform res = parent.transform.FindDeepChild("someId");

            Assert.IsTrue(res == null);
        }

        [Test]
        public void FindDeepChild_ChildOnLevel1_ReturnsChild()
        {
            GameObject parent = new GameObject("Parent");

            for (int i=0;i<5;i++)
            {
                GameObject go = new GameObject($"Child {i}");
                go.transform.parent = parent.transform;
            }

            GameObject expectedFind = new GameObject("Find me");
            expectedFind.transform.parent = parent.transform;

            Transform res = parent.transform.FindDeepChild("Find me");

            Assert.AreEqual(expectedFind.transform, res);
        }

        [Test]
        public void FindDeepChild_ChildOnLevel2_ReturnsChild()
        {
            GameObject parent = new GameObject("Parent");

            for (int i = 0; i < 3; i++)
            {
                GameObject go = new GameObject($"Child {i}");
                go.transform.parent = parent.transform;
                for (int j=0;j<3;j++)
                {
                    GameObject child = new GameObject($"Grandchild {i},{j}");
                    child.transform.parent = go.transform;
                }
            }

            GameObject parentOfFind = new GameObject("Parent of find");
            parentOfFind.transform.parent = parent.transform;
            GameObject expectedFind = new GameObject("Find me");
            expectedFind.transform.parent = parentOfFind.transform;

            Transform res = parent.transform.FindDeepChild("Find me");

            Assert.AreEqual(expectedFind.transform, res);
        }

        [Test]
        public void FindDeepChildTag_NoChildren_ReturnsNull()
        {
            GameObject parent = new GameObject("Parent");

            Transform res = parent.transform.FindDeepChildTag("Player");

            Assert.IsTrue(res == null);
        }

        [Test]
        public void FindDeepChildTag_ChildOnLevel1_ReturnsChild()
        {
            GameObject parent = new GameObject("Parent");

            for (int i = 0; i < 5; i++)
            {
                GameObject go = new GameObject($"Child {i}");
                go.transform.parent = parent.transform;
            }

            GameObject expectedFind = new GameObject("Find me");
            expectedFind.transform.parent = parent.transform;
            expectedFind.tag = "Player";

            Transform res = parent.transform.FindDeepChildTag("Player");

            Assert.AreEqual(expectedFind.transform, res);
        }

        [Test]
        public void FindDeepChildTag_ChildOnLevel2_ReturnsChild()
        {
            GameObject parent = new GameObject("Parent");

            for (int i = 0; i < 3; i++)
            {
                GameObject go = new GameObject($"Child {i}");
                go.transform.parent = parent.transform;
                for (int j = 0; j < 3; j++)
                {
                    GameObject child = new GameObject($"Grandchild {i},{j}");
                    child.transform.parent = go.transform;
                }
            }

            GameObject parentOfFind = new GameObject("Parent of find");
            parentOfFind.transform.parent = parent.transform;
            GameObject expectedFind = new GameObject("Find me");
            expectedFind.transform.parent = parentOfFind.transform;
            expectedFind.tag = "Player";

            Transform res = parent.transform.FindDeepChildTag("Player");

            Assert.AreEqual(expectedFind.transform, res);
        }
    }
}
