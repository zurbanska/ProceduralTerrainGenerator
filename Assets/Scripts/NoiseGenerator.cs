using UnityEngine;

public class NoiseGenerator
{

    private ComputeShader noiseShader;
    public ComputeBuffer valuesBuffer;
    public NoiseData noiseData;

    public NoiseGenerator(ComputeShader noiseShader, NoiseData noiseData)
    {
        this.noiseShader = noiseShader;
        this.noiseData = noiseData;
    }


    public float[] GenerateNoise(int width, int height, Vector2 offset, int octaves, float persistence, float lacunarity, float scale, float groundLevel, float seed, Vector4 neighbors)
    {

        float[] noiseValues = new float[width * height * width]; // 1D array of noise values

        valuesBuffer = new ComputeBuffer(width * height * width, sizeof(float));

        noiseShader.SetBuffer(0, "_Values", valuesBuffer);
        noiseShader.SetInt("_ChunkWidth", width);
        noiseShader.SetInt("_ChunkHeight", height);
        noiseShader.SetFloat("_OffsetX", offset.x + noiseData.moreOffset.x);
        noiseShader.SetFloat("_OffsetZ", offset.y + noiseData.moreOffset.y);

        noiseShader.SetBool("borderDown", neighbors[0] == 0);
        noiseShader.SetBool("borderRight", neighbors[1] == 0);
        noiseShader.SetBool("borderUp", neighbors[2] == 0);
        noiseShader.SetBool("borderLeft", neighbors[3] == 0);

        noiseShader.SetInt("octaves", octaves);
        noiseShader.SetFloat("scale", scale);
        noiseShader.SetFloat("persistence", persistence);
        noiseShader.SetFloat("lacunarity", lacunarity);
        noiseShader.SetFloat("groundLevel", groundLevel);
        noiseShader.SetFloat("seed", seed);

        int numThreadsXZ = Mathf.CeilToInt(width / 8);
        int numThreadsY = Mathf.CeilToInt(height / 8);

        noiseShader.Dispatch(0, numThreadsXZ, numThreadsY, numThreadsXZ);

        valuesBuffer.GetData(noiseValues);

        valuesBuffer.Release();

        return noiseValues;
    }
}
