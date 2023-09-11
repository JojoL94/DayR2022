using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Smoother
{
    private Queue<Vector3> values = new Queue<Vector3>();
    private int windowSize;

    public Vector3Smoother(int windowSize)
    {
        this.windowSize = windowSize;
    }

    public Vector3 Smooth(Vector3 value)
    {
        values.Enqueue(value);

        if (values.Count > windowSize)
        {
            values.Dequeue();
        }

        var sum = Vector3.zero;
        foreach (var v in values)
        {
            sum += v;
        }

        return sum / values.Count;
    }
}
