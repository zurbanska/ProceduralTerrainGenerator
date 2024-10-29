using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    // shaders used for mesh generating
    public ComputeShader noiseShader;
    public ComputeShader marchingCubesShader;


    public int chunkWidth = 8; // points per x & z axis
    public int chunkHeight = 8; // points per y axis
    public float isoLevel = 0.6f;
    public float groundLevel = 0;

    public int renderDistance;

    public int lod = 1;

    public int octaves = 3;
    public float persistence = 0.5f;
    public float lacunarity = 0.4f;
    public float scale = 1;


    [SerializeField] private Transform viewer;
    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>(); // distionary of all created chunks and their coords
    private List<Vector2> chunksVisibleLastUpdate = new List<Vector2>(); // list of chunk coords that were visible last update
    private Vector2 lastChunkCoord;


    // Start is called before the first frame update
    void Start()
    {
        GenerateChunks();
    }

    // Update is called once per frame
    void Update()
    {
        // get viewer's chunk coord
        Vector2 currentChunkCoord = new Vector2(Mathf.FloorToInt(viewer.transform.position.x / chunkWidth), Mathf.FloorToInt(viewer.transform.position.z / chunkWidth));

        if (currentChunkCoord != lastChunkCoord) // only update chunks if viewer moved chunks
        {
            lastChunkCoord = currentChunkCoord;

            // disable chunks that were visible last update but are beyond render distance now
            foreach (var chunkCoord in chunksVisibleLastUpdate)
            {
                if ((currentChunkCoord - chunkCoord).sqrMagnitude >= (renderDistance * renderDistance))
                {
                    DisableChunk(chunkCoord);
                }
            }

            chunksVisibleLastUpdate.Clear();

            // enable/disable chunks based on render distance
            for (int i = -renderDistance; i < renderDistance; i++)
            {
                for (int j = -renderDistance; j < renderDistance; j++)
                {
                    if ((i * i) + (j * j) < (renderDistance * renderDistance))
                    {
                        EnableChunk(new Vector2(i, j) + currentChunkCoord, (i * i) + (j * j));
                        chunksVisibleLastUpdate.Add(new Vector2(i, j) + currentChunkCoord);
                    } else DisableChunk(new Vector2(i, j) + currentChunkCoord);
                }
            }

        }

    }



    public void GenerateChunks()
    {
        DeleteChunks();

        Vector2 viewerPosition = new Vector2(Mathf.FloorToInt(viewer.transform.position.x / chunkWidth), Mathf.FloorToInt(viewer.transform.position.z / chunkWidth));

        // generate chunks that are in render distance
        for (int i = -renderDistance; i < renderDistance; i++)
            {
                for (int j = -renderDistance; j < renderDistance; j++)
                {
                    if ((i * i) + (j * j) < (renderDistance * renderDistance))
                    {
                        EnableChunk(new Vector2(i, j) + viewerPosition, (i * i) + (j * j));
                    }
                }
            }
    }


    public void CreateChunk(Vector2 coord)
    {
        TerrainChunk newChunk = new TerrainChunk(coord, transform, chunkWidth, chunkHeight, noiseShader, marchingCubesShader);
        newChunk.GenerateMesh(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, lod);
        terrainChunkDictionary.Add(new Vector2(coord.x, coord.y), newChunk);
    }

    private void DeleteChunks()
    {
        terrainChunkDictionary.Clear();
        chunksVisibleLastUpdate.Clear();
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    private void DisableChunk(Vector2 coord)
    {
        if (terrainChunkDictionary.ContainsKey(coord))
        {
            terrainChunkDictionary[coord].DisableChunk();
        }
    }

    private void EnableChunk(Vector2 coord, float distance)
    {
            if (terrainChunkDictionary.ContainsKey(coord))
            {
                // terrainChunkDictionary[coord].GenerateMesh(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, lod);
                terrainChunkDictionary[coord].EnableChunk();
            } else CreateChunk(coord);
    }


    private void OnValidate() {

        // chunk width and height have to be multiples of 8 and greater or equal 8
        chunkWidth = Mathf.Max(8, chunkWidth);
        if (chunkWidth % 8 != 0)
        {
            chunkWidth = Mathf.RoundToInt(chunkWidth / 8.0f) * 8;
        }

        chunkHeight = Mathf.Max(chunkHeight, chunkWidth);
        if (chunkHeight % 8 != 0)
        {
            chunkHeight = Mathf.RoundToInt(chunkHeight / 8.0f) * 8;
        }

        // level of detail reduction has to be less than half of chunk width
        if (lod > chunkWidth / 2)
        {
            lod = chunkWidth / 2;
        }
        if (lod < 1)
        {
            lod = 1;
        }
    }


    public struct TerrainChunk
    {
        GameObject chunkObject;

        private Vector2 coord;
        // private Mesh mesh;
        int width;
        int height;
        ComputeShader noiseShader;
        ComputeShader marchingCubesShader;

        ChunkManager chunkManager;
        // private int lod;
        // private int meshLod;
        // private bool isVisible;

        public TerrainChunk(Vector2 coord, Transform parent, int width, int height, ComputeShader noiseShader, ComputeShader marchingCubesShader)
        {
            this.coord = coord;
            this.width = width;
            this.height = height;
            this.noiseShader = noiseShader;
            this.marchingCubesShader = marchingCubesShader;

            chunkObject = new GameObject("Terrain Chunk");
            chunkObject.transform.parent = parent.transform;
            chunkObject.transform.position = new Vector3(coord.x * width, 0, coord.y * width);

            chunkObject.AddComponent<MeshFilter>();
            chunkObject.AddComponent<MeshRenderer>();

            ChunkManager chunkManager = chunkObject.AddComponent<ChunkManager>();
            chunkManager.noiseGenerator = new NoiseGenerator(noiseShader);
            chunkManager.meshGenerator = new MeshGenerator(marchingCubesShader);
            this.chunkManager = chunkManager;

            // mesh = new Mesh();
        }


        public void GenerateMesh(float isolevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int lod)
        {
            chunkManager.GenerateChunk(coord, width, height, isolevel, octaves, persistence, lacunarity, scale, groundLevel, lod);
        }

        public void DisableChunk()
        {
            chunkObject.SetActive(false);
        }

        public void EnableChunk()
        {
            chunkObject.SetActive(true);
        }


    }


}
