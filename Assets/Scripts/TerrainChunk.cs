using System;
using System.Collections;
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
                seed);

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
