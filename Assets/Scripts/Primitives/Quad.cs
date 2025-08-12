using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Primitives
{
    public static class Quad
    {
        public static Mesh CreateMesh()
        {
            Mesh quadMesh = new Mesh();
            quadMesh.vertices = CreatePositions();
            quadMesh.triangles = CreateIndices();
            quadMesh.uv = CreateUV();
            quadMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
            return quadMesh;
        }

        private static Vector3[] CreatePositions()
        {
            return new Vector3[] {
                new Vector3(-0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f),
                new Vector3(0.5f, 0.5f),
                new Vector3(0.5f, -0.5f),
            };
        }

        private static int[] CreateIndices()
        {
            return new int[]
            {
                0, 3, 1,
                1, 3, 2
            };
        }

        private static Vector2[] CreateUV()
        {
            return new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
            };
        }

    }
}
