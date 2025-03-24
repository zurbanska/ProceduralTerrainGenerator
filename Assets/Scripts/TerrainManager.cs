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

    public Material material;


    public int chunkWidth = 8; // points per x & z axis
    public int chunkHeight = 8; // points per y axis

    public int renderDistance = 1;

    public bool randomSeed = false;

    public bool allowTerraforming = true;

    public OBJExporter objExporter = new();
    public PNGExporter pngExporter = new();

    private WaterGenerator waterGenerator = new();
    private float previousWaterLevel = -1;


    public Dictionary<Vector2, GameObject> terrainChunkDictionary = new Dictionary<Vector2, GameObject>(); // dictionary of all created chunks and their coords


    // used in testing
    public void Initialize(ComputeShader noiseShader, ComputeShader marchingCubesShader, Material material, TerrainData terrainData)
    {
        this.noiseShader = noiseShader;
        this.marchingCubesShader = marchingCubesShader;
        this.material = material;
        this.terrainData = terrainData;
    }


    void Start()
    {
        DeleteChunks();
        if (randomSeed) terrainData.seed = Mathf.FloorToInt(Random.value * 1000000);
        UpdateChunks();
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
                    EnableChunk(chunkCoord, chunkNeighbors, needsNewNoise);
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

        // generate water
        Transform existingWater = transform.Find("Water");
        if (existingWater != null && previousWaterLevel != terrainData.waterLevel)
        {
            DestroyImmediate(existingWater.gameObject);
        }

        if (terrainData.waterLevel > 0 && renderDistance > 0)
        {
            Vector2 startPoint = new Vector2(-(renderDistance - 1) * chunkWidth, -(renderDistance - 1) * chunkWidth);
            waterGenerator.GenerateWater(this.transform, startPoint, chunkWidth * (renderDistance * 2 - 1), terrainData.waterLevel, terrainData.lod);
        }

        previousWaterLevel = terrainData.waterLevel;

    }


    public void GenerateChunks()
    {
        DeleteChunks();
        if (randomSeed) terrainData.seed = Mathf.FloorToInt(Random.value * 1000000);
        UpdateChunks();
    }


    public void CreateChunk(Vector2 coord, Vector4 chunkNeighbors)
    {
        if (terrainChunkDictionary.ContainsKey(coord)) terrainChunkDictionary[coord].GetComponent<ChunkManager>().DestroyChunk();

        GameObject newChunk = new GameObject("Terrain Chunk " + coord);
        newChunk.transform.parent = transform;
        newChunk.transform.position = new Vector3(coord.x * chunkWidth, 0, coord.y * chunkWidth);
        newChunk.layer = LayerMask.NameToLayer("Terrain");

        ChunkManager chunkManager = newChunk.AddComponent<ChunkManager>();
        chunkManager.width = chunkWidth;
        chunkManager.height = chunkHeight;
        chunkManager.coord = coord;
        chunkManager.InitChunk(noiseShader, marchingCubesShader, material, terrainData);
        chunkManager.UpdateChunk(chunkNeighbors, terrainData, true);

        terrainChunkDictionary[coord] = newChunk;
    }

    public void DeleteChunks()
    {
        terrainChunkDictionary.Clear();

        if (this == null || transform == null) return;

        while (transform.childCount > 0) {
            if (transform.GetChild(0) != null)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }

    private void DisableChunk(Vector2 coord)
    {
        if (terrainChunkDictionary.ContainsKey(coord))
        {
            terrainChunkDictionary[coord].GetComponent<ChunkManager>().DisableChunk();
        }
    }

    private void EnableChunk(Vector2 coord, Vector4 chunkNeighbors, bool needsNewNoise)
    {
        if (terrainChunkDictionary.ContainsKey(coord))
        {
            terrainChunkDictionary[coord].GetComponent<ChunkManager>().UpdateChunk(chunkNeighbors, terrainData, needsNewNoise);
        } else CreateChunk(coord, chunkNeighbors);
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

    public void ModifyTerrain(Vector3 hitPoint, float brushSize, float brushStrength, bool add)
    {
        if (!allowTerraforming) return;

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
                    chunkManager.Terraform(localHitPoint, brushSize, brushStrength, add, brushBounds);
                }
            }
        }
    }


    public void ExportTerrainMesh()
    {
        List<GameObject> objectList = new();

        foreach (var chunk in terrainChunkDictionary)
        {
            objectList.Add(chunk.Value);
        }

        if (transform.Find("Water") != null) objectList.Add(transform.Find("Water").gameObject);

        objExporter.ExportCombinedMesh(objectList);
    }

    public void ExportScreenshot()
    {
        pngExporter.ExportPNG();
    }


    public void ValidateSettings()
    {
        if (renderDistance > 10) renderDistance = 10; // max render distance for safety
        if (renderDistance < 0) renderDistance = 0;

        // chunk width and height have to be multiples of 8 and greater or equal 8
        chunkWidth = Mathf.Max(8, chunkWidth);
        if (chunkWidth % 8 != 0) chunkWidth = Mathf.RoundToInt(chunkWidth / 8.0f) * 8;

        chunkHeight = Mathf.Max(chunkHeight, chunkWidth);
        if (chunkHeight % 8 != 0) chunkHeight = Mathf.RoundToInt(chunkHeight / 8.0f) * 8;

        if (terrainData != null)
        {
            if (terrainData.waterLevel < 0) terrainData.waterLevel = 0;

            if (terrainData.smoothLevel < 0) terrainData.smoothLevel = 0;
            if (terrainData.smoothLevel > 1) terrainData.smoothLevel = 1;

            if (terrainData.lod < 1) terrainData.lod = 1;
            if (terrainData.lod > 8) terrainData.lod = 8;

            if (terrainData.isoLevel < 0) terrainData.isoLevel = 0;
            if (terrainData.isoLevel > 1) terrainData.isoLevel = 1;

            if (terrainData.octaves < 1) terrainData.octaves = 1;
            if (terrainData.octaves > 6) terrainData.octaves = 6;
        }

    }


    private void OnValidate()
    {
        ValidateSettings();
    }


}
