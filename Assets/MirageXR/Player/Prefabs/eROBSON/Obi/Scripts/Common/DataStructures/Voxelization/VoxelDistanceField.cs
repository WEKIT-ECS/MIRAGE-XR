using System.Collections;
using UnityEngine;

namespace Obi
{

    /**
     * Generates a sparse distance field from a voxel representation of a mesh.
     */
    public class VoxelDistanceField
    {
        public Vector3Int[,,] distanceField; // for each coordinate, stores coordinates of closest surface voxel.

        private MeshVoxelizer voxelizer;

        public VoxelDistanceField(MeshVoxelizer voxelizer)
        {
            this.voxelizer = voxelizer;
        }

        public float SampleUnfiltered(int x, int y, int z)
        {
            if (!voxelizer.VoxelExists(x, y, z)) return float.PositiveInfinity;

            float dist = Vector3.Distance(voxelizer.GetVoxelCenter(distanceField[x, y, z]),
                                          voxelizer.GetVoxelCenter(new Vector3Int(x, y, z)));

            if (voxelizer[x, y, z] == MeshVoxelizer.Voxel.Inside)
                return -dist;
            return dist;
        }

        public Vector4 SampleFiltered(float x, float y, float z)
        {
            var pos = new Vector3(x, y, z);

            // clamp position inside the distance field:
            var min = voxelizer.GetVoxelCenter(new Vector3Int(0, 0, 0)); 
            var max = voxelizer.GetVoxelCenter(new Vector3Int(voxelizer.resolution.x - 1, voxelizer.resolution.y - 1, voxelizer.resolution.z - 1));
            pos.x = Mathf.Clamp(pos.x, min.x, max.x - voxelizer.voxelSize * 0.05f);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y - voxelizer.voxelSize * 0.05f);
            pos.z = Mathf.Clamp(pos.z, min.z, max.z - voxelizer.voxelSize * 0.05f);

            var voxel = voxelizer.GetPointVoxel(pos - Vector3.one * voxelizer.voxelSize * 0.5f) - voxelizer.Origin;

            var voxelCenter = voxelizer.GetVoxelCenter(voxel);
            var norm = (pos - voxelCenter) / voxelizer.voxelSize;

            float xz00 = SampleUnfiltered(voxel.x, voxel.y, voxel.z);
            float xz01 = SampleUnfiltered(voxel.x, voxel.y, voxel.z + 1);
            float xz10 = SampleUnfiltered(voxel.x + 1, voxel.y, voxel.z);
            float xz11 = SampleUnfiltered(voxel.x + 1, voxel.y, voxel.z + 1);

            float yz00 = SampleUnfiltered(voxel.x, voxel.y + 1, voxel.z);
            float yz01 = SampleUnfiltered(voxel.x, voxel.y + 1, voxel.z + 1);
            float yz10 = SampleUnfiltered(voxel.x + 1, voxel.y + 1, voxel.z);
            float yz11 = SampleUnfiltered(voxel.x + 1, voxel.y + 1, voxel.z + 1);

            float X1 = Mathf.Lerp(xz00, xz10, norm.x);
            float X2 = Mathf.Lerp(xz01, xz11, norm.x);
            float X3 = Mathf.Lerp(yz00, yz10, norm.x);
            float X4 = Mathf.Lerp(yz01, yz11, norm.x);

            float Y1 = Mathf.Lerp(X1, X2, norm.z);
            float Y2 = Mathf.Lerp(X3, X4, norm.z);

            float R = Mathf.Lerp(Mathf.Lerp(xz10, xz11, norm.z), Mathf.Lerp(yz10, yz11, norm.z), norm.y);
            float L = Mathf.Lerp(Mathf.Lerp(xz00, xz01, norm.z), Mathf.Lerp(yz00, yz01, norm.z), norm.y);

            float F = Mathf.Lerp(X2, X4, norm.y);
            float B = Mathf.Lerp(X1, X3, norm.y);

