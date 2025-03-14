using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

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

    public async Task<Mesh> GenerateMesh(int width, int height, float isoLevel, float[] densityMap, int lod)
    {
        int cubeSize = lod;

        densityBuffer.SetData(densityMap);
        int kernel = marchingCubesShader.FindKernel("March");

        // set compute shader parameters
        marchingCubesShader.SetBuffer(kernel, "_Triangles", trianglesBuffer);
        marchingCubesShader.SetBuffer(kernel, "_DensityValues", densityBuffer);
        marchingCubesShader.SetInt("_ChunkWidth", width);
        marchingCubesShader.SetInt("_ChunkHeight", height);
        marchingCubesShader.SetFloat("_IsoLevel", isoLevel);
        marchingCubesShader.SetInt("_CubeSize", cubeSize);

        float numThreadsXZ = width / cubeSize;
        float numThreadsY = height / cubeSize;

        // dispatch shader
        marchingCubesShader.Dispatch(kernel, Mathf.CeilToInt(numThreadsXZ / 8.0f), Mathf.CeilToInt(numThreadsY / 8.0f), Mathf.CeilToInt(numThreadsXZ / 8.0f));

        // retrieve triangle data
        ComputeBuffer.CopyCount(trianglesBuffer, triangleCountBuffer, 0);
        int[] triangleCountArray = { 0 };
        triangleCountBuffer.GetData(triangleCountArray);
        int numTriangles = triangleCountArray[0];

        Triangle[] triangles = new Triangle[numTriangles];
        trianglesBuffer.GetData(triangles, 0, 0, numTriangles);

        Mesh mesh = await CreateMesh(triangles, width, height);

        ReleaseBuffers();
        return mesh;
    }


    // generate a mesh from an array of triangles
    private async Task<Mesh> CreateMesh(Triangle[] triangles, int width, int height)
    {

        var meshData = await Task.Run(() =>
        {
            Vector3[] meshVertices = new Vector3[triangles.Length * 3];
            int[] meshTriangles = new int[triangles.Length * 3];
            Vector2[] meshUVs = new Vector2[meshVertices.Length];

            int realVertexCount = 0;

            for (int i = 0; i < triangles.Length; i++)
            {
                int startIndex = i * 3;

                Vector3[] triangleVerts = { triangles[i].c, triangles[i].b, triangles[i].a };

                for (int j = 0; j < 3; j++)
                {
                    Vector3 vertex = triangleVerts[j];

                    meshVertices[realVertexCount] = vertex;
                    meshUVs[realVertexCount] = new Vector2(vertex.x / width, vertex.z / width);
                    meshTriangles[startIndex + j] = realVertexCount;
                    realVertexCount++;
                }
            }

            return (meshVertices, meshTriangles, meshUVs);
        });

        Mesh mesh = new Mesh
        {
            vertices = meshData.meshVertices,
            triangles = meshData.meshTriangles,
            uv = meshData.meshUVs,
        };

        mesh.RecalculateNormals();
        mesh.MarkDynamic();
        return mesh;
    }


    public float[] UpdateDensity(int width, int height, float[] densityMap, Vector3 hitPosition, float brushSize, float brushStrength, bool add, Vector4 neighbors, float smoothLevel)
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
        marchingCubesShader.SetFloat("_BrushStrength", brushStrength);
        marchingCubesShader.SetFloat("smoothLevel", smoothLevel);

        marchingCubesShader.SetBool("borderDown", neighbors[0] == 0);
        marchingCubesShader.SetBool("borderRight", neighbors[1] == 0);
        marchingCubesShader.SetBool("borderUp", neighbors[2] == 0);
        marchingCubesShader.SetBool("borderLeft", neighbors[3] == 0);

        float numThreadsXZ = width;
        float numThreadsY = height;

        // dispatch shader
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