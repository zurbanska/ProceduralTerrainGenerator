using UnityEngine;

public class NoiseGenerator
{

    private ComputeShader noiseShader;
    private ComputeBuffer valuesBuffer;

    public NoiseGenerator(ComputeShader noiseShader)
    {
        this.noiseShader = noiseShader;
    }


    public float[] GenerateNoise(int width, int height)
    {
        valuesBuffer = new ComputeBuffer(width * height * width, sizeof(float));
        float[] noiseValues = new float[width * height * width]; // 1D array of noise values

        noiseShader.SetBuffer(0, "_Values", valuesBuffer);
        noiseShader.SetInt("_ChunkWidth", width);
        noiseShader.SetInt("_ChunkHeight", height);

        noiseShader.Dispatch(0, width / 8, height / 8, width / 8);

        valuesBuffer.GetData(noiseValues);

        valuesBuffer.Release();

        return noiseValues;
    }
}
