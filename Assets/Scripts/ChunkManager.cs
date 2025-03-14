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
    private BiomeGenerator biomeGenerator;
    private GradientBuilder gradientBuilder;

    private ObjectPlacer objectPlacer;

    private Material material;

    private Mesh mesh;
    public float[] densityValues;
    public float[] biomeValues;

    TerrainData terrainData;

    Vector4 neighbors;

    public Bounds bounds;


    public void InitChunk(ComputeShader noiseShader, ComputeShader meshShader, Material material, TerrainData terrainData, Vector4 neighbors)
    {
        this.material = material;
        this.terrainData = ScriptableObject.CreateInstance<TerrainData>();

        this.gradientBuilder = new GradientBuilder();

        this.material.SetTexture("_GradientTex", gradientBuilder.GenerateGradientTexture(terrainData.gradient));

        noiseGenerator = new NoiseGenerator(noiseShader);
        meshGenerator = new MeshGenerator(meshShader);
        biomeGenerator = new BiomeGenerator();

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();

        objectPlacer = gameObject.AddComponent<ObjectPlacer>();

        bounds = new Bounds(new Vector3(width / 2, height / 2, width / 2) + transform.position, new Vector3(width, height, width));

        UpdateChunk(neighbors, terrainData, true);

    }

    public async void UpdateChunk(Vector4 neighbors, TerrainData newTerrainData, bool needsNewNoise)
    {
        gameObject.SetActive(true);

        this.neighbors = neighbors;

        terrainData = newTerrainData;
        Mesh newMesh = await GenerateMesh(needsNewNoise);
        SetMesh(newMesh);

        material.SetFloat("_WaterLevel", terrainData.waterLevel);

        objectPlacer.PlaceObjects(terrainData);
    }

    private void SetMesh(Mesh newMesh)
    {
        if (this == null) return;

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



    private Task<Mesh> GenerateMesh(bool needsNewNoise)
    {
        if (densityValues == null || needsNewNoise)
        {
            // check if chunk needs noise update
            biomeValues = biomeGenerator.GenerateBiomes(width + 1, new Vector2(coord.x * width, coord.y * width), terrainData.seed);

            densityValues = noiseGenerator.GenerateNoise(width + 1, height + 1, new Vector2(coord.x * width, coord.y * width), terrainData, neighbors, biomeValues);
            AsyncGPUReadback.WaitAllRequests();

        }

        meshGenerator.CreateBuffers(width + 1, height + 1);

        Task<Mesh> mesh = meshGenerator.GenerateMesh(width + 1, height + 1, terrainData.isoLevel, densityValues, terrainData.lod, biomeValues);
        AsyncGPUReadback.WaitAllRequests();

        return mesh;
    }

    public void DisableChunk()
    {
        gameObject.SetActive(false);
    }

    public void DestroyChunk()
    {
        Destroy(gameObject);
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
