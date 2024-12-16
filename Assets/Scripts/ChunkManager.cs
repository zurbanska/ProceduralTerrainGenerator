using UnityEngine;
using UnityEngine.Rendering;

public class ChunkManager : MonoBehaviour
{

    public Vector2 coord;
    public int width;
    public int height;

    private MeshGenerator meshGenerator;
    private NoiseGenerator noiseGenerator;
    private WaterGenerator waterGenerator;

    private Material material;
    private Gradient gradient;
    private float seed;

    public int lod;
    private Mesh mesh;
    public float[] densityValues;

    NoiseData noiseData;

    Vector4 neighbors;


    private float isoLevel;
    private int octaves;
    private float persistence;
    private float lacunarity;
    private float scale;
    private float groundLevel;


    void Start()
    {

    }

    void Update()
    {

    }

    public void InitChunk(ComputeShader noiseShader, ComputeShader meshShader, Material material, Gradient gradient, NoiseData noiseData, float seed)
    {
        this.material = material;
        this.gradient = gradient;
        this.noiseData = noiseData;
        this.seed = seed;

        noiseGenerator = new NoiseGenerator(noiseShader, noiseData);
        meshGenerator = new MeshGenerator(meshShader);
        waterGenerator = new WaterGenerator();

        lod = -1;

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();
    }

    public void UpdateChunk(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod, Vector4 neighbors)
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

        Mesh newMesh = GenerateMesh(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, meshLod);
        SetMesh(newMesh);

        material.SetFloat("_WaterLevel", noiseData.waterLevel);
    }

    private void SetMesh(Mesh mesh)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        if (meshFilter != null)
            meshFilter.mesh = mesh;

        if (meshCollider != null)
            meshCollider.sharedMesh = mesh;

        GetComponent<MeshRenderer>().sharedMaterial = material;

        this.mesh = mesh;
    }



    private Mesh GenerateMesh(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod)
    {
        if (densityValues == null)
        {
            densityValues = noiseGenerator.GenerateNoise(width + 1, height + 1, new Vector2(coord.x * width, coord.y * width), octaves, persistence, lacunarity, scale, groundLevel, seed, neighbors, lod);
            AsyncGPUReadback.WaitAllRequests();
        }

        meshGenerator.CreateBuffers(width + 1, height + 1);

        Mesh mesh = meshGenerator.GenerateMesh(width + 1, height + 1, isoLevel, densityValues, meshLod, gradient);
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



    public void OnRaycastHit(Vector3 hitPosition, float brushSize, bool add)
    {
        Vector3 chunkHitPosition = new Vector3(hitPosition.x - coord.x * width, hitPosition.y, hitPosition.z - coord.y * width);
        Debug.Log(chunkHitPosition);

        meshGenerator.CreateBuffers(width + 1, height + 1);
        densityValues = meshGenerator.UpdateDensity(width + 1, height + 1, densityValues, chunkHitPosition, brushSize, add);
        AsyncGPUReadback.WaitAllRequests();

        Mesh newMesh = GenerateMesh(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, lod);
        SetMesh(newMesh);
    }
}
