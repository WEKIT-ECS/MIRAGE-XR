// Based on: https://catlikecoding.com/unity/tutorials/advanced-rendering/flat-and-wireframe-shading/
using UnityEngine;

namespace PolySpatial.Samples
{
    [RequireComponent(typeof(MeshFilter))]
    public class BarycentricDataBuilder : MonoBehaviour
    {
        private void Reset()
        {
            GenerateBarycentricData();
        }

        void GenerateBarycentricData()
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            if (!mesh)
            {
                return;
            }

            SplitMesh(mesh);
            SetVertexColors(mesh);
        }

        public void GenerateData(Mesh mesh)
        {
            SplitMesh(mesh);
            SetVertexColors(mesh);
        }

        void SetVertexColors(Mesh mesh)
        {
            Color[] colorCoords = new[]
            {
                new Color(1, 0, 0),
                new Color(0, 1, 0),
                new Color(0, 0, 1),
            };

            Color32[] vertexColors = new Color32[mesh.vertices.Length];

            for (int i = 0; i < vertexColors.Length; i += 3)
            {
                vertexColors[i] = colorCoords[0];
                vertexColors[i + 1] = colorCoords[1];
                vertexColors[i + 2] = colorCoords[2];
            }

            mesh.colors32 = vertexColors;
        }

        void SplitMesh(Mesh mesh)
        {
            int[] triangles = mesh.triangles;
            Vector3[] verts = mesh.vertices;
            Vector3[] normals = mesh.normals;

            Vector3[] newVerts;
            Vector3[] newNormals;

            int n = triangles.Length;
            newVerts = new Vector3[n];
            newNormals = new Vector3[n];

            for (int i = 0; i < n; i++)
            {
                newVerts[i] = verts[triangles[i]];
                newNormals[i] = normals[triangles[i]];
                triangles[i] = i;
            }

            mesh.vertices = newVerts;
            mesh.normals = newNormals;
            mesh.triangles = triangles;
        }
    }
}
