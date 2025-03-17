using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class WaterGenerator
{

    private float shrinkFactor;
    private List<Mesh> meshes = new();

    public void GenerateWater(Transform parent, Vector2 startPoint, float width, float waterLevel, int lod)
    {
        meshes.Clear();
        GameObject existingWater = parent.Find("Water")?.gameObject;
        if (existingWater != null)
        {
            Transform.DestroyImmediate(existingWater);
        }

        if (waterLevel <= 0) return;

        Material waterMaterial = Resources.Load<Material>("WaterMaterial");
        waterMaterial.SetFloat("_WaterLevel", waterLevel);

        shrinkFactor = lod * 1.5f;

        // top plane corner vertices
        Vector3 t1 = new Vector3(shrinkFactor, waterLevel, shrinkFactor); // bottom left
        Vector3 t2 = new Vector3(width -  shrinkFactor, waterLevel, shrinkFactor); // bottom right
        Vector3 t3 = new Vector3(shrinkFactor, waterLevel, width - shrinkFactor); // top left
        Vector3 t4 = new Vector3(width - shrinkFactor, waterLevel, width - shrinkFactor); // top right


        // bottom plane corner vertices
        Vector3 b1 = new Vector3(shrinkFactor, shrinkFactor, shrinkFactor); // bottom left
        Vector3 b2 = new Vector3(width - shrinkFactor, shrinkFactor, shrinkFactor); // bottom right
        Vector3 b3 = new Vector3(shrinkFactor, shrinkFactor, width - shrinkFactor); // top left
        Vector3 b4 = new Vector3(width - shrinkFactor, shrinkFactor, width - shrinkFactor); // top right


        // resolution (subdivision) of planes
        Vector2Int flatRes = new Vector2Int(Mathf.CeilToInt(((width / 2) + 1) / (2 * lod)), Mathf.CeilToInt(((width / 2) + 1) / (2 * lod)));
        Vector2Int sideRes = new Vector2Int(Mathf.CeilToInt(((width / 2) + 1) / (2 * lod)), Mathf.CeilToInt(waterLevel / (2 * lod)));


        // generating water meshes for each side

        // top plane
        Mesh topMesh = GenerateFlatMesh(flatRes, Vector3.up, t1, t2, t3, t4);
        meshes.Add(topMesh);

        // bottom plane
        Mesh bottomMesh = GenerateFlatMesh(flatRes, Vector3.down, b1, b2, b3, b4);
        meshes.Add(bottomMesh);

        // south plane + southeast corner
        Mesh sideMesh1 = GenerateSideMesh(sideRes, Vector3.back, new Vector2(0, -shrinkFactor * 0.5f), b1, b2, t1, t2);
        meshes.Add(sideMesh1);

        Mesh cornerMesh1 = GenerateCornerMesh(b2, t2, sideRes.y, Vector3.back + Vector3.right, new Vector2(shrinkFactor * 0.5f, -shrinkFactor * 0.5f));
        meshes.Add(cornerMesh1);

        // east plane + northeast corner
        Mesh sideMesh2 = GenerateSideMesh(sideRes, Vector3.right, new Vector2(shrinkFactor * 0.5f, 0), b2, b4, t2, t4);
        meshes.Add(sideMesh2);

        Mesh cornerMesh2 = GenerateCornerMesh(b4, t4, sideRes.y, Vector3.right + Vector3.forward, new Vector2(shrinkFactor * 0.5f, shrinkFactor * 0.5f));
        meshes.Add(cornerMesh2);

        // north plane + northwest corner
        Mesh sideMesh3 = GenerateSideMesh(sideRes, Vector3.forward, new Vector2(0, shrinkFactor * 0.5f), b3, b4, t3, t4);
        meshes.Add(sideMesh3);

        Mesh cornerMesh3 = GenerateCornerMesh(b3, t3, sideRes.y, Vector3.forward + Vector3.left, new Vector2(-shrinkFactor * 0.5f, shrinkFactor * 0.5f));
        meshes.Add(cornerMesh3);

        // west plane + southwest corner
        Mesh sideMesh4 = GenerateSideMesh(sideRes, Vector3.left, new Vector2(-shrinkFactor * 0.5f, 0), b3, b1, t3, t1);
        meshes.Add(sideMesh4);

        Mesh cornerMesh4 = GenerateCornerMesh(b1, t1, sideRes.y, Vector3.left + Vector3.back, new Vector2(-shrinkFactor * 0.5f, -shrinkFactor * 0.5f));
        meshes.Add(cornerMesh4);


        // combine all water planes into single object
        Mesh combinedMesh = CombineMeshes();
        GenerateWaterObject(combinedMesh, parent.transform, startPoint, waterMaterial);

    }


    private Mesh CombineMeshes()
    {
        if (meshes == null || meshes.Count == 0) return null;

        List<CombineInstance> combineInstances = new();

        foreach (Mesh mesh in meshes)
        {
            CombineInstance ci = new CombineInstance
            {
                mesh = mesh,
                transform = Matrix4x4.identity
            };
            combineInstances.Add(ci);
        }

        Mesh combinedMesh = new();
        combinedMesh.indexFormat = IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, false);
        combinedMesh.MarkDynamic();

        return combinedMesh;
    }


    // generating object from mesh
    private void GenerateWaterObject(Mesh mesh, Transform parent, Vector2 startPoint, Material material)
    {
        GameObject water = new GameObject();
        water.transform.position = new Vector3(startPoint.x, parent.position.y, startPoint.y);
        water.transform.parent = parent.transform;
        water.name = "Water";

        MeshFilter meshFilter = water.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = water.AddComponent<MeshRenderer>();

        meshRenderer.material = material;
        water.layer = LayerMask.NameToLayer("Water");
        meshFilter.mesh = mesh;
    }



    // top and bottom meshes
    private Mesh GenerateFlatMesh(Vector2Int resolution, Vector3 baseNormal, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        List<Vector3> vertices = FlatVertices(v1, v2, v3, v4, resolution);
        List<int> triangles = TrianglesFromVertices(vertices, resolution, baseNormal, out List<Vector3> normals);

        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            normals = normals.ToArray()
        };

        return mesh;
    }

    // side (curved) meshes
    private Mesh GenerateSideMesh(Vector2Int resolution, Vector3 baseNormal, Vector2 displacementVector, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        List<Vector3> vertices = SideVertices(v1, v2, v3, v4, resolution, displacementVector);
        List<int> triangles = TrianglesFromVertices(vertices, resolution, baseNormal, out List<Vector3> normals);

        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            normals = normals.ToArray()
        };

        return mesh;
    }

    // corner filling meshes (fill gaps between side meshes)
    private Mesh GenerateCornerMesh(Vector3 v1, Vector3 v2, int resolution, Vector3 baseNormal, Vector2 displacementVector)
    {
        List<Vector3> vertices = CornerVeritices(v1, v2, resolution, displacementVector);
        List<int> triangles = CornerTrianglesFromVertices(vertices, resolution, baseNormal, out List<Vector3> normals);

        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            normals = normals.ToArray()
        };

        return mesh;
    }


    // vertices for flat planes
    private List<Vector3> FlatVertices(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector2Int resolution)
    {
        List<Vector3> vertices = new();

        float stepX = (v2.x - v1.x) / resolution.x;
        float stepZ = (v3.z - v1.z) / resolution.y;
        float yLevel = v1.y;

        for (int x = 0; x <= resolution.x; x++)
        {
            for (int z = 0; z <= resolution.y; z++)
            {
                vertices.Add(new Vector3(v1.x + x * stepX, yLevel, v1.z + z * stepZ));
            }
        }

        return vertices;
    }


    // vertices for side planes
    private List<Vector3> SideVertices(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector2Int resolution, Vector2 expandFactor)
    {
        List<Vector3> vertices = new();

        float topLimit = v3.y - shrinkFactor * 0.5f;
        float bottomLimit = v1.y + shrinkFactor * 0.5f;

        float stepY = (topLimit - bottomLimit) / resolution.y;

        if (v1.x == v2.x) // side facing X axis
        {
            float stepZ = (v2.z - v1.z) / resolution.x;
            float xLevel = v1.x;

            // add vertices like in a flat plane
            for (int z = 0; z <= resolution.x; z++)
            {
                for (int y = 0; y <= resolution.y; y++)
                {
                    vertices.Add(new Vector3(xLevel + expandFactor.x, bottomLimit + y * stepY, v1.z + z * stepZ + expandFactor.y));
                }
            }

            // add additional vertices for bottom curve (angled edge surface)
            for (int z = 0; z <= resolution.x; z++)
            {
                vertices.Add(new Vector3(xLevel, v1.y, v1.z + z * stepZ));
                vertices.Add(new Vector3(xLevel + expandFactor.x, bottomLimit, v1.z + z * stepZ + expandFactor.y));
            }

            // add additional vertices for top curve (angled edge surface)
            for (int z = 0; z <= resolution.x; z++)
            {
                vertices.Add(new Vector3(xLevel, v3.y, v3.z + z * stepZ));
                vertices.Add(new Vector3(xLevel + expandFactor.x, topLimit, v3.z + z * stepZ + expandFactor.y));
            }

        }
        else // side facing Z axis
        {
            float stepX = (v2.x - v1.x) / resolution.x;
            float zLevel = v1.z;

            // add vertices like in a flat plane
            for (int x = 0; x <= resolution.x; x++)
            {
                for (int y = 0; y <= resolution.y; y++)
                {
                    vertices.Add(new Vector3(v1.x + x * stepX + expandFactor.x, bottomLimit + y * stepY, zLevel + expandFactor.y));
                }
            }

            // add additional vertices for bottom curve (angled edge surface)
            for (int x = 0; x <= resolution.x; x++)
            {
                vertices.Add(new Vector3(v1.x + x * stepX, v1.y, zLevel));
                vertices.Add(new Vector3(v1.x + x * stepX + expandFactor.x, bottomLimit, zLevel + expandFactor.y));
            }

            // add additional vertices for top curve (angled edge surface)
            for (int x = 0; x <= resolution.x; x++)
            {
                vertices.Add(new Vector3(v3.x + x * stepX, v3.y, zLevel));
                vertices.Add(new Vector3(v3.x + x * stepX + expandFactor.x, topLimit, zLevel + expandFactor.y));
            }
        }

        return vertices;
    }


    // vertices for corner fillings
    private List<Vector3> CornerVeritices(Vector3 v1, Vector3 v2, int resolution, Vector2 expandFactor)
    {
        List<Vector3> vertices = new();
        float topLimit = v2.y - shrinkFactor * 0.5f;
        float bottomLimit = v1.y + shrinkFactor * 0.5f;

        float stepY = (topLimit - bottomLimit) / resolution;

        for (int y = 0; y <= resolution; y++)
        {
            if (y == 0)
            {
                // vertices for bottom single triangle
                vertices.Add(v1);
                vertices.Add(new Vector3(v1.x + expandFactor.x, bottomLimit + y * stepY, v1.z));
                vertices.Add(new Vector3(v1.x, bottomLimit + y * stepY, v1.z + expandFactor.y));
            }

            // vertices for the corner side plane
            vertices.Add(new Vector3(v1.x + expandFactor.x, bottomLimit + y * stepY, v1.z));
            vertices.Add(new Vector3(v1.x, bottomLimit + y * stepY, v1.z + expandFactor.y));

            if (y == resolution)
            {
                // vertices for top single triangle
                vertices.Add(new Vector3(v1.x + expandFactor.x, bottomLimit + y * stepY, v1.z));
                vertices.Add(new Vector3(v1.x, bottomLimit + y * stepY, v1.z + expandFactor.y));
                vertices.Add(v2);
            }
        }

        return vertices;
    }


    // get triangles and normals from vertices for flat planes and side planes
    private List<int> TrianglesFromVertices(List<Vector3> vertices, Vector2Int resolution, Vector3 baseNormal, out List<Vector3> normals)
    {
        List<int> triangles = new();
        normals = new List<Vector3>(new Vector3[vertices.Count]);

        for (int column = 0; column < resolution.x; column++)
        {
            for (int row = 0; row < resolution.y; row++)
            {
                int i = (column * resolution.y) + column + row;

                triangles.Add(i);
                triangles.Add(i + resolution.y + 1);
                triangles.Add(i + resolution.y + 2);

                triangles.Add(i);
                triangles.Add(i + resolution.y + 2);
                triangles.Add(i + 1);

                Vector3 normal = baseNormal;
                normals[i] += normal;
                normals[i + 1] += normal;
                normals[i + resolution.y + 1] += normal;
                normals[i + resolution.y + 2] += normal;


                triangles.Add(i + resolution.y + 2);
                triangles.Add(i + resolution.y + 1);
                triangles.Add(i);

                triangles.Add(i + 1);
                triangles.Add(i + resolution.y + 2);
                triangles.Add(i);
            }
        }

        // add triangles for curved top and bottom planes of side
        if (baseNormal.y == 0)
        {
            // bottom curve
            for (int column = 0; column < resolution.x; column ++)
            {
                int i = (resolution.x + 1) * (resolution.y + 1) + column * 2;

                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 2);

                triangles.Add(i + 3);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                Vector3 normal = baseNormal + Vector3.down;
                normals[i] = normal;
                normals[i + 1] = normal;
                normals[i + 2] = normal;
                normals[i + 3] = normal;

                triangles.Add(i + 2);
                triangles.Add(i + 1);
                triangles.Add(i);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }

            // top curve
            for (int column = 0; column < resolution.x; column ++)
            {
                int i = (resolution.x + 1) * (resolution.y + 1) + (column + resolution.x) * 2 + 2;

                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 2);

                triangles.Add(i + 3);
                triangles.Add(i + 2);
                triangles.Add(i + 1);

                Vector3 normal = baseNormal + Vector3.up;
                normals[i] = normal;
                normals[i + 1] = normal;
                normals[i + 2] = normal;
                normals[i + 3] = normal;

                triangles.Add(i + 2);
                triangles.Add(i + 1);
                triangles.Add(i);

                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }
        }

        for (int i = 0; i < normals.Count; i++)
        {
            normals[i] = normals[i].normalized;
        }

        return triangles;
    }


    // get triangles and normals for corner fillings
    private List<int> CornerTrianglesFromVertices(List<Vector3> vertices, int resolution, Vector3 baseNormal, out List<Vector3> normals)
    {
        List<int> triangles = new();
        normals = new List<Vector3>(new Vector3[vertices.Count]);

        for (int row = 0; row <= resolution; row++)
        {
            int i = 2 * row + 3;

            // bottom single triangle
            if (row == 0)
            {
                int j = 0;
                triangles.Add(j);
                triangles.Add(j + 1);
                triangles.Add(j + 2);

                Vector3 newNormal = baseNormal + Vector3.down;
                normals[j] = newNormal;
                normals[j + 1] = newNormal;
                normals[j + 2] = newNormal;

                triangles.Add(j + 2);
                triangles.Add(j + 1);
                triangles.Add(j);
            }

            // side seam plane triangles
            triangles.Add(i);
            triangles.Add(i + 2);
            triangles.Add(i + 1);

            triangles.Add(i + 1);
            triangles.Add(i + 2);
            triangles.Add(i + 3);

            Vector3 normal = baseNormal;
            normals[i] = normal;
            normals[i + 1] = normal;
            normals[i + 2] = normal;
            normals[i + 3] = normal;

            triangles.Add(i + 1);
            triangles.Add(i + 2);
            triangles.Add(i);

            triangles.Add(i + 3);
            triangles.Add(i + 2);
            triangles.Add(i + 1);

            // top single triangle
            if (row == resolution)
            {
                int j = i + 2;

                triangles.Add(j);
                triangles.Add(j + 1);
                triangles.Add(j + 2);

                Vector3 newNormal = baseNormal + Vector3.up;
                normals[j] = newNormal;
                normals[j + 1] = newNormal;
                normals[j + 2] = newNormal;

                triangles.Add(j + 2);
                triangles.Add(j + 1);
                triangles.Add(j);
            }
        }

        for (int i = 0; i < normals.Count; i++)
        {
            normals[i] = normals[i].normalized;
        }

        return triangles;
    }


}

