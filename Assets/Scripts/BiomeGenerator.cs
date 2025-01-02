using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator
{

    public float[] GenerateBiomes(int width, Vector2 offset, float seed)
    {
        float[] biomes = new float[width * width];

        float scale = 100;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                float sampleX = (x + offset.x + seed) / scale;
                float sampleZ = (z + offset.y + seed) / scale;
                biomes[z * width + x] = Mathf.PerlinNoise(sampleX, sampleZ);
            }
        }

        return biomes;
    }
}
