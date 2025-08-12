using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardSphereRenderer : MonoBehaviour
{
    public Material material;

    Matrix4x4[] matrices;
    MaterialPropertyBlock mpb;
    private Mesh quadMesh;

    private Vector3[] positions = new Vector3[] {
    new Vector3(0, 0, 0),
    new Vector3(1, 0, 0),
    new Vector3(0, 1, 0),
    new Vector3(1, 1, 0)
    };

    void Start()
    {
        quadMesh = new Mesh();
        quadMesh.vertices = createPositions();
        quadMesh.triangles = createIndices();
        quadMesh.uv = CreateUV();
        quadMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);

        int x = 10;
        int y = 10;
        positions = new Vector3[x * y];
        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                positions[i * x + j] = new Vector3(j, i, 0);
            }
        }

        matrices = new Matrix4x4[positions.Length];
        Vector4[] colors = new Vector4[positions.Length];
        mpb = new MaterialPropertyBlock();

        for (int i = 0; i < positions.Length; i++)
        {
            matrices[i] = Matrix4x4.identity;
            colors[i] = new Vector4(i % 2, Mathf.Abs(i % 2 - 1), 0, 1);
        }

        mpb.SetVectorArray("_Color", colors);
    }

    private Vector3[] createPositions()
    {
        return new Vector3[] {
                new Vector3(-0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f),
                new Vector3(0.5f, 0.5f),
                new Vector3(0.5f, -0.5f),
            };
    }

    private int[] createIndices()
    {
        return new int[]
        {
                0, 3, 1,
                1, 3, 2
        };
    }

    private Vector2[] CreateUV()
    {
        return new Vector2[]
        {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
        };
    }

    private float offset = 0;

    void Update()
    {
        // Нам не нужны полные матрицы — только позиции
        Vector4[] instancePositions = new Vector4[positions.Length];
        for (int i = 0; i < positions.Length; i++)
            instancePositions[i] = new Vector4(positions[i].x + offset, positions[i].y, positions[i].z, 0.25f);
        mpb.SetVectorArray("_Position", instancePositions);
        //offset += 0.01f;

        Graphics.DrawMeshInstanced(quadMesh, 0, material, matrices, positions.Length, mpb);
    }
}
