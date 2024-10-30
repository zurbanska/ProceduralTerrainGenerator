using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{

    private GameObject chunk;

    public Vector2 coord;
    public int width;
    public int height;

    public MeshGenerator meshGenerator;
    public NoiseGenerator noiseGenerator;

    public Material material;
    public Gradient gradient;

    public int lod;
    public Mesh mesh;
    public List<LodMesh> LodMeshes = new List<LodMesh>();

    public float[] densityValues;


    public TerrainChunk(Vector2 coord, Transform parent, int width, int height, ComputeShader noiseShader, ComputeShader meshShader, Material material, Gradient gradient)
    {
        this.coord = coord;
        this.width = width;
        this.height = height;

        this.material = material;
        this.gradient = gradient;

        noiseGenerator = new NoiseGenerator(noiseShader);
        meshGenerator = new MeshGenerator(meshShader);

        lod = -1;

        chunk = new GameObject("TerrainChunk");
        chunk.transform.parent = parent;
        chunk.transform.position = new Vector3(coord.x * width, 0, coord.y * width);

        chunk.AddComponent<MeshFilter>();
        chunk.AddComponent<MeshRenderer>();
    }


    public Mesh GenerateMesh(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod)
    {
        densityValues ??= noiseGenerator.GenerateNoise(width + 1, height + 1, new Vector2(coord.x * width, coord.y * width), octaves, persistence, lacunarity, scale, groundLevel);

        MeshRenderer mr = chunk.GetComponent<MeshRenderer>();
        mr.material = material;

        mesh = meshGenerator.GenerateMesh(width + 1, height + 1, isoLevel, densityValues, meshLod, gradient);
        return mesh;
    }

    public void DisableChunk()
    {
        chunk.SetActive(false);
    }

    public void EnableChunk()
    {
        chunk.SetActive(true);
    }

    public void EnableChunkLOD(float isoLevel, int octaves, float persistence, float lacunarity, float scale, float groundLevel, int meshLod)
    {
        MeshFilter mf = chunk.GetComponent<MeshFilter>();

        foreach (var LodMesh in LodMeshes)
        {
            if (LodMesh.lod == meshLod)
            {
                mf.mesh = LodMesh.mesh;
                lod = LodMesh.lod;
                break;
            }
        }
        if (lod != meshLod)
        {
            Mesh newMesh = GenerateMesh(isoLevel, octaves, persistence, lacunarity, scale, groundLevel, meshLod);
            mf.mesh = newMesh;
            lod = meshLod;
            LodMeshes.Add(new LodMesh(meshLod, newMesh));
        }

        chunk.SetActive(true);
    }

    public bool IsVisible()
    {
        return chunk.activeSelf;
    }

    public void DestroyChunk()
    {
        Object.DestroyImmediate(chunk);
    }


    public struct LodMesh
    {
        public int lod;
        public Mesh mesh;

        public LodMesh(int lod, Mesh mesh)
        {
            this.lod = lod;
            this.mesh = mesh;
        }
    }

}
