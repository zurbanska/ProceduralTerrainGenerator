using UnityEngine;

public class NoiseGenerator
{

    private ComputeShader noiseShader;
    public ComputeBuffer valuesBuffer;

    public NoiseGenerator(ComputeShader noiseShader)
    {
        this.noiseShader = noiseShader;
    }


    public float[] GenerateNoise(int width, int height, Vector2 offset, TerrainData terrainData , Vector4 neighbors)
    {
        if (width < 1 || height < 1)
        {
            return new float[0];
        }

        float[] noiseValues = new float[width * height * width]; // 1D array of noise values

        valuesBuffer = new ComputeBuffer(width * height * width, sizeof(float));

        noiseShader.SetBuffer(0, "_Values", valuesBuffer);
        noiseShader.SetInt("_ChunkWidth", width);
        noiseShader.SetInt("_ChunkHeight", height);
        noiseShader.SetFloat("_OffsetX", offset.x + terrainData.offsetX);
        noiseShader.SetFloat("_OffsetZ", offset.y + terrainData.offsetZ);
        noiseShader.SetFloat("lod", terrainData.lod);

        noiseShader.SetBool("borderDown", neighbors[0] == 0);
        noiseShader.SetBool("borderRight", neighbors[1] == 0);
        noiseShader.SetBool("borderUp", neighbors[2] == 0);
        noiseShader.SetBool("borderLeft", neighbors[3] == 0);

        noiseShader.SetInt("octaves", terrainData.octaves);
        noiseShader.SetFloat("scale", terrainData.scale);
        noiseShader.SetFloat("persistence", terrainData.persistence);
        noiseShader.SetFloat("lacunarity", terrainData.lacunarity);
        noiseShader.SetFloat("groundLevel", terrainData.groundLevel);
        noiseShader.SetFloat("smoothLevel", terrainData.smoothLevel);
        noiseShader.SetFloat("seed", terrainData.seed);

        int numThreadsXZ = Mathf.CeilToInt(width / 8.0f);
        int numThreadsY = Mathf.CeilToInt(height / 8.0f);

        if (numThreadsXZ <= 0 || numThreadsY <= 0)
        {
            valuesBuffer.Release();
            return noiseValues;
        }

        noiseShader.Dispatch(0, numThreadsXZ, numThreadsY, numThreadsXZ);

        valuesBuffer.GetData(noiseValues);

        valuesBuffer.Release();

        return noiseValues;
    }
}
