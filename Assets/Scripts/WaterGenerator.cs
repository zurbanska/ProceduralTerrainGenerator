using UnityEngine;

public class WaterGenerator
{

    public void GenerateWater(Transform parent, float width, float waterLevel, int lod, Vector4 neighbors)
    {
        GameObject water = new GameObject("Water");
        water.transform.parent = parent;
        water.transform.position = parent.position;

        Material waterMaterial = Resources.Load<Material>("WaterMaterial");
        waterMaterial.SetFloat("_WaterLevel", waterLevel);

        float shrinkFactor = lod;

        // top water plane
        Vector3 t1 = new Vector3((1 - neighbors[3]) * shrinkFactor, waterLevel, (1 - neighbors[0]) * shrinkFactor); // bottom left
        Vector3 t2 = new Vector3(width - (1 - neighbors[1]) * shrinkFactor, waterLevel, (1 - neighbors[0]) * shrinkFactor); // bottom right
        Vector3 t3 = new Vector3((1 - neighbors[3]) * shrinkFactor, waterLevel, width - (1 - neighbors[2]) * shrinkFactor); // top left
        Vector3 t4 = new Vector3(width - (1 - neighbors[1]) * shrinkFactor, waterLevel, width - (1 - neighbors[2]) * shrinkFactor); // top right

        Mesh topMesh = GenerateFlatPlaneMesh(t1, t2, t3, t4);
        GenerateWaterPlaneObject(topMesh, water.transform, waterMaterial);

        // bottom water plane
        Vector3 b1 = new Vector3((1 - neighbors[3]) * shrinkFactor, 0, (1 - neighbors[0]) * shrinkFactor); // bottom left
        Vector3 b2 = new Vector3(width - (1 - neighbors[1]) * shrinkFactor, 0, (1 - neighbors[0]) * shrinkFactor); // bottom right
        Vector3 b3 = new Vector3((1 - neighbors[3]) * shrinkFactor, 0, width - (1 - neighbors[2]) * shrinkFactor); // top left
        Vector3 b4 = new Vector3(width - (1 - neighbors[1]) * shrinkFactor, 0, width - (1 - neighbors[2]) * shrinkFactor); // top right

        // Mesh bottomMesh = GenerateFlatPlaneMesh(b1, b2, b3, b4);
        // GenerateWaterPlaneObject(bottomMesh, water.transform, waterMaterial);

        // neighbor chunks present: 0 - bottom, 1 - right, 2 - up, 3 - left
        int neighborCount = GetNeighborCount(neighbors);

        if (neighborCount == 4) {
            return;
        }
        else if (neighborCount == 3) {
            if (neighbors[0] == 0) GenerateWaterPlaneObject(GenerateCurvedPlaneMesh(b1, b2, t1, t2, shrinkFactor * 0.5f, new Vector3(0, 0, -shrinkFactor * 0.5f)), water.transform, waterMaterial);
            else if (neighbors[1] == 0) GenerateWaterPlaneObject(GenerateCurvedPlaneMesh(b2, b4, t2, t4, shrinkFactor * 0.5f, new Vector3(shrinkFactor * 0.5f, 0)), water.transform, waterMaterial);
            else if (neighbors[2] == 0) GenerateWaterPlaneObject(GenerateCurvedPlaneMesh(b3, b4, t3, t4, shrinkFactor * 0.5f, new Vector3(0, 0, shrinkFactor * 0.5f)), water.transform, waterMaterial);
            else if (neighbors[3] == 0) GenerateWaterPlaneObject(GenerateCurvedPlaneMesh(b3, b1, t3, t1, shrinkFactor * 0.5f, new Vector3(-shrinkFactor * 0.5f, 0, 0)), water.transform, waterMaterial);
        }
        else if (neighborCount == 2) {
            if (neighbors[0] == 0 && neighbors[3] == 0) GenerateWaterPlaneObject(GenerateCornerPlaneMesh(b1, b2, t1, t2, b3, b1, t3, t1, shrinkFactor * 0.5f, new Vector3(-shrinkFactor * 0.5f, 0, 0), new Vector3(0, 0, -shrinkFactor * 0.5f), 0), water.transform, waterMaterial);
            else if (neighbors[0] == 0 && neighbors[1] == 0) GenerateWaterPlaneObject(GenerateCornerPlaneMesh(b1, b2, t1, t2, b2, b4, t2, t4, shrinkFactor * 0.5f, new Vector3(shrinkFactor * 0.5f, 0), new Vector3(0, 0, -shrinkFactor * 0.5f), 1), water.transform, waterMaterial);
            else if (neighbors[2] == 0 && neighbors[1] == 0) GenerateWaterPlaneObject(GenerateCornerPlaneMesh(b3, b4, t3, t4, b2, b4, t2, t4, shrinkFactor * 0.5f, new Vector3(shrinkFactor * 0.5f, 0), new Vector3(0, 0, shrinkFactor * 0.5f), 2), water.transform, waterMaterial);
            else if (neighbors[2] == 0 && neighbors[3] == 0) GenerateWaterPlaneObject(GenerateCornerPlaneMesh(b3, b4, t3, t4, b3, b1, t3, t1, shrinkFactor * 0.5f, new Vector3(-shrinkFactor * 0.5f, 0, 0), new Vector3(0, 0, shrinkFactor * 0.5f), 3), water.transform, waterMaterial);

        }
        else if (neighborCount == 0) {
            return;
        }

    }

