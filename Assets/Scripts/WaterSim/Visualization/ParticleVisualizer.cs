using Assets.Scripts.Primitives;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Scripts.WaterSim.Visualization
{
    public class ParticleVisualizer
    {
        private Material material;
        private Mesh mesh;

        public ParticleVisualizer(Material material)
        {
            this.material = material;
            mesh = Quad.CreateMesh();
        }

        private Matrix4x4[] ComputeMatrices(Vector3[] positions, float r)
        {
            Matrix4x4[] matrices = new Matrix4x4[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                matrices[i] = Matrix4x4.LookAt(positions[i], Camera.main.transform.position, Vector3.up);
                matrices[i] = matrices[i] * Matrix4x4.Scale(new Vector3(r, r, r));
            }
            return matrices;
        }

        public void Draw(Vector3[] positions, Vector3 color, float r)
        {
            Vector4[] colors = new Vector4[positions.Length];
            Vector4[] instancePositions = new Vector4[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                colors[i] = new Vector4(color.x, color.y, color.z, 1);
                instancePositions[i] = new Vector4(positions[i].x, positions[i].y, positions[i].z, 0);
            }

            Matrix4x4[] matrices = ComputeMatrices(positions, r);

            Draw(instancePositions, colors, matrices);
        }

        public void Draw(Vector3[] positions, float r, Vector3[] color)
        {
            Vector4[] colors = new Vector4[positions.Length];
            Vector4[] instancePositions = new Vector4[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                colors[i] = new Vector4(color[i].x, color[i].y, color[i].z, 1);
                instancePositions[i] = new Vector4(positions[i].x, positions[i].y, positions[i].z, 0);
            }

            Matrix4x4[] matrices = ComputeMatrices(positions, r);

            Draw(instancePositions, colors, matrices);
        }

        public void Draw(Vector4[] positions, Vector4[] colors, Matrix4x4[] matrices)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();

            mpb.SetVectorArray("_Position", positions);
            mpb.SetVectorArray("_Color", colors);

            Graphics.DrawMeshInstanced(mesh, 0, material, matrices, positions.Length, mpb);
        }
    }
}
