using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshGenerator
{
    public ComputeBuffer trianglesBuffer;
    public ComputeBuffer triangleCountBuffer;

    public ComputeBuffer densityBuffer; // noise values are treated like terrain density
    public ComputeBuffer vertexCacheBuffer;
    public ComputeShader marchingCubesShader;

    public MeshGenerator(ComputeShader marchingCubesShader)
    {
        this.marchingCubesShader = marchingCubesShader;
    }

    const int maxTriangles = 100000;

    public void CreateBuffers(int width, int height)
    {
        ReleaseBuffers();
        if (width < 0 || height < 0) return;

        trianglesBuffer = new ComputeBuffer(maxTriangles, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        trianglesBuffer.SetCounterValue(0);
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        densityBuffer = new ComputeBuffer(width * width * height, sizeof(float));

        vertexCacheBuffer = new ComputeBuffer(maxTriangles, sizeof(float) * 3);
    }

    public void ReleaseBuffers()
    {
        trianglesBuffer?.Release();
        triangleCountBuffer?.Release();
        densityBuffer?.Release();
        vertexCacheBuffer?.Release();
    }

    public async Task<Mesh> GenerateMesh(int width, int height, float isoLevel, float[] densityMap, int lod)
    {
        int cubeSize = lod;

        if (width <= 8 || height <= 8 || cubeSize > 8)
        {
            ReleaseBuffers();
            return new Mesh();
        }

        // create buffers if none created
        if (trianglesBuffer == null || triangleCountBuffer == null || densityBuffer == null || vertexCacheBuffer == null) CreateBuffers(width, height);


        densityBuffer.SetData(densityMap);
        int kernel = marchingCubesShader.FindKernel("March");

        // set compute shader parameters
        marchingCubesShader.SetBuffer(kernel, "_Triangles", trianglesBuffer);
        marchingCubesShader.SetBuffer(kernel, "_DensityValues", densityBuffer);
        marchingCubesShader.SetInt("_ChunkWidth", width);
        marchingCubesShader.SetInt("_ChunkHeight", height);
        marchingCubesShader.SetFloat("_IsoLevel", isoLevel);
        marchingCubesShader.SetInt("_CubeSize", cubeSize);

        int numThreadsXZ = Mathf.CeilToInt(width / cubeSize / 8.0f);
        int numThreadsY = Mathf.CeilToInt(height / cubeSize / 8.0f);

        if (numThreadsXZ <= 0 || numThreadsY <= 0)
        {
            ReleaseBuffers();
            return new Mesh();
        }

        // dispatch shader
        marchingCubesShader.Dispatch(kernel, numThreadsXZ, numThreadsY, numThreadsXZ);

        // retrieve triangle data
        ComputeBuffer.CopyCount(trianglesBuffer, triangleCountBuffer, 0);
        int[] triangleCountArray = { 0 };
        triangleCountBuffer.GetData(triangleCountArray);
        int numTriangles = triangleCountArray[0];

        Triangle[] triangles = new Triangle[numTriangles];
        trianglesBuffer.GetData(triangles, 0, 0, numTriangles);

        Mesh mesh = await CreateMesh(triangles, width);

        ReleaseBuffers();
        return mesh;
    }


    // generate a mesh from an array of triangles
    private async Task<Mesh> CreateMesh(Triangle[] triangles, int width)
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

        Mesh mesh = new()
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
        if (width < 1 || height < 1)
        {
            ReleaseBuffers();
            return densityMap;
        }

        // create buffers if none created
        if (trianglesBuffer == null || triangleCountBuffer == null || densityBuffer == null || vertexCacheBuffer == null) CreateBuffers(width, height);

        if (densityMap == null || densityMap.Length != width * width * height) densityMap = new float[width * width * height];

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

        int numThreadsXZ = Mathf.CeilToInt(width / 8.0f);
        int numThreadsY = Mathf.CeilToInt(height / 8.0f);

        if (numThreadsXZ <= 0 || numThreadsY <= 0)
        {
            ReleaseBuffers();
            return densityMap;
        }

        // dispatch shader
        marchingCubesShader.Dispatch(kernel, numThreadsXZ, numThreadsY, numThreadsXZ);

        float[] newDensityMap = new float[densityMap.Length];

        densityBuffer.GetData(newDensityMap);

        ReleaseBuffers();
        return newDensityMap;
    }


    void OnDestroy()
    {
        ReleaseBuffers();
    }


    struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
    }


}