using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PrefabVisualizer
{
    private GameObject[] createdObjects;
    private bool hidden = false;

    public PrefabVisualizer(GameObject prefab, int count, Transform parent, float scale)
    {
        createdObjects = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            createdObjects[i] = MonoBehaviour.Instantiate(prefab, parent);
            createdObjects[i].transform.localScale = new Vector3(scale * 4, scale * 4, scale * 4);
        }
    }

    public void DrawPoints(Vector3[] points)
    {
        if (points.Length != createdObjects.Length)
            throw new ArgumentException("Length of the list of points is not equal to constructor parameter count");

        Show();
        for (int i = 0; i < createdObjects.Length; i++)
        {
            createdObjects[i].transform.position = points[i];
        }
    }

    public void DrawBB(Bounds bounds)
    {
        Vector3[] points = new Vector3[]
        {
            new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), // near down left
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), // near up left
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), // near up right
            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), // near down right
            new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), // far down left
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), // far up left
            new Vector3(bounds.max.x, bounds.max.y, bounds.max.z), // far up right
            new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), // far down right
        };

        int[] indices = new int[]
        {
            0, 1, // near quad
            1, 2,
            2, 3,
            0, 3,
            0, 4, // lines from near quad to far quad
            1, 5,
            2, 6,
            3, 7,
            4, 5, // far quad
            5, 6,
            6, 7,
            4, 7,
        };

        for (int i = 0; i < indices.Length; i += 2)
        {
            Debug.DrawLine(points[indices[i]], points[indices[i + 1]]);
        }
    }

    public void Hide()
    {
        if (!hidden)
        {
            for (int i = 0; i < createdObjects.Length; i++)
            {
                createdObjects[i].SetActive(false);
            }
            hidden = true;
        }
    }

    public void Show()
    {
        if (hidden)
        {
            for (int i = 0; i < createdObjects.Length; i++)
            {
                createdObjects[i].SetActive(true);
            }
            hidden = false;
        }
    }
}
