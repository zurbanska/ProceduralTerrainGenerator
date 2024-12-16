using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    public ComputeBuffer trianglesBuffer;
    ComputeBuffer triangleCountBuffer;

    ComputeBuffer densityBuffer; // noise values are treated like terrain density
    ComputeBuffer vertexCacheBuffer;
    public ComputeShader marchingCubesShader;

    public MeshGenerator(ComputeShader marchingCubesShader)
    {
        this.marchingCubesShader = marchingCubesShader;
    }

    const int maxTriangles = 100000;

    public void CreateBuffers(int width, int height)
    {
        trianglesBuffer = new ComputeBuffer(maxTriangles, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        trianglesBuffer.SetCounterValue(0);
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        densityBuffer = new ComputeBuffer(width * width * height, sizeof(float));

        vertexCacheBuffer = new ComputeBuffer(maxTriangles, sizeof(float) * 3);
    }

    public void ReleaseBuffers()
    {
        trianglesBuffer.Release();
        triangleCountBuffer.Release();
        densityBuffer.Release();
        vertexCacheBuffer.Release();
    }

    public Mesh GenerateMesh(int width, int height, float isoLevel, float[] densityMap, int lod, Gradient gradient)
    {
        int cubeSize = lod;

        densityBuffer.SetData(densityMap);
        int kernel = marchingCubesShader.FindKernel("March");

        // Set compute shader parameters
        marchingCubesShader.SetBuffer(kernel, "_Triangles", trianglesBuffer);
        marchingCubesShader.SetBuffer(kernel, "_DensityValues", densityBuffer);
        marchingCubesShader.SetInt("_ChunkWidth", width);
        marchingCubesShader.SetInt("_ChunkHeight", height);
        marchingCubesShader.SetFloat("_IsoLevel", isoLevel);
        marchingCubesShader.SetInt("_CubeSize", cubeSize);

        float numThreadsXZ = width / cubeSize;
        float numThreadsY = height / cubeSize;

        // Dispatch shader
        marchingCubesShader.Dispatch(kernel, Mathf.CeilToInt(numThreadsXZ / 8.0f), Mathf.CeilToInt(numThreadsY / 8.0f), Mathf.CeilToInt(numThreadsXZ / 8.0f));

        // Retrieve triangle data
        ComputeBuffer.CopyCount(trianglesBuffer, triangleCountBuffer, 0);
        int[] triangleCountArray = { 0 };
        triangleCountBuffer.GetData(triangleCountArray);
        int numTriangles = triangleCountArray[0];

        Triangle[] triangles = new Triangle[numTriangles];
        trianglesBuffer.GetData(triangles, 0, 0, numTriangles);

        Mesh mesh = CreateMesh(triangles, width, height, gradient);

        ReleaseBuffers();

        return mesh;
    }

    // generates a mesh from an array of triangles
    private Mesh CreateMesh(Triangle[] triangles, int width, int height, Gradient gradient)
    {
        Mesh mesh = new Mesh();

        Vector3[] meshVertices = new Vector3[triangles.Length * 3];
        int[] meshTriangles = new int[triangles.Length * 3];
        Vector2[] meshUVs = new Vector2[meshVertices.Length];
        Color[] meshColors = new Color[meshVertices.Length];

        // Dictionary for mapping vertices to indices
        Dictionary<Vector3, int> vertexIndexMap = new Dictionary<Vector3, int>();

        int realVertexCount = 0;

        for (int i = 0; i < triangles.Length; i++)
        {
            int startIndex = i * 3;

            // Extract triangle vertices
            Vector3[] triangleVerts = { triangles[i].c, triangles[i].b, triangles[i].a };

             for (int j = 0; j < 3; j++) // Loop over a, b, c of the triangle
            {
                Vector3 vertex = triangleVerts[j];
                // if (!vertexIndexMap.TryGetValue(vertex, out int existingIndex)) // try to reuse existing vertex
                // {
                //     existingIndex = realVertexCount;
                //     vertexIndexMap[vertex] = realVertexCount;

                //     meshVertices[realVertexCount] = vertex;

                //     meshUVs[realVertexCount] = new Vector2(vertex.x / width, vertex.z / width);
                //     meshColors[realVertexCount] = gradient.Evaluate(Mathf.InverseLerp(0, height, vertex.y));
                //     realVertexCount++;

                // }
                // meshTriangles[startIndex + j] = existingIndex;

                meshVertices[realVertexCount] = vertex;
                meshUVs[realVertexCount] = new Vector2(vertex.x / width, vertex.z / width);
                meshColors[realVertexCount] = gradient.Evaluate(Mathf.InverseLerp(0, height, vertex.y));
                meshTriangles[startIndex + j] = realVertexCount;
                realVertexCount++;
            }
        }

        // Array.Resize(ref meshVertices, realVertexCount);
        // Array.Resize(ref meshUVs, realVertexCount);
        // Array.Resize(ref meshColors, realVertexCount);

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.uv = meshUVs;
        mesh.colors = meshColors;
        mesh.RecalculateNormals();
        return mesh;
    }


    public float[] UpdateDensity(int width, int height, float[] densityMap, Vector3 hitPosition, float brushSize, bool add, Vector4 neighbors)
    {
        densityBuffer.SetData(densityMap);
        int kernel = marchingCubesShader.FindKernel("UpdateDensity");

        marchingCubesShader.SetBuffer(kernel, "_DensityValues", densityBuffer);
        marchingCubesShader.SetInt("_ChunkWidth", width);
        marchingCubesShader.SetInt("_ChunkHeight", height);
        marchingCubesShader.SetInt("_CubeSize", 1);
        marchingCubesShader.SetVector("_HitPosition", hitPosition);
        marchingCubesShader.SetFloat("_BrushSize", brushSize);
        marchingCubesShader.SetFloat("_TerraformStrength", add ? 1f : -1f);

        marchingCubesShader.SetBool("borderDown", neighbors[0] == 0);
        marchingCubesShader.SetBool("borderRight", neighbors[1] == 0);
        marchingCubesShader.SetBool("borderUp", neighbors[2] == 0);
        marchingCubesShader.SetBool("borderLeft", neighbors[3] == 0);

        float numThreadsXZ = width;
        float numThreadsY = height;

        // Dispatch shader
        marchingCubesShader.Dispatch(kernel, Mathf.CeilToInt(numThreadsXZ / 8.0f), Mathf.CeilToInt(numThreadsY / 8.0f), Mathf.CeilToInt(numThreadsXZ / 8.0f));

        float[] newDensityMap = new float[densityMap.Length];

        densityBuffer.GetData(newDensityMap);

        ReleaseBuffers();
        return newDensityMap;
    }


    struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
    }
}