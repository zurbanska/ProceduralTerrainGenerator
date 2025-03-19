using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NoiseGeneratorTests
{

    private TerrainData InitializeTerrainData()
    {
        TerrainData td = ScriptableObject.CreateInstance<TerrainData>();

        td.waterLevel = 1;
        td.groundLevel = 0;
        td.smoothLevel = 0;
        td.lod = 1;
        td.isoLevel = 0.9f;
        td.seed = 0;
        td.octaves = 3;
        td.persistence = 0.7f;
        td.lacunarity = 2;
        td.scale = 10;
        td.offsetX = 0;
        td.offsetZ = 0;
        td.objectDensity = 0;
        td.gradient = new();

        return td;
    }


    [Test]
    public void NoiseGenerator_GenerateNoise_HandlesIncorrectChunkSize()
    {
        ComputeShader noiseShader = Resources.Load<ComputeShader>("Compute/PerlinNoiseCompute");
        TerrainData terrainData = InitializeTerrainData();
        NoiseGenerator noiseGenerator = new(noiseShader);
        int width = -1;
        int height = -1;

        Assert.DoesNotThrow(() => noiseGenerator.GenerateNoise(width, height, Vector2.zero, terrainData, Vector4.zero));
    }


    [Test]
    public void NoiseGenerator_GenerateNoise_CreatesBuffer()
    {
        ComputeShader noiseShader = Resources.Load<ComputeShader>("Compute/PerlinNoiseCompute");
        TerrainData terrainData = InitializeTerrainData();
        NoiseGenerator noiseGenerator = new(noiseShader);
        int width = 33;
        int height = 33;

        noiseGenerator.GenerateNoise(width, height, Vector2.zero, terrainData, Vector4.zero);

        Assert.NotNull(noiseGenerator.valuesBuffer);
    }


    [UnityTest]
    public IEnumerator NoiseGenerator_GenerateNoise_ReleasesBuffer()
    {
        ComputeShader noiseShader = Resources.Load<ComputeShader>("Compute/PerlinNoiseCompute");
        TerrainData terrainData = InitializeTerrainData();
        NoiseGenerator noiseGenerator = new(noiseShader);
        float startTime = Time.time;
        int width = 33;
        int height = 33;

        float[] noiseValues = null;
        noiseValues = noiseGenerator.GenerateNoise(width, height, Vector2.zero, terrainData, Vector4.zero);
        yield return new WaitUntil(() => noiseValues != null || Time.time > startTime + 3f);

        Assert.IsTrue(noiseGenerator.valuesBuffer == null || !noiseGenerator.valuesBuffer.IsValid());
    }


    [UnityTest]
    public IEnumerator NoiseGenerator_GenerateNoise_ReturnsNoiseValues()
    {
        ComputeShader noiseShader = Resources.Load<ComputeShader>("Compute/PerlinNoiseCompute");
        TerrainData terrainData = InitializeTerrainData();
        NoiseGenerator noiseGenerator = new(noiseShader);
        float startTime = Time.time;
        int width = 33;
        int height = 33;

        float[] noiseValues = noiseGenerator.GenerateNoise(width, height, Vector2.zero, terrainData, Vector4.zero);
        yield return new WaitUntil(() => noiseValues != null || Time.time > startTime + 3f);

        foreach (var noiseValue in noiseValues) Debug.Log(noiseValue);

        Assert.NotNull(noiseValues);
        Assert.AreEqual(width * width * height, noiseValues.Length);
    }


    [UnityTest]
    public IEnumerator NoiseGenerator_GenerateNoise_ReturnsDeterministicNoiseValues()
    {
        ComputeShader noiseShader = Resources.Load<ComputeShader>("Compute/PerlinNoiseCompute");
        TerrainData terrainData = InitializeTerrainData();
        NoiseGenerator noiseGenerator = new(noiseShader);
        float startTime = Time.time;

        int width = 33;
        int height = 33;

        float[] noiseValues1 = noiseGenerator.GenerateNoise(width, height, Vector2.zero, terrainData, Vector4.zero);
        yield return new WaitUntil(() => noiseValues1 != null || Time.time > startTime + 3f);

        float[] noiseValues2 = noiseGenerator.GenerateNoise(width, height, Vector2.zero, terrainData, Vector4.zero);
        yield return new WaitUntil(() => noiseValues2 != null || Time.time > startTime + 6f);

        Assert.AreEqual(noiseValues1, noiseValues2); // always expecting the same noise from the same input
    }


    [UnityTest]
    public IEnumerator NoiseGenerator_GenerateNoise_ReturnsPseudoRandomNoiseValues()
    {
        ComputeShader noiseShader = Resources.Load<ComputeShader>("Compute/PerlinNoiseCompute");
        TerrainData terrainData = InitializeTerrainData();
        NoiseGenerator noiseGenerator = new(noiseShader);
        float startTime = Time.time;

        int width = 33;
        int height = 33;

        terrainData.scale = 1f;
        float[] noiseValues1 = noiseGenerator.GenerateNoise(width, height, Vector2.zero, terrainData, Vector4.zero);
        yield return new WaitUntil(() => noiseValues1.Length == width * width * height|| Time.time > startTime + 3f);

        terrainData.scale = 10f;
        float[] noiseValues2 = noiseGenerator.GenerateNoise(width, height, Vector2.zero, terrainData, Vector4.zero);
        yield return new WaitUntil(() => noiseValues2.Length == width * width * height || Time.time > startTime + 6f);

        Assert.AreNotEqual(noiseValues1, noiseValues2); // expecting new noise from new input
    }


}
