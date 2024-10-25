using System.Collections.Generic;
using System.Data.Common;
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

    // Start is called before the first frame update
    void Start()
    {
        GenerateChunks();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateChunks()
    {
        DeleteChunks();

        Vector2 viewerPosition = new Vector2(transform.position.x / chunkWidth, transform.position.z / chunkWidth);

        // generate chunks that are in render distance
        for (int i = -renderDistance; i < renderDistance; i++)
            {
                for (int j = -renderDistance; j < renderDistance; j++)
                {
                    if ((i * i) + (j * j) < (renderDistance * renderDistance))
                    {
                        CreateChunk(new Vector2(i, j) + viewerPosition);
                    }
                }
            }
    }

    public void CreateChunk(Vector2 coord)
    {
        GameObject newChunk = new GameObject("Terrain Chunk");
        newChunk.transform.parent = transform;

        // real voxel with of chunk is 1 lesser than chunkWidth (chunkWidth is the num of vertices created in X and Z axes)
        newChunk.transform.position = new Vector3(coord.x * (chunkWidth - 1), 0, coord.y * (chunkWidth - 1));

        ChunkManager chunkManager = newChunk.AddComponent<ChunkManager>();

        chunkManager.noiseShader = noiseShader;
        chunkManager.marchingCubesShader = marchingCubesShader;

        chunkManager.GenerateChunk(coord, chunkWidth, chunkHeight, isoLevel, octaves, persistence, lacunarity, scale, groundLevel, lod);
    }

    private void DeleteChunks()
    {
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
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


}
