using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    // shaders used for mesh generating
    public ComputeShader noiseShader;
    public ComputeShader marchingCubesShader;

    public Material material;
    public Gradient gradient;


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

    public float seed = 0;
    public bool randomSeed = false;


    [SerializeField] private Transform viewer;
    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>(); // dictionary of all created chunks and their coords
    private List<Vector2> chunksVisibleLastUpdate = new List<Vector2>(); // list of chunk coords that were visible last update
    private Vector2 lastChunkCoord;


    public List<LodInfo> lods = new List<LodInfo>();


    // Start is called before the first frame update
    void Start()
    {
        DeleteChunks();
        Vector2 currentChunkCoord = new Vector2(Mathf.FloorToInt(viewer.transform.position.x / chunkWidth), Mathf.FloorToInt(viewer.transform.position.z / chunkWidth));
        lastChunkCoord = currentChunkCoord;
        if (randomSeed) seed = Random.value * 10000000;
        UpdateChunks(currentChunkCoord);
        // UpdateChunks(currentChunkCoord);
    }

    // Update is called once per frame
    void Update()
    {
        // get viewer's chunk coord
        Vector2 currentChunkCoord = new Vector2(Mathf.FloorToInt(viewer.transform.position.x / chunkWidth), Mathf.FloorToInt(viewer.transform.position.z / chunkWidth));

        if (currentChunkCoord != lastChunkCoord) // only update chunks if viewer moved chunks
        {
            lastChunkCoord = currentChunkCoord;
            UpdateChunks(currentChunkCoord);
        }

    }

    void UpdateChunks(Vector2 currentChunkCoord)
    {
        // disable chunks that were visible last update but are beyond render distance now
        foreach (var chunkCoord in chunksVisibleLastUpdate)
        {
            if ((currentChunkCoord - chunkCoord).sqrMagnitude >= renderDistance * renderDistance)
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
                    Vector2 chunkCoord = new Vector2(i, j) + currentChunkCoord;
                    float distance = (i * i) + (j * j);

                    if (distance < renderDistance * renderDistance)
                    {
                        int chunkLod = GetChunkLOD(distance);
                        EnableChunk(chunkCoord, chunkLod);
                        chunksVisibleLastUpdate.Add(chunkCoord);
                    } else {
                        DisableChunk(chunkCoord);
                    }
                }
            }

    }

    private int GetChunkLOD(float distance)
    {
        int chunkLod = lods[^1].lod;
        foreach (var lodInfo in lods)
        {
            if (distance < lodInfo.distanceThreshold)
            {
                chunkLod = lodInfo.lod;
                break;
            }
        }
        return chunkLod;
    }

    private Dictionary<Vector2, int> GetNeighborLODS(Vector2 coord)
    {
        Vector2[] directions = new Vector2[]
        {
            Vector2.left,
            Vector2.right,
            Vector2.up,
            Vector2.down
        };

        Dictionary<Vector2, int> neighborLODs = new Dictionary<Vector2, int>();

        foreach (var direction in directions)
        {
            Vector2 neighborCoord = coord + direction;
            if (terrainChunkDictionary.TryGetValue(neighborCoord, out TerrainChunk neighborChunk))
            {
                neighborLODs[direction] = neighborChunk.lod;
            } else {
                float dist = (neighborCoord - coord).sqrMagnitude;
                neighborLODs[direction] = lods[^1].lod;;
            }
        }

        return neighborLODs;

    }


    // used in editor
    public void GenerateChunks()
    {
        DeleteChunks();
        if (randomSeed) seed = Random.value * 10000000;

        Vector2 viewerPosition = new Vector2(Mathf.FloorToInt(viewer.transform.position.x / chunkWidth), Mathf.FloorToInt(viewer.transform.position.z / chunkWidth));

        // generate chunks that are in render distance
        for (int i = -renderDistance; i < renderDistance; i++)
            {
                for (int j = -renderDistance; j < renderDistance; j++)
                {
                    Vector2 chunkCoord = new Vector2(i, j) + viewerPosition;
                    float distance = (i * i) + (j * j);

                    if (distance < renderDistance * renderDistance)
                    {
                        int chunkLod = GetChunkLOD(distance);
                        EnableChunk(chunkCoord, chunkLod);
                    }
                }
            }

        // // doubled code for debugging
        // for (int i = -renderDistance; i < renderDistance; i++)
        //     {
        //         for (int j = -renderDistance; j < renderDistance; j++)
        //         {
        //             Vector2 chunkCoord = new Vector2(i, j) + viewerPosition;
        //             float distance = (i * i) + (j * j);

        //             if (distance < renderDistance * renderDistance)
        //             {
        //                 int chunkLod = GetChunkLOD(distance);
        //                 EnableChunk(chunkCoord, chunkLod);
        //             }
        //         }
        //     }
    }


    public void CreateChunk(Vector2 coord, int chunkLod)
    {
        TerrainChunk newChunk = new TerrainChunk(coord, transform, chunkWidth, chunkHeight, noiseShader, marchingCubesShader, material, gradient, seed);
        newChunk.EnableChunkLOD(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, chunkLod, GetNeighborLODS(coord));
        terrainChunkDictionary[coord] = newChunk;
    }

    private void DeleteChunks()
    {
        foreach (var chunk in terrainChunkDictionary.Values)
        {
            chunk.DestroyChunk();
        }
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

    private void EnableChunk(Vector2 coord, int chunkLod)
    {
        if (terrainChunkDictionary.ContainsKey(coord))
        {
            terrainChunkDictionary[coord].EnableChunkLOD(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, chunkLod, GetNeighborLODS(coord));
        } else CreateChunk(coord, chunkLod);
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


    [System.Serializable]
    public struct LodInfo
    {
        public float distanceThreshold;
        public int lod;
    }

}
