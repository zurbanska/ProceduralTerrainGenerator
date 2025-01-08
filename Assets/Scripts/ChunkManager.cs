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
    private WaterGenerator waterGenerator;

    private Material material;
    private Gradient gradient;
    private float seed;

    public int lod;
    private Mesh mesh;
    public float[] densityValues;
    public float[] biomeValues;

    NoiseData noiseData;

    Vector4 neighbors;


    private float isoLevel;
    private int octaves;
    private float persistence;
    private float lacunarity;
    private float scale;
    private float groundLevel;

    public Bounds bounds;


    public void InitChunk(ComputeShader noiseShader, ComputeShader meshShader, Material material, Gradient gradient, NoiseData noiseData, float seed)
    {
        this.material = material;
        this.gradient = gradient;
        this.noiseData = noiseData;
        this.seed = seed;

        noiseGenerator = new NoiseGenerator(noiseShader, noiseData);
        meshGenerator = new MeshGenerator(meshShader);
        waterGenerator = new WaterGenerator();
        biomeGenerator = new BiomeGenerator();

        lod = -1;

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();

        bounds = new Bounds(new Vector3(width / 2, height / 2, width / 2) + transform.position, new Vector3(width, height, width));
    }

    public async void UpdateChunk(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod, Vector4 neighbors, TerrainData terrainData)
    {
        this.neighbors = neighbors;
        this.isoLevel = isoLevel;
        this.octaves = octaves;
        this.persistence = persistence;
        this.lacunarity = lacunarity;
        this.scale = scale;
        this.groundLevel = groundLevel;

        if (lod != meshLod && noiseData.waterLevel > 0) waterGenerator.GenerateWater(gameObject.transform, width, noiseData.waterLevel, meshLod, neighbors);
        lod = meshLod;

        Mesh newMesh = await GenerateMesh(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, meshLod, true);
        SetMesh(newMesh);

        material.SetFloat("_WaterLevel", noiseData.waterLevel);
    }

    private void SetMesh(Mesh newMesh)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        if (meshFilter != null)
            meshFilter.mesh = newMesh;

        if (meshCollider != null)
            meshCollider.sharedMesh = newMesh;

        GetComponent<MeshRenderer>().sharedMaterial = material;

        mesh = newMesh;
    }



    private Task<Mesh> GenerateMesh(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod, bool needsNewNoise)
    {
        if (densityValues == null || needsNewNoise)
        {
        // check if chunk needs noise update
            biomeValues = biomeGenerator.GenerateBiomes(width + 1, new Vector2(coord.x * width, coord.y * width), seed);

            densityValues = noiseGenerator.GenerateNoise(width + 1, height + 1, new Vector2(coord.x * width, coord.y * width), octaves, persistence, lacunarity, scale, groundLevel, seed, neighbors, lod, biomeValues);
            AsyncGPUReadback.WaitAllRequests();

        }

        meshGenerator.CreateBuffers(width + 1, height + 1);

        Task<Mesh> mesh = meshGenerator.GenerateMesh(width + 1, height + 1, isoLevel, densityValues, meshLod, gradient, biomeValues);
        AsyncGPUReadback.WaitAllRequests();

        return mesh;
    }

    public void DisableChunk()
    {
        gameObject.SetActive(false);
    }

    public void DestroyChunk()
    {
        UnityEngine.Object.DestroyImmediate(gameObject);
    }

    public async void Terraform(Vector3 hitPosition, float brushSize, bool add)
    {
        meshGenerator.CreateBuffers(width + 1, height + 1);
        densityValues = meshGenerator.UpdateDensity(width + 1, height + 1, densityValues, hitPosition, brushSize, add, neighbors);
        AsyncGPUReadback.WaitAllRequests();

        Mesh newMesh = await GenerateMesh(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, lod, false);
        SetMesh(newMesh);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }



}
