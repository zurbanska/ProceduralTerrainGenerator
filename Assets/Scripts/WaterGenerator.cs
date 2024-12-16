using UnityEngine;

public class WaterGenerator
{

    public void GenerateWater(Transform parent, float width, float waterLevel, int lod, Vector4 neighbors)
    {
        GameObject water = new GameObject("Water");
        water.transform.parent = parent;
        water.transform.position = parent.position;
        water.layer = LayerMask.NameToLayer("Water"); ;

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

        Mesh bottomMesh = GenerateFlatPlaneMesh(b1, b2, b3, b4);
        GenerateWaterPlaneObject(bottomMesh, water.transform, waterMaterial);

        // neighbor chunks present: 0 - bottom, 1 - right, 2 - up, 3 - left
        int neighborCount = GetNeighborCount(neighbors);

        if (neighborCount == 4) {
            // neighbor chunks on all sides
            return;
        }
        else if (neighborCount == 3) {
            // one side exposed
            if (neighbors[0] == 0) GenerateWaterPlaneObject(GenerateCurvedPlaneMesh(b1, b2, t1, t2, shrinkFactor * 0.5f, new Vector3(0, 0, -shrinkFactor * 0.5f), 0), water.transform, waterMaterial);
            else if (neighbors[1] == 0) GenerateWaterPlaneObject(GenerateCurvedPlaneMesh(b2, b4, t2, t4, shrinkFactor * 0.5f, new Vector3(shrinkFactor * 0.5f, 0), 1), water.transform, waterMaterial);
            else if (neighbors[2] == 0) GenerateWaterPlaneObject(GenerateCurvedPlaneMesh(b3, b4, t3, t4, shrinkFactor * 0.5f, new Vector3(0, 0, shrinkFactor * 0.5f), 2), water.transform, waterMaterial);
            else if (neighbors[3] == 0) GenerateWaterPlaneObject(GenerateCurvedPlaneMesh(b3, b1, t3, t1, shrinkFactor * 0.5f, new Vector3(-shrinkFactor * 0.5f, 0, 0), 3), water.transform, waterMaterial);
        }
        else if (neighborCount == 2) {
            // 2 sides exposed
            if (neighbors[0] == 0 && neighbors[3] == 0) GenerateWaterPlaneObject(GenerateCornerPlaneMesh(b1, b2, t1, t2, b3, b1, t3, t1, shrinkFactor * 0.5f, new Vector3(-shrinkFactor * 0.5f, 0, 0), new Vector3(0, 0, -shrinkFactor * 0.5f), 0), water.transform, waterMaterial);
            else if (neighbors[0] == 0 && neighbors[1] == 0) GenerateWaterPlaneObject(GenerateCornerPlaneMesh(b1, b2, t1, t2, b2, b4, t2, t4, shrinkFactor * 0.5f, new Vector3(shrinkFactor * 0.5f, 0), new Vector3(0, 0, -shrinkFactor * 0.5f), 1), water.transform, waterMaterial);
            else if (neighbors[2] == 0 && neighbors[1] == 0) GenerateWaterPlaneObject(GenerateCornerPlaneMesh(b3, b4, t3, t4, b2, b4, t2, t4, shrinkFactor * 0.5f, new Vector3(shrinkFactor * 0.5f, 0), new Vector3(0, 0, shrinkFactor * 0.5f), 2), water.transform, waterMaterial);
            else if (neighbors[2] == 0 && neighbors[3] == 0) GenerateWaterPlaneObject(GenerateCornerPlaneMesh(b3, b4, t3, t4, b3, b1, t3, t1, shrinkFactor * 0.5f, new Vector3(-shrinkFactor * 0.5f, 0, 0), new Vector3(0, 0, shrinkFactor * 0.5f), 3), water.transform, waterMaterial);
        }
        else if (neighborCount == 0) {
            // all sides exposed
            GenerateWaterPlaneObject(GenerateCornerPlaneMesh(b1, b2, t1, t2, b3, b1, t3, t1, shrinkFactor * 0.5f, new Vector3(-shrinkFactor * 0.5f, 0, 0), new Vector3(0, 0, -shrinkFactor * 0.5f), 0), water.transform, waterMaterial);
            GenerateWaterPlaneObject(GenerateCornerPlaneMesh(b3, b4, t3, t4, b2, b4, t2, t4, shrinkFactor * 0.5f, new Vector3(shrinkFactor * 0.5f, 0), new Vector3(0, 0, shrinkFactor * 0.5f), 2), water.transform, waterMaterial);
            GenerateWaterPlaneObject(GenerateCornerFillingMesh(b3, t3, shrinkFactor * 0.5f, new Vector3(-shrinkFactor * 0.5f, 0, 0), new Vector3(0, 0, shrinkFactor * 0.5f)), water.transform, waterMaterial);
            GenerateWaterPlaneObject(GenerateCornerFillingMesh(b2, t2, shrinkFactor * 0.5f, new Vector3(shrinkFactor * 0.5f, 0, 0), new Vector3(0, 0, - shrinkFactor * 0.5f)), water.transform, waterMaterial);
        }

    }

