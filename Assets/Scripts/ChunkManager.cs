using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class ChunkManager : MonoBehaviour
{

    public Vector2 coord;
    public int width;
    public int height;

    private MeshGenerator meshGenerator;
    private NoiseGenerator noiseGenerator;
    private GradientBuilder gradientBuilder;

    private ObjectPlacer objectPlacer;

    private Material material;

    public Mesh mesh;
    public float[] densityValues;

    public TerrainData terrainData;

    public Vector4 neighbors;

    public Bounds bounds;


    public void InitChunk(ComputeShader noiseShader, ComputeShader meshShader, Material material, TerrainData terrainData)
    {
        this.material = material;
        this.terrainData = terrainData;

        this.gradientBuilder = new GradientBuilder();

        if (this.material.HasProperty("_GradientTex")) this.material.SetTexture("_GradientTex", gradientBuilder.GenerateGradientTexture(terrainData.gradient));

        noiseGenerator = new NoiseGenerator(noiseShader);
        meshGenerator = new MeshGenerator(meshShader);

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();

        objectPlacer = gameObject.AddComponent<ObjectPlacer>();

        bounds = new Bounds(new Vector3(width / 2, height / 2, width / 2) + transform.position, new Vector3(width, height, width));

    }


    public async void UpdateChunk(Vector4 neighbors, TerrainData newTerrainData, bool needsNewNoise)
    {
        gameObject.SetActive(true);

        this.neighbors = neighbors;

        terrainData = newTerrainData;
        Mesh newMesh = await GenerateMesh(needsNewNoise);
        SetMesh(newMesh);

        if (this.material.HasProperty("_WaterLevel")) material.SetFloat("_WaterLevel", terrainData.waterLevel);

        if (objectPlacer != null) objectPlacer.PlaceObjects(terrainData);
    }

    public void SetMesh(Mesh newMesh)
    {
        if (this == null || newMesh == null) return;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter != null)
            meshFilter.mesh = newMesh;

        if (meshCollider != null)
            meshCollider.sharedMesh = newMesh;

        if (meshRenderer != null)
            meshRenderer.sharedMaterial = material;

        mesh = newMesh;
    }



    public Task<Mesh> GenerateMesh(bool needsNewNoise)
    {
        if (densityValues == null || needsNewNoise)
        {
            // check if chunk needs noise update
            densityValues = noiseGenerator.GenerateNoise(width + 1, height + 1, new Vector2(coord.x * width, coord.y * width), terrainData, neighbors);
            AsyncGPUReadback.WaitAllRequests();
        }

        meshGenerator.CreateBuffers(width + 1, height + 1);

        Task<Mesh> mesh = meshGenerator.GenerateMesh(width + 1, height + 1, terrainData.isoLevel, densityValues, terrainData.lod);
        AsyncGPUReadback.WaitAllRequests();

        return mesh;
    }

    public void DisableChunk()
    {
        gameObject.SetActive(false);
    }

    public void DestroyChunk()
    {
        DestroyImmediate(gameObject);
    }

    public virtual async void Terraform(Vector3 hitPosition, float brushSize, float brushStrength, bool add, Bounds brushBounds)
    {
        meshGenerator.CreateBuffers(width + 1, height + 1);
        densityValues = meshGenerator.UpdateDensity(width + 1, height + 1, densityValues, hitPosition, brushSize, brushStrength, add, neighbors, terrainData.smoothLevel);
        AsyncGPUReadback.WaitAllRequests();

        Mesh newMesh = await GenerateMesh(false);
        SetMesh(newMesh);
        objectPlacer.DestroyObjectsInArea(brushBounds);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }



}
