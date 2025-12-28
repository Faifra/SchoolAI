using UnityEngine;

public class ANN
{
    public int inputCount;
    public int hiddenCount;
    public int outputCount;

    public float[,] w1; // input → hidden
    public float[,] w2; // hidden → output

    public ANN(int inputCount, int hiddenCount, int outputCount)
    {
        this.inputCount = inputCount;
        this.hiddenCount = hiddenCount;
        this.outputCount = outputCount;

        w1 = new float[inputCount, hiddenCount];
        w2 = new float[hiddenCount, outputCount];

        RandomizeWeights();
    }

    void RandomizeWeights()
    {
        for (int i = 0; i < inputCount; i++)
            for (int j = 0; j < hiddenCount; j++)
                w1[i, j] = Random.Range(-1f, 1f);

        for (int i = 0; i < hiddenCount; i++)
            for (int j = 0; j < outputCount; j++)
                w2[i, j] = Random.Range(-1f, 1f);
    }

    float Activation(float x)
    {
return (float)System.Math.Tanh(x);
    }

    public float[] Forward(float[] inputs)
    {
        float[] hidden = new float[hiddenCount];
        float[] output = new float[outputCount];

        // Input → Hidden
        for (int h = 0; h < hiddenCount; h++)
        {
            float sum = 0f;
            for (int i = 0; i < inputCount; i++)
                sum += inputs[i] * w1[i, h];

            hidden[h] = Activation(sum);
        }

        // Hidden → Output
        for (int o = 0; o < outputCount; o++)
        {
            float sum = 0f;
            for (int h = 0; h < hiddenCount; h++)
                sum += hidden[h] * w2[h, o];

            output[o] = Activation(sum);
        }

        return output;
    }
}