    private int GetNeighborCount(Vector4 neighbors)
    {
        int neighborCount = Mathf.RoundToInt(neighbors.x + neighbors.y + neighbors.z + neighbors.w);
        return neighborCount;
    }

    private void GenerateWaterPlaneObject(Mesh mesh, Transform parent, Material material)
    {
        GameObject waterPlane = new GameObject();
        waterPlane.transform.position = new Vector3(parent.position.x, parent.position.y, parent.position.z);
        waterPlane.transform.parent = parent.transform;

        MeshFilter meshFilter = waterPlane.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = waterPlane.AddComponent<MeshRenderer>();

        meshRenderer.material = material;
        waterPlane.layer = LayerMask.NameToLayer("Water"); // water layer
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

        Vector3 normalVector = (v1.y == 0) ? Vector3.down : Vector3.up;

        Vector3[] normals = new Vector3[]
        {
            normalVector,
            normalVector,
            normalVector,
            normalVector
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;

    }

    private Mesh GenerateCurvedPlaneMesh(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float shrinkFactor, Vector3 displacement, int edgeIndex)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[12];
        vertices[0] = v1; // bottom left back
        vertices[1] = v1 + displacement + new Vector3(0, shrinkFactor, 0); // bottom left front
        vertices[2] = v2; // bottom right back
        vertices[3] = v2 + displacement + new Vector3(0, shrinkFactor, 0); // bottom right front
        vertices[4] = v3; // top left back
        vertices[5] = v3 + displacement - new Vector3(0, shrinkFactor, 0); // top left front
        vertices[6] = v4; // top right back
        vertices[7] = v4 + displacement - new Vector3(0, shrinkFactor, 0); // top right front

        // duplicate vertices for flatshading
        vertices[8] = vertices[1]; // bottom left front duplicate
        vertices[9] = vertices[3]; // bottom right front duplicate
        vertices[10] = vertices[5]; // top left front duplicate
        vertices[11] = vertices[7]; // top right front duplicate

        int[] triangles = new int[]
        {
            // bottom curve
            0, 1, 2,
            2, 1, 0,
            1, 3, 2,
            2, 3, 1,

            // main side plane
            8, 10, 9,
            9, 10, 8,
            10, 11, 9,
            9, 11, 10,

            // top curve
            5, 4, 7,
            7, 4, 5,
            4, 6, 7,
            7, 6, 4
        };

        Vector2[] uvs = new Vector2[12];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(0, 0);
        uvs[2] = new Vector2(1, 0);
        uvs[3] = new Vector2(1, 0);
        uvs[4] = new Vector2(0, 1);
        uvs[5] = new Vector2(0, 1);
        uvs[6] = new Vector2(1, 1);
        uvs[7] = new Vector2(1, 1);

        uvs[8] = uvs[1];
        uvs[9] = uvs[3];
        uvs[10] = uvs[5];
        uvs[11] = uvs[7];


        Vector3[] normals = new Vector3[]
        {
            Vector3.down + displacement,
            Vector3.down + displacement,
            Vector3.down + displacement,
            Vector3.down + displacement,

            Vector3.up + displacement,
            Vector3.up + displacement,
            Vector3.up + displacement,
            Vector3.up + displacement,

            displacement,
            displacement,
            displacement,
            displacement
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }

    private Mesh GenerateCornerFillingMesh(Vector3 v1, Vector3 v2, float shrinkFactor, Vector3 displacementX, Vector3 displacementZ)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[10];
        vertices[0] = v1;
        vertices[1] = v1 + displacementZ + new Vector3(0, shrinkFactor, 0);
        vertices[2] = v1 + displacementX + new Vector3(0, shrinkFactor, 0);
        vertices[3] = v2;
        vertices[4] = v2 + displacementZ - new Vector3(0, shrinkFactor, 0);
        vertices[5] = v2 + displacementX - new Vector3(0, shrinkFactor, 0);

        vertices[6] = vertices[1];
        vertices[7] = vertices[2];
        vertices[8] = vertices[4];
        vertices[9] = vertices[5];

        int[] triangles = new int[]
        {
            // bottom triangle
            0, 1, 2,
            2, 1, 0,

            // corner side
            6, 8, 7,
            7, 8, 6,
            8, 9, 7,
            7, 9, 8,

            // top triangle
            3, 4, 5,
            5, 4, 3
        };

        Vector2[] uvs = new Vector2[10];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(1, 0);
        uvs[3] = new Vector2(1, 1);
        uvs[4] = new Vector2(0, 1);
        uvs[5] = new Vector2(1, 0);
        uvs[6] = new Vector2(0, 0);
        uvs[7] = new Vector2(1, 0);
        uvs[8] = new Vector2(0, 1);
        uvs[9] = new Vector2(1, 1);


        Vector3[] normals = new Vector3[]
        {
            Vector3.down + displacementX + displacementZ,
            Vector3.down + displacementX + displacementZ,
            Vector3.down + displacementX + displacementZ,
            Vector3.up + displacementX + displacementZ,
            Vector3.up + displacementX + displacementZ,
            Vector3.up + displacementX + displacementZ,
            displacementX + displacementZ,
            displacementX + displacementZ,
            displacementX + displacementZ,
            displacementX + displacementZ
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;

    }

    private Mesh GenerateCornerPlaneMesh(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Vector3 v7, Vector3 v8, float shrinkFactor, Vector3 displacementX, Vector3 displacementZ, int cornerIndex)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[34];

        // chunk edge facing Z
        vertices[0] = v1; // bottom left back
        vertices[1] = v1 + displacementZ + new Vector3(0, shrinkFactor, 0); // bottom left front
        vertices[2] = v2; // bottom right back
        vertices[3] = v2 + displacementZ + new Vector3(0, shrinkFactor, 0); // bottom right front
        vertices[4] = v3; // top left back
        vertices[5] = v3 + displacementZ - new Vector3(0, shrinkFactor, 0); // top left front
        vertices[6] = v4; // top right back
        vertices[7] = v4 + displacementZ - new Vector3(0, shrinkFactor, 0); // top right front

        // chunk edge facing X
        vertices[8] = v5; // bottom left back
        vertices[9] = v5 + displacementX + new Vector3(0, shrinkFactor, 0); // bottom left front
        vertices[10] = v6; // bottom right back
        vertices[11] = v6 + displacementX + new Vector3(0, shrinkFactor, 0); // bottom right front
        vertices[12] = v7; // top left back
        vertices[13] = v7 + displacementX - new Vector3(0, shrinkFactor, 0); // top left front
        vertices[14] = v8; // top right back
        vertices[15] = v8 + displacementX - new Vector3(0, shrinkFactor, 0); // top right front

        // duplicate vertices for flatshading
        vertices[16] = vertices[1];
        vertices[17] = vertices[3];
        vertices[18] = vertices[5];
        vertices[19] = vertices[7];

        vertices[20] = vertices[9];
        vertices[21] = vertices[11];
        vertices[22] = vertices[13];
        vertices[23] = vertices[15];


        // vertices at chunk corner to fill
        if (cornerIndex == 0)
        {
            vertices[24] = vertices[15];
            vertices[25] = vertices[4];
            vertices[26] = vertices[5];

            vertices[27] = vertices[1];
            vertices[28] = vertices[10];
            vertices[29] = vertices[11];
        }
        else if (cornerIndex == 1)
        {
            vertices[24] = vertices[7];
            vertices[25] = vertices[6];
            vertices[26] = vertices[13];

            vertices[27] = vertices[9];
            vertices[28] = vertices[8];
            vertices[29] = vertices[3];
        }
        else if (cornerIndex == 2)
        {
            vertices[24] = vertices[15];
            vertices[25] = vertices[14];
            vertices[26] = vertices[7];

            vertices[27] = vertices[3];
            vertices[28] = vertices[2];
            vertices[29] = vertices[11];
        }
        else
        {
            vertices[24] = vertices[5];
            vertices[25] = vertices[4];
            vertices[26] = vertices[13];

            vertices[27] = vertices[9];
            vertices[28] = vertices[0];
            vertices[29] = vertices[1];
        }

        vertices[30] = vertices[29];
        vertices[31] = vertices[24];
        vertices[32] = vertices[27];
        vertices[33] = vertices[26];

        int[] triangles = new int[]
        {
            // edge facing Z
            // bottom curve
            0, 1, 2,
            2, 1, 0,
            1, 3, 2,
            2, 3, 1,

            // main side mesh
            16, 18, 17,
            17, 18, 16,
            18, 19, 17,
            17, 19, 18,

            // top curve
            5, 4, 7,
            7, 4, 5,
            4, 6, 7,
            7, 6, 4,

            // edge facing X
            // bottom curve
            8, 9, 10,
            10, 9, 8,
            9, 11, 10,
            10, 11, 9,

            // main side mesh
            20, 22, 21,
            21, 22, 20,
            22, 23, 21,
            21, 23, 22,

            // top curve
            13, 12, 15,
            15, 12, 13,
            12, 14, 15,
            15, 14, 12,

            // corner fillings
            24, 25, 26,
            26, 25, 24,
            27, 28, 29,
            29, 28, 27,

            30, 31, 32,
            32, 31, 30,
            31, 33, 32,
            32, 33, 31

        };

        Vector2[] uvs = new Vector2[34];
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

        uvs[16] = uvs[1];
        uvs[17] = uvs[3];
        uvs[18] = uvs[5];
        uvs[19] = uvs[7];
        uvs[20] = uvs[9];
        uvs[21] = uvs[11];
        uvs[22] = uvs[13];
        uvs[23] = uvs[15];

        uvs[24] = uvs[0];
        uvs[25] = uvs[0];
        uvs[26] = uvs[0];
        uvs[27] = uvs[0];
        uvs[28] = uvs[0];
        uvs[29] = uvs[0];
        uvs[30] = uvs[0];
        uvs[31] = uvs[0];
        uvs[32] = uvs[0];
        uvs[33] = uvs[0];

        Vector3[] normals = new Vector3[]
        {
            Vector3.down + displacementZ,
            Vector3.down + displacementZ,
            Vector3.down + displacementZ,
            Vector3.down + displacementZ,

            Vector3.up + displacementZ,
            Vector3.up + displacementZ,
            Vector3.up + displacementZ,
            Vector3.up + displacementZ,

            Vector3.down + displacementX,
            Vector3.down + displacementX,
            Vector3.down + displacementX,
            Vector3.down + displacementX,

            Vector3.up + displacementX,
            Vector3.up + displacementX,
            Vector3.up + displacementX,
            Vector3.up + displacementX,

            displacementZ,
            displacementZ,
            displacementZ,
            displacementZ,

            displacementX,
            displacementX,
            displacementX,
            displacementX,

            Vector3.up + displacementX + displacementZ,
            Vector3.up + displacementX + displacementZ,
            Vector3.up + displacementX + displacementZ,

            Vector3.down + displacementX + displacementZ,
            Vector3.down + displacementX + displacementZ,
            Vector3.down + displacementX + displacementZ,

            displacementZ + displacementX,
            displacementZ + displacementX,
            displacementZ + displacementX,
            displacementZ + displacementX,

        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }

}
