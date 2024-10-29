using UnityEngine;

public class NoiseGenerator
{

    private ComputeShader noiseShader;
    private ComputeBuffer valuesBuffer;

    public NoiseGenerator(ComputeShader noiseShader)
    {
        this.noiseShader = noiseShader;
    }


    public float[] GenerateNoise(int width, int height, Vector2 offset, int octaves, float persistence, float lacunarity, float scale, float groundLevel)
    {

        float[] noiseValues = new float[width * height * width]; // 1D array of noise values

        valuesBuffer = new ComputeBuffer(width * height * width, sizeof(float));

        noiseShader.SetBuffer(0, "_Values", valuesBuffer);
        noiseShader.SetInt("_ChunkWidth", width);
        noiseShader.SetInt("_ChunkHeight", height);
        noiseShader.SetFloat("_OffsetX", offset.x);
        noiseShader.SetFloat("_OffsetZ", offset.y);

        noiseShader.SetInt("octaves", octaves);
        noiseShader.SetFloat("scale", scale);
        noiseShader.SetFloat("persistence", persistence);
        noiseShader.SetFloat("lacunarity", lacunarity);
        noiseShader.SetFloat("groundLevel", groundLevel);

        int numThreadsXZ = Mathf.CeilToInt(width / 8);
        int numThreadsY = Mathf.CeilToInt(height / 8);

        noiseShader.Dispatch(0, numThreadsXZ, numThreadsY, numThreadsXZ);

        valuesBuffer.GetData(noiseValues);

        valuesBuffer.Release();

        return noiseValues;
    }
}
