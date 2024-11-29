using System;
using System.Collections;
using System.Collections.Generic;
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
    public Vector4 neighbors;

    NoiseData noiseData;

    public TerrainChunk(Vector2 coord, Transform parent, int width, int height, ComputeShader noiseShader, ComputeShader meshShader, Material material, Gradient gradient, float seed, NoiseData noiseData, Vector4 neighbors)
    {
        this.coord = coord;
        this.width = width;
        this.height = height;

        this.material = material;
        this.gradient = gradient;
        this.seed = seed;

        this.neighbors = neighbors;

        noiseGenerator = new NoiseGenerator(noiseShader, noiseData);
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

        this.noiseData = noiseData;

        GenerateWaterPlanes();
    }

    // public LodMesh GenerateMesh(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod)
    // {
    //     densityValues ??= noiseGenerator.GenerateNoise(width + 1, height + 1, new Vector2(coord.x * width, coord.y * width), octaves, persistence, lacunarity, scale, groundLevel, seed);

    //     Mesh mesh = meshGenerator.GenerateMesh(width + 1, height + 1, isoLevel, densityValues, meshLod, gradient);
    //     LodMesh lodMesh = new LodMesh(meshLod, mesh);

    //     return lodMesh;
    // }

    private void GenerateWaterPlanes()
    {
        float shrinkFactor = width * 0.005f;

        GenerateWaterPlane(new Vector3(0, 0, 0), new Vector3(width, 0, 0), new Vector3(0, 0, width), new Vector3(width, 0, width));
        if (neighbors[0] == 0) GenerateWaterPlane( // down edge water plane
            new Vector3(width, -noiseData.waterLevel, 0),
            new Vector3(0, -noiseData.waterLevel, 0),
            new Vector3(width, 0, 0),
            new Vector3(0, 0, 0)
        );
        if (neighbors[1] == 0) GenerateWaterPlane( // right edge water plane
            new Vector3(width, -noiseData.waterLevel, 0),
            new Vector3(width, -noiseData.waterLevel, width),
            new Vector3(width, 0, 0),
            new Vector3(width, 0, width)
        );
        if (neighbors[2] == 0) GenerateWaterPlane( // up edge water plane
            new Vector3(width, -noiseData.waterLevel, width),
            new Vector3(0, -noiseData.waterLevel, width),
            new Vector3(width, 0, width),
            new Vector3(0, 0, width)
        );
        if (neighbors[3] == 0) GenerateWaterPlane( // left edge water plane
            new Vector3(0, -noiseData.waterLevel, 0),
            new Vector3(0, -noiseData.waterLevel, width),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, width)
        );
    }

    private void GenerateWaterPlane(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        GameObject waterPlane = new GameObject("Water");
        waterPlane.transform.position = new Vector3(chunk.transform.position.x, noiseData.waterLevel, chunk.transform.position.z);
        waterPlane.transform.parent = chunk.transform;

        waterPlane.AddComponent<CubemapRenderer>();
        MeshFilter meshFilter = waterPlane.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = waterPlane.AddComponent<MeshRenderer>();

        meshRenderer.material = Resources.Load<Material>("WaterMaterial");
        waterPlane.layer = 4; // water layer
        meshFilter.mesh = GeneratePlaneMesh(v1, v2, v3, v4);

        material.SetFloat("_WaterLevel", noiseData.waterLevel); // terrain material
        meshRenderer.sharedMaterial.SetFloat("_WaterLevel", noiseData.waterLevel); // water material
    }

    private Mesh GeneratePlaneMesh(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        // vertices[0] = new Vector3(0, 0, 0); // bottom-left
        // vertices[1] = new Vector3(width, 0, 0); // bottom-right
        // vertices[2] = new Vector3(0, 0, depth); // top-left
        // vertices[3] = new Vector3(width, 0, depth); // top-right
        vertices[0] = v1;
        vertices[1] = v2;
        vertices[2] = v3;
        vertices[3] = v4;

        int[] triangles = new int[]
        {
            0, 2, 1, // first triangle
            1, 2, 0, // first triangle - reverse
            2, 3, 1,  // second triangle
            1, 3, 2  // second triangle - reverse
        };

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        return mesh;
    }

    public LodMesh GenerateMeshAsync(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod)
    {
        if (densityValues == null)
        {
            densityValues = noiseGenerator.GenerateNoise(width + 1, height + 1, new Vector2(coord.x * width, coord.y * width), octaves, persistence, lacunarity, scale, groundLevel, seed, neighbors);
            AsyncGPUReadback.WaitAllRequests();
        }
        meshGenerator.CreateBuffers(width + 1, height + 1);
        Mesh mesh = meshGenerator.GenerateMesh(width + 1, height + 1, isoLevel, densityValues, meshLod, gradient);

        AsyncGPUReadback.WaitAllRequests();

        return new LodMesh(meshLod, mesh);
    }

    public void RequestMeshData(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod, Action<LodMesh> callback)
    {
        // Run the mesh generation asynchronously
        CoroutineRunner.Instance.RunCoroutine(GenerateMeshDataAsync(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, meshLod, callback));
    }

    private IEnumerator GenerateMeshDataAsync(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod, Action<LodMesh> callback)
    {
        // Ensure density values are initialized
        if (densityValues == null)
        {
            densityValues = noiseGenerator.GenerateNoise(
                width + 1,
                height + 1,
                new Vector2(coord.x * width, coord.y * width),
                octaves,
                persistence,
                lacunarity,
                scale,
                groundLevel,
                seed,
                neighbors);

            // Wait for GPU readback to complete
            AsyncGPUReadback.WaitAllRequests();

        }

        // Prepare mesh generator buffers
        meshGenerator.CreateBuffers(width + 1, height + 1);

        // Generate the mesh
        Mesh mesh = null;

        yield return new WaitUntil(() =>
        {
            mesh = meshGenerator.GenerateMesh(
                width + 1,
                height + 1,
                isoLevel,
                densityValues,
                meshLod,
                gradient);

            return true;
        });


        // Invoke the callback with the generated mesh data
        LodMesh result = new LodMesh(meshLod, mesh);
        callback?.Invoke(result);
    }

    void OnMeshDataReceived(LodMesh lodMesh)
    {
        SetMesh(lodMesh);
    }


    public void DisableChunk()
    {
        chunk.SetActive(false);
    }

    public void EnableChunk()
    {
        chunk.SetActive(true);
    }


    public void EnableChunkLOD(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod, Dictionary<Vector2, int> neighborLods, Vector4 neighbors)
    {

        this.neighbors = neighbors;
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
        SetMesh(newMesh);

        // doesnt work in editor, only in game mode
        // RequestMeshData(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, meshLod, OnMeshDataReceived);

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
        UnityEngine.Object.DestroyImmediate(chunk);
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



    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var runnerGameObject = new GameObject("CoroutineRunner");
                    _instance = runnerGameObject.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(runnerGameObject);
                }
                return _instance;
            }
        }

        public void RunCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }

}
