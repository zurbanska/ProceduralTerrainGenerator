using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    // shaders used for mesh generating
    public ComputeShader noiseShader;
    public ComputeShader marchingCubesShader;

    public TerrainData terrainData;
    private TerrainData lastTerrainData;

    public Material material;
    public Gradient gradient;


    public int chunkWidth = 8; // points per x & z axis
    public int chunkHeight = 8; // points per y axis

    public int renderDistance;

    public bool randomSeed = false;

    public bool allowTerraforming = true;

    public MeshExporter meshExporter;


    private Dictionary<Vector2, GameObject> terrainChunkDictionary = new Dictionary<Vector2, GameObject>(); // dictionary of all created chunks and their coords


    // Start is called before the first frame update
    void Start()
    {
        lastTerrainData = terrainData;
        meshExporter = new();
        DeleteChunks();
        if (randomSeed) terrainData.seed = Mathf.FloorToInt(Random.value * 1000000);
        UpdateChunks();

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void UpdateChunks(bool needsNewNoise = true)
    {
        // enable chunks within render distance
        for (int i = -renderDistance + 1; i < renderDistance; i++)
            {
                for (int j = -renderDistance + 1; j < renderDistance; j++)
                {
                    Vector2 chunkCoord = new Vector2(i, j);
                    Vector4 chunkNeighbors = CheckForChunkNeighbors(i, j);
                    EnableChunk(chunkCoord, terrainData.lod, chunkNeighbors, needsNewNoise);
                }
            }

        // disable existing chunks outside of render distance
        foreach (var chunk in terrainChunkDictionary)
        {
            Vector2 coord = chunk.Key;
            if (Mathf.Abs(coord.x) >= renderDistance || Mathf.Abs(coord.y) >= renderDistance)
            {
                DisableChunk(coord);
            }
        }

    }


    // used in editor
    public void GenerateChunks()
    {
        DeleteChunks();
        if (randomSeed) terrainData.seed = Mathf.FloorToInt(Random.value * 1000000);
        UpdateChunks();
    }


    public void CreateChunk(Vector2 coord, int chunkLod, Vector4 chunkNeighbors)
    {
        GameObject newChunk = new GameObject("Terrain Chunk " + coord);
        newChunk.transform.parent = transform;
        newChunk.transform.position = new Vector3(coord.x * chunkWidth, 0, coord.y * chunkWidth);
        newChunk.layer = LayerMask.NameToLayer("Terrain");

        ChunkManager chunkManager = newChunk.AddComponent<ChunkManager>();
        chunkManager.width = chunkWidth;
        chunkManager.height = chunkHeight;
        chunkManager.coord = coord;
        chunkManager.InitChunk(noiseShader, marchingCubesShader, material, gradient, terrainData, chunkNeighbors);
        // chunkManager.UpdateChunk(chunkNeighbors, terrainData);

        terrainChunkDictionary[coord] = newChunk;
    }

    private void DeleteChunks()
    {
        foreach (var chunk in terrainChunkDictionary.Values)
        {
            chunk.GetComponent<ChunkManager>().DestroyChunk();
        }
        terrainChunkDictionary.Clear();

        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

    }

    private void DisableChunk(Vector2 coord)
    {
        if (terrainChunkDictionary.ContainsKey(coord))
        {
            terrainChunkDictionary[coord].GetComponent<ChunkManager>().DisableChunk();
        }
    }

    private void EnableChunk(Vector2 coord, int chunkLod, Vector4 chunkNeighbors, bool needsNewNoise)
    {
        if (terrainChunkDictionary.ContainsKey(coord))
        {
            terrainChunkDictionary[coord].GetComponent<ChunkManager>().UpdateChunk(chunkNeighbors, terrainData, needsNewNoise);
        } else CreateChunk(coord, chunkLod, chunkNeighbors);
    }

    private Vector4 CheckForChunkNeighbors(int x, int z)
    {
        Vector4 chunkNeighbors = new Vector4(
            (Mathf.Abs(z) + 1 > renderDistance - 1 && z <= 0) ? 0 : 1,
            (Mathf.Abs(x) + 1 > renderDistance - 1 && x >= 0) ? 0 : 1,
            (Mathf.Abs(z) + 1 > renderDistance - 1 && z >= 0) ? 0 : 1,
            (Mathf.Abs(x) + 1 > renderDistance - 1 && x <= 0) ? 0 : 1
        );

        return chunkNeighbors;
    }

    public void ModifyTerrain(Vector3 hitPoint, float brushSize, bool add)
    {
        Bounds brushBounds = new Bounds(hitPoint, Vector3.one * brushSize);

        foreach (var item in terrainChunkDictionary)
        {
            GameObject chunk = item.Value;
            ChunkManager chunkManager = chunk.GetComponent<ChunkManager>();
            if (chunkManager != null)
            {
                if (chunkManager.bounds.Intersects(brushBounds))
                {
                    Vector3 localHitPoint = hitPoint - chunk.transform.position;
                    chunkManager.Terraform(localHitPoint, brushSize, add, brushBounds);
                }
            }
        }
    }


    public void ExportTerrainMesh()
    {
        meshExporter.ExportCombinedMesh(terrainChunkDictionary);
    }


    private void OnValidate() {

        if (renderDistance > 10) {
            renderDistance = 10; // max render distance for safety
        }
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

    }


}