            return new Vector4((R - L) / voxelizer.voxelSize,
                               (Y2 - Y1) / voxelizer.voxelSize,
                               (F - B) / voxelizer.voxelSize,
                               Mathf.Lerp(Y1, Y2, norm.y));
        }

        public IEnumerator JumpFlood()
        {

            // create and initialize distance field:
            distanceField = new Vector3Int[voxelizer.resolution.x,
                                           voxelizer.resolution.y,
                                           voxelizer.resolution.z];

            // create auxiliar buffer for ping-pong.
            Vector3Int[,,] auxBuffer = new Vector3Int[voxelizer.resolution.x,
                                                      voxelizer.resolution.y,
                                                      voxelizer.resolution.z];

            // initialize distance field:
            for (int x = 0; x < distanceField.GetLength(0); ++x)
                for (int y = 0; y < distanceField.GetLength(1); ++y)
                    for (int z = 0; z < distanceField.GetLength(2); ++z)
                    {
                        if (voxelizer[x, y, z] == MeshVoxelizer.Voxel.Boundary)
                            distanceField[x, y, z] = new Vector3Int(x, y, z);
                        else
                            distanceField[x, y, z] = new Vector3Int(-1, -1, -1);
                    }

            // calculate the maximum size of the buffer:
            int size = Mathf.Max(distanceField.GetLength(0),
                                 distanceField.GetLength(1),
                                 distanceField.GetLength(2));
            int step = (int)(size / 2.0f);

            yield return new CoroutineJob.ProgressInfo("Generating voxel distance field...",0);

            float numPasses = (int) Mathf.Log(size, 2);
            int i = 0;

            // jump flood passes:
            while (step >= 1)
            {
                JumpFloodPass(step, distanceField, auxBuffer);

                // halve step:
                step /= 2;

                // swap buffers:
                Vector3Int[,,] temp = distanceField;
                distanceField = auxBuffer;
                auxBuffer = temp;

                yield return new CoroutineJob.ProgressInfo("Generating voxel distance field...", ++i / numPasses);
            }

        }

        private void JumpFloodPass(int stride, Vector3Int[,,] input, Vector3Int[,,] output)
        {
            // for each voxel:
            for (int x = 0; x < input.GetLength(0); ++x)
                for (int y = 0; y < input.GetLength(1); ++y)
                    for (int z = 0; z < input.GetLength(2); ++z)
                    {
                        // our position:
                        Vector3Int p = new Vector3Int(x, y, z);

                        // our seed:
                        Vector3Int s = input[x, y, z];

                        // copy the closest seed to the output, in case we do not update it this pass:
                        output[x, y, z] = s;

                        // this voxel is a seed, skip it.
                        if (s.x == x && s.y == y && s.z == z)
                            continue;

                        // distance to our closest seed:
                        float dist = float.MaxValue;
                        if (s.x >= 0)
                            dist = (s - p).sqrMagnitude;

                        // for each neighbor voxel:
                        for (int nx = -1; nx <= 1; ++nx)
                            for (int ny = -1; ny <= 1; ++ny)
                                for (int nz = -1; nz <= 1; ++nz)
                                {
                                    // neighbor's position:
                                    int px = x + nx * stride;
                                    int py = y + ny * stride;
                                    int pz = z + nz * stride;

                                    if (voxelizer.VoxelExists(px,py,pz))
                                    {
                                        // neighbors' closest seed.
                                        Vector3Int n = input[px,py,pz];

                                        if (n.x >= 0)
                                        {
                                            // distance to neighbor's closest seed:
                                            float newDist = (n - p).sqrMagnitude;

                                            // if the distance to the neighbor's closest seed is smaller than the distance to ours:
                                            if (newDist < dist)
                                            {
                                                output[x, y, z] = n;
                                                dist = newDist;
                                            }
                                        }
                                    }
                                }
                    }

        }
    }
}