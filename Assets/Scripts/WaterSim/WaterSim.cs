using Assets.Scripts.WaterSim.Visualization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Profiling;
using static UnityEditor.ShaderData;

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
    public ComputeShader physicsCompute;

    [Header("Walls")]
    public Bounds bounds = new Bounds(Vector3.zero, new Vector3(3, 3, 3));
    [Range(0, 10f)]
    public float wallForce = 1f;
    [Range(0, 10)]
    public float wallDistance = 0.1f;

    [Header("Visualization")]
    public Material material;
    [Range(0, 1f)]
    public float visualRadius = 0.5f;

    private Vector3[] positions;
    private Vector3[] velocities;
    private float[] density;
    private float[] pressure;
     
    private ParticleVisualizer particleVisualizer;
    private static readonly Vector3 g = new Vector3(0, -9.8f, 0);

    ComputeBuffer posBuffer;
    ComputeBuffer velocityBuffer;
    ComputeBuffer densityBuffer;
    ComputeBuffer pressureBuffer;

    void Start()
    {
        positions = new Vector3[count];
        for (int i = 0; i < count; i++)
            positions[i] = RandomExtensions.Random(bounds.min, bounds.max);

        velocities = new Vector3[count];

        posBuffer = new ComputeBuffer(count, sizeof(float) * 3);
        velocityBuffer = new ComputeBuffer(count, sizeof(float) * 3);
        densityBuffer = new ComputeBuffer(count, sizeof(float));
        pressureBuffer = new ComputeBuffer(count, sizeof(float));

        posBuffer.SetData(positions);

        // visualizer = new PrefabVisualizer(prefab, count, transform, radius);
        particleVisualizer = new ParticleVisualizer(material);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Profiler.BeginSample("FixedUpdate");

        Profiler.BeginSample("ComputeDensityPressure");
        ComputeDensityPressure();
        Profiler.EndSample();

        Profiler.BeginSample("ComputeWallForce");
        ComputeWallForce();
        Profiler.EndSample();

        Profiler.BeginSample("ComputePressureDensity");
        ComputePressureDensity();
        Profiler.EndSample();

        Profiler.BeginSample("ComputePositions");
        ComputePositions();
        Profiler.EndSample();

        posBuffer.GetData(positions);
        velocityBuffer.GetData(velocities);

        Profiler.EndSample();
    }

    private void Update()
    {
        Draw();
    }

    private void ComputeDensityPressure()
    {
        int kernel = physicsCompute.FindKernel("DensityPressure");

        physicsCompute.SetInt("count", count);
        physicsCompute.SetFloat("mass", mass);
        physicsCompute.SetFloat("radius", radius);
        physicsCompute.SetFloat("restDensity", restDensity);
        physicsCompute.SetFloat("restPressure", restPressure);

        physicsCompute.SetBuffer(kernel, "positions", posBuffer);
        physicsCompute.SetBuffer(kernel, "density", densityBuffer);
        physicsCompute.SetBuffer(kernel, "pressure", pressureBuffer);

        int threadGroups = Mathf.CeilToInt(count / 256f);
        physicsCompute.Dispatch(kernel, threadGroups, 1, 1);
    }

    private void ComputePressureDensity()
    {
        int kernel = physicsCompute.FindKernel("PressureForce");

        physicsCompute.SetInt("count", count);
        physicsCompute.SetFloat("mass", mass);
        physicsCompute.SetFloat("radius", radius);
        physicsCompute.SetFloat("restDensity", restDensity);
        physicsCompute.SetFloat("restPressure", restPressure);
        physicsCompute.SetFloat("viscosuty", viscosuty);
        physicsCompute.SetFloat("fixedDeltaTime", Time.fixedDeltaTime);

        physicsCompute.SetBuffer(kernel, "positions", posBuffer);
        physicsCompute.SetBuffer(kernel, "density", densityBuffer);
        physicsCompute.SetBuffer(kernel, "pressure", pressureBuffer);
        physicsCompute.SetBuffer(kernel, "velocities", velocityBuffer);

        int threadGroups = Mathf.CeilToInt(count / 256f);
        physicsCompute.Dispatch(kernel, threadGroups, 1, 1);
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
        int kernel = physicsCompute.FindKernel("WallForce");

        physicsCompute.SetVector("minBounds", new Vector4(bounds.min.x, bounds.min.y, bounds.min.z, 0));
        physicsCompute.SetVector("maxBounds", new Vector4(bounds.max.x, bounds.max.y, bounds.max.z, 0));
        physicsCompute.SetFloat("wallDistance", wallDistance);
        physicsCompute.SetFloat("wallForce", wallForce);

        physicsCompute.SetBuffer(kernel, "positions", posBuffer);
        physicsCompute.SetBuffer(kernel, "velocities", velocityBuffer);

        int threadGroups = Mathf.CeilToInt(count / 256f);
        physicsCompute.Dispatch(kernel, threadGroups, 1, 1);
    }

    private void ComputePositions()
    {
        int kernel = physicsCompute.FindKernel("ComputePositions");

        physicsCompute.SetBuffer(kernel, "positions", posBuffer);
        physicsCompute.SetBuffer(kernel, "velocities", velocityBuffer);

        int threadGroups = Mathf.CeilToInt(count / 256f);
        physicsCompute.Dispatch(kernel, threadGroups, 1, 1);
    }

    private void Draw()
    {
        Vector3[] colors = new Vector3[velocities.Length];
        for (int i = 0; i < velocities.Length; i++)
        {
            colors[i] = (velocities[i] / 10 + new Vector3(1, 1, 1)) / 2; 
        }
        particleVisualizer.Draw(positions, visualRadius, colors);
    }
}
