using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OBJExporter
{
    int count = 0;
    string fileName = "ExportedTerrain";

    public void ExportCombinedMesh(List<GameObject> objectList)
    {
        if (objectList == null || objectList.Count == 0) return;

        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop); // path to desktop
        string objFilePath = Path.Combine(path, fileName + ".obj");
        string mtlFilePath = Path.Combine(path, fileName + ".mtl");

        // ensure unique file name
        while (File.Exists(objFilePath))
        {
            count++;
            objFilePath = Path.Combine(path, fileName + $"({count}).obj");
            mtlFilePath = Path.Combine(path, fileName + $"({count}).mtl");
        }

        using (StreamWriter writer = new StreamWriter(objFilePath))
        {
            writer.Write(CombineTerrainChunksToObj(objectList));
        }

        using (StreamWriter writer = new StreamWriter(mtlFilePath))
        {
            writer.WriteLine("newmtl Terrain");
            writer.WriteLine("Kd 1.0 1.0 1.0");

            writer.WriteLine("newmtl Water");
            writer.WriteLine("Kd 0.2 0.6 0.8");

            writer.WriteLine("newmtl Tree");
            writer.WriteLine("Kd 0.5 0.8 0.4");
        }

        Debug.Log("Terrain exported to: " + objFilePath);
    }

    private string CombineTerrainChunksToObj(List<GameObject> objectList)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int vertexOffset = 0; // keep track of vertex indices across chunks

        if (count == 0)
        {
            sb.AppendLine($"mtllib {fileName}.mtl");
        } else sb.AppendLine($"mtllib {fileName}({count}).mtl");


        foreach (var obj in objectList)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();

            if (mf == null || mf.sharedMesh == null) continue;

            Mesh mesh = mf.sharedMesh;
            Transform transform = obj.transform;

            if (obj.name == "Water")
            {
                sb.AppendLine("g Water");
                sb.AppendLine("usemtl Water");
            } else {
                sb.AppendLine("g Terrain");
                sb.AppendLine("usemtl Terrain");
            }

            // vertices (converted to world space)
            foreach (Vector3 v in mesh.vertices)
            {
                Vector3 worldPos = transform.TransformPoint(v);
                sb.AppendLine($"v {worldPos.x} {worldPos.y} {worldPos.z}");
            }

            // normals (converted to world space)
            foreach (Vector3 n in mesh.normals)
            {
                Vector3 worldNormal = transform.TransformDirection(n);
                sb.AppendLine($"vn {worldNormal.x} {worldNormal.y} {worldNormal.z}");
            }

            // UVs
            foreach (Vector2 uv in mesh.uv)
            {
                sb.AppendLine($"vt {uv.x} {uv.y}");
            }

            // faces (adjusting index with vertexOffset)
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v1 = triangles[i] + 1 + vertexOffset;
                int v2 = triangles[i + 1] + 1 + vertexOffset;
                int v3 = triangles[i + 2] + 1 + vertexOffset;
                sb.AppendLine($"f {v1}//{v1} {v2}//{v2} {v3}//{v3}");
            }

            vertexOffset += mesh.vertexCount; // increase vertex offset for next chunk

            foreach (Transform child in obj.transform)
            {
                if (child.GetComponent<MeshFilter>() == obj.GetComponent<MeshFilter>())
                    continue;

                MeshFilter childMeshFilter = child.GetComponentInChildren<MeshFilter>();
                if (childMeshFilter == null || childMeshFilter.sharedMesh == null) continue;

                Mesh childMesh = childMeshFilter.sharedMesh;
                Transform childTransform = child.transform;

                sb.AppendLine("g Tree"); // assume child objects are trees
                sb.AppendLine("usemtl Tree");

                // vertices (converted to world space)
                foreach (Vector3 v in childMesh.vertices)
                {
                    Vector3 worldPos = childTransform.TransformPoint(v);
                    sb.AppendLine($"v {worldPos.x} {worldPos.y} {worldPos.z}");
                }

                // normals (converted to world space)
                foreach (Vector3 n in childMesh.normals)
                {
                    Vector3 worldNormal = childTransform.TransformDirection(n);
                    sb.AppendLine($"vn {worldNormal.x} {worldNormal.y} {worldNormal.z}");
                }

                // UVs
                foreach (Vector2 uv in childMesh.uv)
                {
                    sb.AppendLine($"vt {uv.x} {uv.y}");
                }

                // faces (adjusting index with vertexOffset)
                int[] childTriangles = childMesh.triangles;
                for (int i = 0; i < childTriangles.Length; i += 3)
                {
                    int v1 = childTriangles[i] + 1 + vertexOffset;
                    int v2 = childTriangles[i + 1] + 1 + vertexOffset;
                    int v3 = childTriangles[i + 2] + 1 + vertexOffset;
                    sb.AppendLine($"f {v1}//{v1} {v2}//{v2} {v3}//{v3}");
                }

                vertexOffset += childMesh.vertexCount;
            }

        }

        return sb.ToString();
    }
}
