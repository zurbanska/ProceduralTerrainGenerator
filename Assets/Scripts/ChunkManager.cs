using Unity.VisualScripting;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{

    public NoiseGenerator noiseGenerator;
    public MeshGenerator meshGenerator;
    private float[] densityMap;


    private void GenerateDensityMap(int width, int height, Vector2 offset, int octaves, float persistence, float lacunarity, float scale, float groundLevel)
    {
        densityMap = noiseGenerator.GenerateNoise(width, height, offset, octaves, persistence, lacunarity, scale, groundLevel, 0);
    }


    public void GenerateChunk(Vector2 coord, int width, int height, float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int lod, Material material, Gradient gradient)
    {
        // adding 1 to the width and height to account for the extra vertices at the edges of chunk
        width++;
        height++;

        // chunk coord multiplier has to be the actual (-1) width of chunk in voxel cubes
        float[] densityMap = noiseGenerator.GenerateNoise(width, height, new Vector2(coord.x * (width - 1), coord.y * (width - 1)), octaves, persistence, lacunarity, scale, groundLevel, 0);
        Mesh mesh = meshGenerator.GenerateMesh(width, height, isoLevel, densityMap, lod, gradient);

        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

        mf.mesh = mesh;
        mr.material = material;
    }

}
