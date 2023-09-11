using System.Collections.Generic;


public class FloatSmoother
{
    private Queue<float> values = new Queue<float>();
    private int windowSize;

    public FloatSmoother(int windowSize)
    {
        this.windowSize = windowSize;
    }

    public float Smooth(float value)
    {
        values.Enqueue(value);

        if (values.Count > windowSize)
        {
            values.Dequeue();
        }

        float sum = 0f;
        foreach (float v in values)
        {
            sum += v;
        }

        return sum / values.Count;
    }
}

