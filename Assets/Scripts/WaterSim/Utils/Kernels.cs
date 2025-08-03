using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public static class Kernels
{
    public static float Poly6(float sqrR, float h)
    {
        // poly6(r, h) = 315/(64 * π * h^9) * (h^2 - r^2)^3, 0 <= r <= h
        if (sqrR < Mathf.Epsilon || h * h < sqrR)
            return 0;

        float x = (h * h - sqrR);
        return 315.0f / (64 * Mathf.PI * Mathf.Pow(h, 9)) * Mathf.Pow(x, 3);
    }

    public static Vector3 SpikyGrad(float h, Vector3 r_vec)
    {
        // ∇spiky(r, h) = -45 / (π * h^6) * (|h| - |r|)^2 * norm(r)
        float r = r_vec.magnitude;
        if (r < Mathf.Epsilon)
            return Vector3.zero;

        return -45 / (Mathf.PI * Mathf.Pow(h, 6)) * (h - r) * (h - r) * r_vec.normalized;
    }

    public static float ViscosityLaplas(float h, float r)
    {
        if (r < Mathf.Epsilon || h < r)
            return 0;

        // ∇^2viscosity(r, h) = 45 / (π * h^6) * (h - r)
        return 45 / (Mathf.PI * Mathf.Pow(h, 6)) * (h - r);
    }
}
