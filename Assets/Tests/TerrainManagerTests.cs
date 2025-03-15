using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TerrainManagerTests
{

    private TerrainManager InitializeTerrainManager(GameObject go)
    {
        TerrainManager tm = go.AddComponent<TerrainManager>();

        ComputeShader noiseShader = Resources.Load<ComputeShader>("Compute/PerlinNoiseCompute");
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        Material material = Resources.Load<Material>("TerrainMaterial");
        TerrainData terrainData = Resources.Load<TerrainData>("DefaultTerrainData");

        tm.Initialize(noiseShader, marchingCubesShader, material, terrainData);
        return tm;
    }


    [Test]
    public void TerrainManager_SavesChunksToDictionary()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);
        int chunkNum = (int)Math.Pow((tm.renderDistance * 2) - 1, 2);

        tm.UpdateChunks();

        Assert.AreEqual(chunkNum, tm.terrainChunkDictionary.Count);
    }


    [UnityTest]
    public IEnumerator TerrainManager_CreatesChunkObjects()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);
        int chunkNum = (int)Math.Pow((tm.renderDistance * 2) - 1, 2);

        tm.UpdateChunks();
        yield return null;

        int chunkCount = 0;
        foreach (Transform child in go.transform)
        {
            if (child.name.Contains("Terrain Chunk"))
            {
                chunkCount++;
            }
        }

        Assert.AreEqual(chunkNum, chunkCount);
    }


    [Test]
    public void TerrainManager_UpdateChunks_GeneratesChunksWithinRenderDistance()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);
        tm.renderDistance = 1;
        int newRenderDistance = tm.renderDistance + 1;

        tm.UpdateChunks();
        tm.renderDistance = newRenderDistance;
        tm.UpdateChunks();

        int chunkNum = (int)Math.Pow((newRenderDistance * 2) - 1, 2);

        Assert.AreEqual(chunkNum, tm.terrainChunkDictionary.Count);
    }


    [Test]
    public void TerrainManager_UpdateChunks_DisablesChunksOutsideRenderDistance()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);
        tm.renderDistance = 2;
        int newRenderDistance = tm.renderDistance - 1;

        tm.UpdateChunks();
        tm.renderDistance = newRenderDistance;
        tm.UpdateChunks();

        Vector2 disabledChunkCoord = new Vector2(1, 1);

        Assert.IsFalse(tm.terrainChunkDictionary[disabledChunkCoord].activeInHierarchy);
    }


    [Test]
    public void TerrainManager_CreateChunk_HandlesDuplicateCoordinates()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);

        Vector2 coord = new Vector2(1,1);
        Vector4 neighbors = Vector4.zero;

        tm.CreateChunk(coord, neighbors);
        tm.CreateChunk(coord, neighbors);

        Assert.AreEqual(1, tm.terrainChunkDictionary.Count);
    }


    [Test]
    public void TerrainManager_ValidateSettings_ClampsValues()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);

        tm.chunkWidth = 7;
        tm.chunkHeight = 19;
        tm.renderDistance = 11;
        tm.terrainData.waterLevel = -10;
        tm.terrainData.smoothLevel = -10;
        tm.terrainData.lod = -1;
        tm.terrainData.isoLevel = -1;
        tm.terrainData.octaves = 0;

        tm.ValidateSettings();

        Assert.AreEqual(8, tm.chunkWidth);
        Assert.AreEqual(16, tm.chunkHeight);
        Assert.AreEqual(10, tm.renderDistance);
        Assert.AreEqual(0, tm.terrainData.waterLevel);
        Assert.AreEqual(0, tm.terrainData.smoothLevel);
        Assert.AreEqual(1, tm.terrainData.lod);
        Assert.AreEqual(0, tm.terrainData.isoLevel);
        Assert.AreEqual(1, tm.terrainData.octaves);
    }


    [Test]
    public void TerrainManager_UpdateChunks_GeneratesWaterIfWaterLevelMoreThanZero()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);
        tm.terrainData.waterLevel = 10;

        tm.UpdateChunks();

        Assert.IsNotNull(go.transform.Find("Water"));
    }


    [Test]
    public void TerrainManager_UpdateChunks_DoesntGenerateWaterIfWaterLevelIsZero()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);
        tm.terrainData.waterLevel = 0;

        tm.UpdateChunks();

        Assert.IsNull(go.transform.Find("Water"));
    }


    [UnityTest]
    public IEnumerator TerrainManager_UpdateChunks_OnlyOneWaterObjectExists()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);
        tm.terrainData.waterLevel = 10;

        tm.UpdateChunks();
        yield return null;

        tm.terrainData.waterLevel = 20;
        tm.UpdateChunks();
        yield return null;

        int waterCount = 0;
        foreach (Transform child in go.transform)
        {
            if (child.name == "Water")
            {
                waterCount++;
            }
        }

        Assert.AreEqual(1, waterCount);
    }


    [Test]
    public void TerrainManager_DeleteChunks_ClearsChunkDictionary()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);

        tm.UpdateChunks();
        tm.DeleteChunks();

        Assert.IsTrue(tm.terrainChunkDictionary.Count == 0);
    }


    [UnityTest]
    public IEnumerator TerrainManager_DeleteChunks_DestroysChunkObjects()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);

        tm.UpdateChunks();
        tm.DeleteChunks();
        yield return null;

        Assert.IsNull(go.transform.Find("Terrain Chunk"));
    }


    [Test]
    public void TerrainManager_ModifyTerrain_ReturnsIfTerraformingNotAllowed()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);
        tm.allowTerraforming = false;

        Vector3 hitPoint = new Vector3(0,0,0); // inside center chunk
        float brushSize = 1f;
        float brushStrength = 1f;

        var mockChunk = go.AddComponent<MockChunkManager>();
        tm.terrainChunkDictionary[new Vector2(0, 0)] = mockChunk.gameObject;

        tm.ModifyTerrain(hitPoint, brushSize, brushStrength, true);

        Assert.IsFalse(mockChunk.wasCalled);

    }


    [Test]
    public void TerrainManager_ModifyTerrain_CallsTerraform()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);
        tm.allowTerraforming = true;

        Vector3 hitPoint = new Vector3(0,0,0); // inside center chunk
        float brushSize = 1f;
        float brushStrength = 1f;

        var mockChunk = go.AddComponent<MockChunkManager>();
        tm.terrainChunkDictionary[new Vector2(0, 0)] = mockChunk.gameObject;

        tm.ModifyTerrain(hitPoint, brushSize, brushStrength, true);

        Assert.IsTrue(mockChunk.wasCalled);
    }

    [Test]
    public void TerrainManager_ModifyTerrain_CallsTerraformInCorrectChunks()
    {
        var go = new GameObject();
        TerrainManager tm = InitializeTerrainManager(go);
        tm.allowTerraforming = true;

        Vector3 hitPoint = new Vector3(0,0,0); // inside center chunk
        float brushSize = 1f;
        float brushStrength = 1f;

        var mockChunk = go.AddComponent<MockChunkManager>();
        var mockChunk2 = go.AddComponent<MockChunkManager>();
        tm.terrainChunkDictionary[new Vector2(0, 0)] = mockChunk.gameObject;
        tm.terrainChunkDictionary[new Vector2(1, 0)] = mockChunk.gameObject;

        tm.ModifyTerrain(hitPoint, brushSize, brushStrength, true);

        Assert.IsFalse(mockChunk2.wasCalled);
    }


    public class MockChunkManager : ChunkManager
    {
        public bool wasCalled = false;

        public override async void Terraform(Vector3 hitPosition, float brushSize, float brushStrength, bool add, Bounds brushBounds)
        {
            wasCalled = true;
            await Task.Yield();
        }
    }


}
