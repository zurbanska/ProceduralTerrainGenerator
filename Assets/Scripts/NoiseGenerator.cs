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

        noiseShader.Dispatch(0, width / 8, height / 8, width / 8);

        valuesBuffer.GetData(noiseValues);

        valuesBuffer.Release();

        // for (int x = 0; x < width; x++)
        // {
        //     for (int y = 0; y < height ; y++)
        //     {
        //         for (int z = 0; z < width ; z++)
        //         {
        //             float globalX = (x + offset.x) * 0.05f;
        //             float globalZ = (z + offset.y) * 0.05f;
        //             float currentHeight = height * Mathf.PerlinNoise(globalX, globalZ);
        //             // currentHeight = Random.value;

        //             float density = 0;

        //             if (y <= currentHeight - 0.5f)
        //             {
        //                 density = 0f;
        //             }
        //             else if (y > currentHeight + 0.5f)
        //             {
        //                 density = 1f;
        //             }
        //             else if (y > currentHeight)
        //             {
        //                 density = y - currentHeight;
        //             }
        //             else if (y < currentHeight)
        //             {
        //                 density = currentHeight - y;
        //             }

        //             noiseValues[x + width * (y + height * z)] = density;
        //         }
        //     }
        // }

        return noiseValues;
    }
}
