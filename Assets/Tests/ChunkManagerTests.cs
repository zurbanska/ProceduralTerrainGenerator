using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.TestTools;

public class ChunkManagerTests
{
    private TerrainData InitializeTerrainData()
    {
        TerrainData td = ScriptableObject.CreateInstance<TerrainData>();

        td.waterLevel = 20;
        td.groundLevel = 20;
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
        td.objectDensity = 20;

        var gradient = new Gradient();

        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(Color.yellow, 0.0f);
        colors[1] = new GradientColorKey(Color.green, 1.0f);

        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);

        gradient.SetKeys(colors, alphas);

        td.gradient = gradient;

        return td;
    }


    private ChunkManager InitializeChunkManager(GameObject go)
    {
        ChunkManager cm = go.AddComponent<ChunkManager>();

        ComputeShader noiseShader = Resources.Load<ComputeShader>("Compute/PerlinNoiseCompute");
        ComputeShader marchingCubesShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
        Material material = Resources.Load<Material>("TerrainMaterial");
        TerrainData terrainData = InitializeTerrainData();

        cm.width = 32;
        cm.height = 128;
        cm.coord = Vector2.zero;

        cm.InitChunk(noiseShader, marchingCubesShader, material, terrainData);

        return cm;
    }


    [Test]
    public void ChunkManager_InitChunk_HandlesEmptyValues()
    {
        var go = new GameObject();
        ChunkManager cm = go.AddComponent<ChunkManager>();

        Assert.DoesNotThrow(() =>
            cm.InitChunk(null, null, new Material(Shader.Find("Standard")), ScriptableObject.CreateInstance<TerrainData>())
        );

    }


    [Test]
    public void ChunkManager_InitChunk_SetsUpComponents()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go); // initchunk runs here

        Assert.NotNull(cm.GetComponent<MeshFilter>());
        Assert.NotNull(cm.GetComponent<MeshRenderer>());
        Assert.NotNull(cm.GetComponent<MeshCollider>());
        Assert.NotNull(cm.GetComponent<ObjectPlacer>());
    }


    [UnityTest]
    public IEnumerator ChunkManager_GenerateMesh_ReturnsMesh()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go);

        Task<Mesh> meshTask = cm.GenerateMesh(true);

        yield return new WaitUntil(() => meshTask.IsCompleted);

        Assert.NotNull(meshTask.Result);
    }


    [Test]
    public void ChunkManager_SetMesh_SetsCorrectMesh()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go);
        Mesh mesh = new Mesh();

        cm.SetMesh(mesh);

        Assert.AreEqual(mesh, go.GetComponent<MeshFilter>().mesh);
    }


    [Test]
    public void ChunkManager_SetMesh_IgnoresEmptyMesh()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go);
        Mesh mesh = new Mesh();

        cm.SetMesh(mesh);
        cm.SetMesh(null);

        Assert.AreEqual(mesh, go.GetComponent<MeshFilter>().mesh);
    }


    [Test]
    public void ChunkManager_DisableChunk_DisablesChunk()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go);

        cm.DisableChunk();

        Assert.IsFalse(go.activeInHierarchy);
    }


    [UnityTest]
    public IEnumerator ChunkManager_DestroyChunk_DestroysGameObject()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go);

        cm.DestroyChunk();
        yield return null;

        Assert.IsTrue(go == null || !go);
    }


    [Test]
    public void ChunkManager_UpdateChunk_ActivatesDisabledChunk()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go);

        cm.DisableChunk();
        cm.UpdateChunk(Vector4.zero, cm.terrainData, false);

        Assert.IsTrue(go.activeInHierarchy);
    }


    [UnityTest]
    public IEnumerator ChunkManager_UpdateChunk_GeneratesMesh()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go);
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        TerrainData terrainData = cm.terrainData;

        cm.UpdateChunk(Vector4.zero, terrainData, false);

        yield return new WaitUntil(() => meshFilter.mesh != null);

        Assert.IsNotNull(meshFilter.mesh);
    }


    [UnityTest]
    public IEnumerator ChunkManager_UpdateChunk_GeneratesNewMesh()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go);
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        TerrainData terrainData = cm.terrainData;
        float startTime = Time.time;

        cm.UpdateChunk(Vector4.zero, terrainData, false);
        yield return new WaitUntil(() => meshFilter.mesh != null);
        Mesh mesh = meshFilter.mesh;

        cm.UpdateChunk(Vector4.one, terrainData, true);
        yield return new WaitUntil(() => meshFilter.mesh != mesh || Time.time > startTime + 3f);
        Mesh newMesh = meshFilter.mesh;

        Assert.AreNotEqual(mesh, newMesh);
    }


    [UnityTest]
    public IEnumerator ChunkManager_Terraform_UpdatesDensityValues()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go);
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        TerrainData terrainData = cm.terrainData;
        float startTime = Time.time;

        cm.UpdateChunk(Vector4.zero, terrainData, false);
        yield return new WaitUntil(() => cm.densityValues != null);
        float[] densityValues = cm.densityValues;

        Vector3 hitPoint = new Vector3(0,0,0);
        float brushSize = 10f;
        cm.Terraform(hitPoint, brushSize, 1f, true, new Bounds(hitPoint, Vector3.one * brushSize));
        yield return new WaitUntil(() => cm.densityValues != densityValues || Time.time > startTime + 3f);
        float[] newDensityValues = cm.densityValues;

        Assert.AreNotEqual(densityValues, newDensityValues);
    }


    [UnityTest]
    public IEnumerator ChunkManager_Terraform_UpdatesMesh()
    {
        var go = new GameObject();
        ChunkManager cm = InitializeChunkManager(go);
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        TerrainData terrainData = cm.terrainData;
        float startTime = Time.time;

        cm.UpdateChunk(Vector4.zero, terrainData, false);
        yield return new WaitUntil(() => meshFilter.mesh != null);
        Mesh mesh = meshFilter.mesh;

        Vector3 hitPoint = new Vector3(0,0,0);
        float brushSize = 10f;
        cm.Terraform(hitPoint, brushSize, 1f, true, new Bounds(hitPoint, Vector3.one * brushSize));
        yield return new WaitUntil(() => meshFilter.mesh != mesh || Time.time > startTime + 3f);
        Mesh newMesh = meshFilter.mesh;

        Assert.AreNotEqual(mesh, newMesh);
    }

}
