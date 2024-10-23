using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public ComputeShader marchingCubesShader;
    public ComputeShader noiseShader;

    private NoiseGenerator noiseGenerator;
    private MeshGenerator meshGenerator;


    public void GenerateChunk(int width, int height, float isoLevel)
    {
        noiseGenerator = new NoiseGenerator(noiseShader);
        meshGenerator = new MeshGenerator(marchingCubesShader);


        float[] densityMap = noiseGenerator.GenerateNoise(width, height);

        Mesh mesh = meshGenerator.GenerateMesh(width, height, isoLevel, densityMap);

        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

        mf.mesh = mesh;
        mr.material = new Material(Shader.Find("Standard"));
    }


}