    private void GenerateWaterPlaneObject(Mesh mesh, Transform parent, Material material)
    {
        GameObject waterPlane = new GameObject();
        waterPlane.transform.position = new Vector3(parent.position.x, parent.position.y, parent.position.z);
        waterPlane.transform.parent = parent.transform;

        MeshFilter meshFilter = waterPlane.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = waterPlane.AddComponent<MeshRenderer>();

        meshRenderer.material = material;
        waterPlane.layer = 4; // water layer
        meshFilter.mesh = mesh;
    }

    private Mesh GenerateFlatPlaneMesh(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        vertices[0] = v1; // bottom left
        vertices[1] = v2; // bottom right
        vertices[2] = v3; // top left
        vertices[3] = v4; // top right

        int[] triangles = new int[]
        {
            0, 2, 1, // first triangle
            1, 2, 0, // first triangle - reverse
            2, 3, 1,  // second triangle
            1, 3, 2  // second triangle - reverse
        };

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();

        return mesh;

    }

    private Mesh GenerateCurvedPlaneMesh(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float shrinkFactor, Vector3 displacement)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[8];
        vertices[0] = v1; // bottom left back
        vertices[1] = v1 + displacement + new Vector3(0, shrinkFactor, 0); // bottom left front
        vertices[2] = v2; // bottom right back
        vertices[3] = v2 + displacement + new Vector3(0, shrinkFactor, 0); // bottom right front
        vertices[4] = v3; // top left back
        vertices[5] = v3 + displacement - new Vector3(0, shrinkFactor, 0); // top left front
        vertices[6] = v4; // top right back
        vertices[7] = v4 + displacement - new Vector3(0, shrinkFactor, 0); // top right front

        int[] triangles = new int[]
        {
            0, 1, 2,
            2, 1, 0,
            1, 3, 2,
            2, 3, 1,

            1, 5, 3,
            3, 5, 1,
            5, 7, 3,
            3, 7, 5,

            5, 4, 7,
            7, 4, 5,
            4, 6, 7,
            7, 6, 4,
        };

