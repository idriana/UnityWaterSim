using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI tmp;
    float[] timedeltas = new float[300];
    int ptr = 0;

    // Update is called once per frame
    void Update()
    {
        timedeltas[ptr] = Time.deltaTime;

        float sumTime = 0;
        for (int i = 0; i < timedeltas.Length; i++)
        {
            sumTime += timedeltas[i]; 
        }
        float fps = timedeltas.Length / sumTime;

        tmp.text = $"fps: {fps}";

        ptr = (ptr + 1) % timedeltas.Length;
    }
}
