using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Tests
{
    public class UIPolygonTests: UIPolygon
    {
        
        [UnityTest]
        public IEnumerator mainTexture_IsNotNull_Test()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<UIPolygon>();
            obj.AddComponent<UnitTestManager>();

            Assert.IsNotNull(obj.GetComponent<UIPolygon>().mainTexture);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DrawPolygon_Test()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<UIPolygon>();
            obj.AddComponent<UnitTestManager>();

            obj.GetComponent<UIPolygon>().DrawPolygon(6);
            Assert.AreEqual(7, obj.GetComponent<UIPolygon>().VerticesDistances.Length);
            Assert.AreEqual(new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f }, obj.GetComponent<UIPolygon>().VerticesDistances);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetVbo_Test()
        {
            Vector2[] vec =  { new Vector2(1, 2), new Vector2(1, 3) };
            Vector2[] uvs = { new Vector2(3, 3), new Vector2(1, 2) };
   
            UIVertex[] vx = new UIVertex[4];

            var vert = UIVertex.simpleVert;
            vert.color = new Color(255, 255, 255);
            vert.position = new Vector2(1, 2);
            vert.uv0 = new Vector2(3,3);
            Assert.AreEqual(vert, SetVbo(vec, uvs)[0]);
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