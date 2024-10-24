using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public ComputeShader marchingCubesShader;
    public ComputeShader noiseShader;

    private NoiseGenerator noiseGenerator;
    private MeshGenerator meshGenerator;


    public void GenerateChunk(Vector2 coord, int width, int height, float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel)
    {
        noiseGenerator = new NoiseGenerator(noiseShader);
        meshGenerator = new MeshGenerator(marchingCubesShader);

        // width & height must be 1 greater than the chunk coord multiplier for the chunk offset
        // to account for the extra vertex needed for last voxel of row in each axis
        float[] densityMap = noiseGenerator.GenerateNoise(width, height, new Vector2(coord.x * (width - 1), coord.y * (width - 1)), octaves, persistence, lacunarity, scale, groundLevel);
        Mesh mesh = meshGenerator.GenerateMesh(width, height, isoLevel, densityMap);

        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

        mf.mesh = mesh;
        mr.material = new Material(Shader.Find("Standard"));
    }


}
