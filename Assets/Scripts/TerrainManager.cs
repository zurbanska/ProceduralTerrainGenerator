using UnityEngine;

public class TerrainManager : MonoBehaviour
{

    public ComputeShader noiseShader;
    public ComputeShader marchingCubesShader;


    public int chunkWidth = 8;
    public int chunkHeight = 8;
    public float isoLevel = 0.6f;


    // Start is called before the first frame update
    void Start()
    {
        CreateChunk();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateChunk()
    {
        Vector2 coord = transform.position;
        DeleteChunks();

        GameObject newChunk = new GameObject("Terrain Chunk");
        newChunk.transform.parent = transform;
        newChunk.transform.position = new Vector3(coord.x * chunkWidth, 0, coord.y * chunkWidth);

        ChunkManager chunkManager = newChunk.AddComponent<ChunkManager>();

        chunkManager.noiseShader = noiseShader;
        chunkManager.marchingCubesShader = marchingCubesShader;

        chunkManager.GenerateChunk(chunkWidth, chunkHeight, isoLevel);
    }

    private void DeleteChunks()
    {
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

}
