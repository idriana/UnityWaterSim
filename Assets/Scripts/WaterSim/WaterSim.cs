using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public class WaterSim : MonoBehaviour
{
    [Header("Common")]
    public int count = 100;

    [Header("Physics")]
    [Range(0, 1f)]
    public float radius = 1f;
    [Range(0, 1f)]
    public float mass = 1.0f;
    [Range(0, 1f)]
    public float restDensity = 0.25f;
    [Range(0, 1f)]
    public float viscosuty = 0.1f;
    public float restPressure = 100;
    [Range(0, 1f)]
    public float smoothingCoeff = 0.1f;

    [Header("Walls")]
    public Bounds bounds = new Bounds(Vector3.zero, new Vector3(3, 3, 3));
    [Range(0, 10f)]
    public float wallForce = 1f;
    [Range(0, 10)]
    public float wallDistance = 0.1f;

    [Header("Visualization")]
    public GameObject prefab;

    private Vector3[] positions;
    private Vector3[] velocities;
    private float[] density;
    private float[] pressure;

    private PrefabVisualizer visualizer;
    private static readonly Vector3 g = new Vector3(0, -9.8f, 0);

    void Start()
    {
        positions = new Vector3[count];
        for (int i = 0; i < count; i++)
            positions[i] = RandomExtensions.Random(bounds.min, bounds.max);

        velocities = new Vector3[count];
        density = new float[count];
        pressure = new float[count];
        visualizer = new PrefabVisualizer(prefab, count, transform, radius);
    }

    // Update is called once per frame
    void Update()
    {
        ComputeDensityPressure();
        ComputeVelocities();
        ComputePositions();
        Draw();
    }

    private void ComputeDensityPressure()
    {
        for (int i = 0; i < count; i++)
        {
            density[i] = 0;
            for (int j = 0; j < count; j++)
            {
                Vector3 r_vec = positions[i] - positions[j];
                density[i] += mass * Kernels.Poly6(r_vec.sqrMagnitude, radius * 4);
            }
            pressure[i] = restPressure * (density[i] - restDensity);
        }
    }

    private void ComputeVelocities()
    {
        ComputeWallForce();
        ComputePressureDensity();
        XSPHCorrection();
    }

    private void ComputePressureDensity()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 forcePressute = Vector3.zero;
            Vector3 forceViscosity = Vector3.zero;
            for (int j = 0; j < count; j++)
            {
                if (i == j)
                    continue;

                Vector3 r_vec = positions[i] - positions[j];
                float r = r_vec.magnitude;

                if (r < radius * 4 && density[j] != 0)
                {
                    forcePressute += -mass * (pressure[i] + pressure[j]) / (2 * density[j]) * Kernels.SpikyGrad(radius * 4, r_vec);
                    forceViscosity += viscosuty * mass * (velocities[j] - velocities[i]) / density[j] * Kernels.ViscosityLaplas(radius * 4, r);
                }
            }
            velocities[i] += Time.fixedDeltaTime * (forcePressute + forceViscosity + g);
            if (velocities[i].sqrMagnitude > 100)
                velocities[i] = velocities[i].normalized * 10;
        }
    }

    private void XSPHCorrection()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 xsph = Vector3.zero;
            for (int j = 0; j < count; j++)
            {
                if (i == j || density[j] == 0) continue;

                Vector3 r_vec = positions[i] - positions[j];
                float r2 = r_vec.sqrMagnitude;
                if (r2 < (radius * 4) * (radius * 4))
                {
                    float w = Kernels.Poly6(r2, radius * 4);
                    xsph += (velocities[j] - velocities[i]) * (mass / density[j]) * w;
                }
            }

            velocities[i] += smoothingCoeff * xsph;
        }
    }

    private void ComputeWallForce()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 minDistance = positions[i] - bounds.min;
            if (minDistance.x <= wallDistance)
                velocities[i].x += Mathf.Lerp(wallForce, 0f, 1 - Mathf.Max(minDistance.x / wallDistance + 0.01f, 0.01f));
            if (minDistance.y <= wallDistance)
                velocities[i].y += Mathf.Lerp(wallForce, 0f, 1 - Mathf.Max(minDistance.y / wallDistance + 0.01f, 0.01f));
            if (minDistance.z <= wallDistance)
                velocities[i].z += Mathf.Lerp(wallForce, 0f, 1 - Mathf.Max(minDistance.z / wallDistance + 0.01f, 0.01f));

            Vector3 maxDistance = bounds.max - positions[i];
            if (maxDistance.x <= wallDistance)
                velocities[i].x += Mathf.Lerp(-wallForce, 0f, 1 - Mathf.Max(maxDistance.x / wallDistance + 0.01f, 0.01f));
            if (maxDistance.y <= wallDistance)
                velocities[i].y += Mathf.Lerp(-wallForce, 0f, 1 - Mathf.Max(maxDistance.y / wallDistance + 0.01f, 0.01f));
            if (maxDistance.z <= wallDistance)
                velocities[i].z += Mathf.Lerp(-wallForce, 0f, 1 - Mathf.Max(maxDistance.z / wallDistance + 0.01f, 0.01f));
            if (positions[i].y <= bounds.min.y)
            {
                velocities[i].y -= Time.fixedDeltaTime * g.y;
            }
        }
    }

    private void ComputePositions()
    {
        for (int i = 0; i < count; i++)
        {
            positions[i] += Time.fixedDeltaTime * velocities[i];
            if (positions[i].x < bounds.min.x)
            {
                positions[i].x = bounds.min.x;
                if (velocities[i].x < 0)
                    velocities[i].x *= -0.5f;
            }
            if (positions[i].x > bounds.max.x)
            {
                positions[i].x = bounds.max.x;
                if (velocities[i].x > 0)
                    velocities[i].x *= -0.5f;
            }
            if (positions[i].y < bounds.min.y)
            {
                positions[i].y = bounds.min.y;
                if (velocities[i].y < 0)
                    velocities[i].y *= -0.5f;
            }
            if (positions[i].y > bounds.max.y)
            {
                positions[i].y = bounds.max.y;
                if (velocities[i].y > 0)
                    velocities[i].y *= -0.5f;
            }
            if (positions[i].z < bounds.min.z)
            {
                positions[i].z = bounds.min.z;
                if (velocities[i].z < 0)
                    velocities[i].z *= -0.5f;
            }
            if (positions[i].z > bounds.max.z)
            {
                positions[i].z = bounds.max.z;
                if (velocities[i].z > 0)
                    velocities[i].z *= -0.5f;
            }
        }
        
    }

    private void Draw()
    {
        visualizer.DrawBB(bounds);
        visualizer.DrawPoints(positions);
    }
}
