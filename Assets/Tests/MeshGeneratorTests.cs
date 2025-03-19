using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MeshGeneratorTests
{
    [Test]
    public void MeshGenerator_CreateBuffers_CreatesAllBuffers()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);

        meshGenerator.CreateBuffers(16, 16);

        Assert.NotNull(meshGenerator.trianglesBuffer);
        Assert.NotNull(meshGenerator.triangleCountBuffer);
        Assert.NotNull(meshGenerator.densityBuffer);
        Assert.NotNull(meshGenerator.vertexCacheBuffer);
    }


    [Test]
    public void MeshGenerator_CreateBuffers_HandlesNegativeValues()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);

        Assert.DoesNotThrow(() => meshGenerator.CreateBuffers(-1, -1));
    }


    [Test]
    public void MeshGenerator_ReleaseBuffers_ReleasesAllBuffers()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);

        meshGenerator.CreateBuffers(33, 33);
        meshGenerator.ReleaseBuffers();

        Assert.IsTrue(meshGenerator.trianglesBuffer == null || !meshGenerator.trianglesBuffer.IsValid());
        Assert.IsTrue(meshGenerator.triangleCountBuffer == null || !meshGenerator.trianglesBuffer.IsValid());
        Assert.IsTrue(meshGenerator.densityBuffer == null || !meshGenerator.trianglesBuffer.IsValid());
        Assert.IsTrue(meshGenerator.vertexCacheBuffer == null || !meshGenerator.trianglesBuffer.IsValid());
    }

     [Test]
    public void MeshGenerator_ReleaseBuffers_HandlesReleasingEmptyBuffers()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);

        Assert.DoesNotThrow(() => meshGenerator.ReleaseBuffers());
    }



    [Test]
    public void MeshGenerator_GenerateMesh_CreatesBuffersIfNoneCreated()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);
        int width = 33;
        int height = 33;

        float[] densityMap = new float[width * width * height];
        _ = meshGenerator.GenerateMesh(width, height, 0.9f, densityMap, 1);

        Assert.NotNull(meshGenerator.trianglesBuffer);
    }


    [UnityTest]
    public IEnumerator MeshGenerator_GenerateMesh_CreatesMesh()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);
        int width = 33;
        int height = 33;

        float[] densityMap = new float[width * width * height];

        Task<Mesh> meshTask = meshGenerator.GenerateMesh(width, height, 0.9f, densityMap, 1);

        yield return new WaitUntil(() => meshTask.IsCompleted);

        Assert.NotNull(meshTask.Result);
    }


    [UnityTest]
    public IEnumerator MeshGenerator_GenerateMesh_ReturnsExpectedMesh()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);
        int width = 33;
        int height = 33;

        float[] densityMap = new float[width * width * height];
        densityMap[0] = 1; // adding a non-zero value to force a single triangle to generate

        Task<Mesh> meshTask = meshGenerator.GenerateMesh(width, height, 0.9f, densityMap, 1);

        yield return new WaitUntil(() => meshTask.IsCompleted);

        Assert.AreEqual(3, meshTask.Result.vertexCount); // especting single triangle mesh
    }


    [UnityTest]
    public IEnumerator MeshGenerator_GenerateMesh_ReturnsEmptyMeshIfIncorrectChunkSizeRequested()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);
        int width = 2;
        int height = 1;

        float[] densityMap = new float[width * width * height];
        densityMap[0] = 1;

        Task<Mesh> meshTask = meshGenerator.GenerateMesh(width, height, 0.9f, densityMap, 1);

        yield return new WaitUntil(() => meshTask.IsCompleted);

        Assert.AreEqual(0, meshTask.Result.vertexCount);
    }


    [Test]
    public void MeshGenerator_UpdateDensity_HandlesEmptyDensityMap()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);
        int width = 32;
        int height = 32;

        Assert.DoesNotThrow(() => meshGenerator.UpdateDensity(width, height, null, Vector3.zero, 1f, 1f, true, Vector4.zero, 0f));
    }


    [Test]
    public void MeshGenerator_UpdateDensity_HandlesIncorrectChunkSize()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);
        int width = 0;
        int height = 0;

        Assert.DoesNotThrow(() => meshGenerator.UpdateDensity(width, height, null, Vector3.zero, 1f, 1f, true, Vector4.zero, 0f));
    }


    [Test]
    public void MeshGenerator_UpdateDensity_ReturnsExpectedDensityMap()
    {
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        MeshGenerator meshGenerator = new(marchingCubesShader);
        int width = 2;
        int height = 2;
        float brushSize = 8;

        float[] densityMap = new float[width * width * height];

        float[] newDensityMap = meshGenerator.UpdateDensity(width, height, densityMap, Vector3.zero, brushSize, 1f, true, Vector4.zero, 0f);

        // expecting every value to be changed because of the big brush size
        float[] expectedDensityMap = new float[width * width * height];
        for (int i = 0; i < expectedDensityMap.Length; i++) expectedDensityMap[i] = 1;

        Assert.AreEqual(expectedDensityMap, newDensityMap);
    }


}
