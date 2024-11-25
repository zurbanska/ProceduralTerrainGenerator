using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class TerrainChunk
{

    private GameObject chunk;

    public Vector2 coord;
    public int width;
    public int height;

    public MeshGenerator meshGenerator;
    public NoiseGenerator noiseGenerator;

    public Material material;
    public Gradient gradient;
    public float seed;

    public int lod;
    public Mesh mesh;
    public List<LodMesh> LodMeshes = new List<LodMesh>();

    public float[] densityValues;

    public Dictionary<Vector2, int> neighborLods = new Dictionary<Vector2, int>();


    public TerrainChunk(Vector2 coord, Transform parent, int width, int height, ComputeShader noiseShader, ComputeShader meshShader, Material material, Gradient gradient, float seed)
    {
        this.coord = coord;
        this.width = width;
        this.height = height;

        this.material = material;
        this.gradient = gradient;
        this.seed = seed;

        noiseGenerator = new NoiseGenerator(noiseShader);
        meshGenerator = new MeshGenerator(meshShader);

        lod = -1;

        // create chunk object
        chunk = new GameObject("Terrain Chunk " + coord);
        chunk.transform.parent = parent;
        chunk.transform.position = new Vector3(coord.x * width, 0, coord.y * width);

        chunk.AddComponent<MeshFilter>();
        chunk.AddComponent<MeshRenderer>();
        chunk.AddComponent<MeshCollider>();
        Rigidbody rb = chunk.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }


    // public LodMesh GenerateMesh(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod)
    // {
    //     densityValues ??= noiseGenerator.GenerateNoise(width + 1, height + 1, new Vector2(coord.x * width, coord.y * width), octaves, persistence, lacunarity, scale, groundLevel, seed);

    //     Mesh mesh = meshGenerator.GenerateMesh(width + 1, height + 1, isoLevel, densityValues, meshLod, gradient);
    //     LodMesh lodMesh = new LodMesh(meshLod, mesh);

    //     return lodMesh;
    // }

    public LodMesh GenerateMeshAsync(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod)
    {
        if (densityValues == null)
        {
            densityValues = noiseGenerator.GenerateNoise(width + 1, height + 1, new Vector2(coord.x * width, coord.y * width), octaves, persistence, lacunarity, scale, groundLevel, seed);
            AsyncGPUReadback.WaitAllRequests();
        }
        meshGenerator.CreateBuffers(width + 1, height + 1);
        Mesh mesh = meshGenerator.GenerateMesh(width + 1, height + 1, isoLevel, densityValues, meshLod, gradient);

        AsyncGPUReadback.WaitAllRequests();

        return new LodMesh(meshLod, mesh);
    }


    public void DisableChunk()
    {
        chunk.SetActive(false);
    }

    public void EnableChunk()
    {
        chunk.SetActive(true);
    }


    public void EnableChunkLOD(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod, Dictionary<Vector2, int> neighborLods)
    {

        foreach (var LodMesh in LodMeshes)
        {
            if (LodMesh.lod == meshLod)
            {
                SetMesh(LodMesh);
                lod = LodMesh.lod;
                chunk.SetActive(true);
                return;
            }
        }

        LodMesh newMesh = GenerateMeshAsync(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, meshLod);

        // Set new mesh
        SetMesh(newMesh);
        lod = meshLod;
        chunk.SetActive(true);
    }


    private void SetMesh(LodMesh lodMesh)
    {
        chunk.GetComponent<MeshFilter>().mesh = lodMesh.mesh;
        chunk.GetComponent<MeshCollider>().sharedMesh = lodMesh.mesh;

        chunk.GetComponent<MeshRenderer>().material = material;
    }


    public bool IsVisible()
    {
        return chunk.activeSelf;
    }

    public void DestroyChunk()
    {
        Object.DestroyImmediate(chunk);
    }

    void SpawnGrass()
    {
        if (mesh == null || lod != 1) return;

        Vector3[] vertices = mesh.vertices; // tutaj musisz zrobic array z kazdego child element mesh vertices i po tym iterowac dopiero
        Vector3[] normals = mesh.normals;

        List<Vector3> grassPositions = new List<Vector3>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertexWorldPos = chunk.transform.TransformPoint(vertices[i]);
            // Vector3 normal = normals[i];
            grassPositions.Add(vertexWorldPos);

        }

        for (int i = 0; i < 10; i++)
        {
            int grassid = Random.Range(0, grassPositions.Count - 1);
            GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grass.transform.parent = chunk.transform;
            grass.transform.position = grassPositions[grassid];
            grass.transform.up = normals[grassid];

        }
    }


    public struct LodMesh
    {
        public int lod;
        public Mesh mesh;

        public LodMesh(int lod, Mesh mesh)
        {
            this.lod = lod;
            this.mesh = mesh;
        }
    }

}
