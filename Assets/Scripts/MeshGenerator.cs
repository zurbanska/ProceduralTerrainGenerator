using UnityEngine;

public class MeshGenerator
{
    ComputeBuffer trianglesBuffer;
    ComputeBuffer triangleCountBuffer;
    ComputeBuffer densityBuffer; // noise values are treated like terrain density
    public ComputeShader marchingCubesShader;

    public MeshGenerator(ComputeShader marchingCubesShader)
    {
        this.marchingCubesShader = marchingCubesShader;
    }

    const int maxTriangles = 100000;

    void CreateBuffers(int width, int height)
    {
        trianglesBuffer = new ComputeBuffer(maxTriangles, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        trianglesBuffer.SetCounterValue(0);
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        densityBuffer = new ComputeBuffer(width * width * height, sizeof(float));
    }

    void ReleaseBuffers()
    {
        trianglesBuffer.Release();
        triangleCountBuffer.Release();
        densityBuffer.Release();
    }

    public Mesh GenerateMesh(int width, int height, float isoLevel, float[] densityMap, int lod)
    {
        int cubeSize = lod;

        CreateBuffers(width, height);

        densityBuffer.SetData(densityMap);

        // Set compute shader parameters
        marchingCubesShader.SetBuffer(0, "_Triangles", trianglesBuffer);
        marchingCubesShader.SetBuffer(0, "_DensityValues", densityBuffer);
        marchingCubesShader.SetInt("_ChunkWidth", width);
        marchingCubesShader.SetInt("_ChunkHeight", height);
        marchingCubesShader.SetFloat("_IsoLevel", isoLevel);
        marchingCubesShader.SetInt("_CubeSize", cubeSize);

        float numThreadsXZ = width / cubeSize;
        float numThreadsY = height / cubeSize;

        // Dispatch shader
        marchingCubesShader.Dispatch(0, Mathf.CeilToInt(numThreadsXZ / 8.0f), Mathf.CeilToInt(numThreadsY / 8.0f), Mathf.CeilToInt(numThreadsXZ / 8.0f));

        // Retrieve triangle data
        ComputeBuffer.CopyCount(trianglesBuffer, triangleCountBuffer, 0);
        int[] triangleCountArray = { 0 };
        triangleCountBuffer.GetData(triangleCountArray);
        int numTriangles = triangleCountArray[0];


        Triangle[] triangles = new Triangle[numTriangles];
        trianglesBuffer.GetData(triangles, 0, 0, numTriangles);

        // Create mesh
        Mesh mesh = new Mesh();

        Vector3[] meshVertices = new Vector3[triangles.Length * 3];
        int[] meshTriangles = new int[triangles.Length * 3];

        for (int i = 0; i < triangles.Length; i++)
        {
            int startIndex = i * 3;

            meshVertices[startIndex] = triangles[i].c;
            meshVertices[startIndex + 1] = triangles[i].b;
            meshVertices[startIndex + 2] = triangles[i].a;

            meshTriangles[startIndex] = startIndex;
            meshTriangles[startIndex + 1] = startIndex + 1;
            meshTriangles[startIndex + 2] = startIndex + 2;
        }

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();

        ReleaseBuffers();

        return mesh;
    }

    struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
    }
}