        Vector2[] uvs = new Vector2[8];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(0, 0);
        uvs[2] = new Vector2(1, 0);
        uvs[3] = new Vector2(1, 0);
        uvs[4] = new Vector2(0, 1);
        uvs[5] = new Vector2(0, 1);
        uvs[6] = new Vector2(1, 1);
        uvs[7] = new Vector2(1, 1);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        return mesh;
    }

    private Mesh GenerateCornerPlaneMesh(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Vector3 v7, Vector3 v8, float shrinkFactor, Vector3 displacementX, Vector3 displacementZ, int cornerIndex)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[16];

        // chunk edge facing Z vertices
        vertices[0] = v1; // bottom left back
        vertices[1] = v1 + displacementZ + new Vector3(0, shrinkFactor, 0); // bottom left front
        vertices[2] = v2; // bottom right back
        vertices[3] = v2 + displacementZ + new Vector3(0, shrinkFactor, 0); // bottom right front
        vertices[4] = v3; // top left back
        vertices[5] = v3 + displacementZ - new Vector3(0, shrinkFactor, 0); // top left front
        vertices[6] = v4; // top right back
        vertices[7] = v4 + displacementZ - new Vector3(0, shrinkFactor, 0); // top right front

        // chunk edge facing X vertices
        vertices[8] = v5; // bottom left back
        vertices[9] = v5 + displacementX + new Vector3(0, shrinkFactor, 0); // bottom left front
        vertices[10] = v6; // bottom right back
        vertices[11] = v6 + displacementX + new Vector3(0, shrinkFactor, 0); // bottom right front
        vertices[12] = v7; // top left back
        vertices[13] = v7 + displacementX - new Vector3(0, shrinkFactor, 0); // top left front
        vertices[14] = v8; // top right back
        vertices[15] = v8 + displacementX - new Vector3(0, shrinkFactor, 0); // top right front

        // vertices at chunk corner to fill
        int[] cornerVertices = new int[6];
        if (cornerIndex == 0) cornerVertices = new int[6]{15,4,5,1,10,11};
        else if (cornerIndex == 1) cornerVertices = new int[6]{7,6,13,9,8,3};
        else if (cornerIndex == 2) cornerVertices = new int[6]{15,14,7,3,2,11};
        else cornerVertices = new int[6]{5,4,13,9,10,1};

        int[] triangles = new int[]
        {
            // edge facing Z
            0, 1, 2,
            2, 1, 0,
            1, 3, 2,
            2, 3, 1,

            1, 5, 3,
            3, 5, 1,
            5, 7, 3,
            3, 7, 5,

            5, 4, 7,
            7, 4, 5,
            4, 6, 7,
            7, 6, 4,

            // edge facing X
            8, 9, 10,
            10, 9, 8,
            9, 11, 10,
            10, 11, 9,

            9, 13, 11,
            11, 13, 9,
            13, 15, 11,
            11, 15, 13,

            13, 12, 15,
            15, 12, 13,
            12, 14, 15,
            15, 14, 12,

            // corner fillings
            cornerVertices[0], cornerVertices[1], cornerVertices[2],
            cornerVertices[2], cornerVertices[1], cornerVertices[0],
            cornerVertices[3], cornerVertices[4], cornerVertices[5],
            cornerVertices[5], cornerVertices[4], cornerVertices[3],

            cornerVertices[5], cornerVertices[0], cornerVertices[3],
            cornerVertices[3], cornerVertices[0], cornerVertices[5],
            cornerVertices[0], cornerVertices[2], cornerVertices[3],
            cornerVertices[3], cornerVertices[2], cornerVertices[0],
        };

        Vector2[] uvs = new Vector2[16];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(0, 0);
        uvs[2] = new Vector2(1, 0);
        uvs[3] = new Vector2(1, 0);
        uvs[4] = new Vector2(0, 1);
        uvs[5] = new Vector2(0, 1);
        uvs[6] = new Vector2(1, 1);
        uvs[7] = new Vector2(1, 1);
        uvs[8] = new Vector2(0, 0);
        uvs[9] = new Vector2(0, 0);
        uvs[10] = new Vector2(1, 0);
        uvs[11] = new Vector2(1, 0);
        uvs[12] = new Vector2(0, 1);
        uvs[13] = new Vector2(0, 1);
        uvs[14] = new Vector2(1, 1);
        uvs[15] = new Vector2(1, 1);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        return mesh;
    }

    private int GetNeighborCount(Vector4 neighbors)
    {
        int neighborCount = Mathf.RoundToInt(neighbors.x + neighbors.y + neighbors.z + neighbors.w);
        return neighborCount;
    }
}
