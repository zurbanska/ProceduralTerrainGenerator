using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MeshExporter
{

    public void ExportCombinedMesh(Dictionary<Vector2, GameObject> terrainChunks)
    {
        if (terrainChunks == null || terrainChunks.Count == 0) return;

        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, "CombinedMesh.obj");

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.Write(CombineTerrainChunksToObj(terrainChunks));
        }

        Debug.Log("Mesh exported to: " + filePath);
    }

    private string CombineTerrainChunksToObj(Dictionary<Vector2, GameObject> terrainChunks)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int vertexOffset = 0; // keep track of vertex indices across chunks

        foreach (var chunk in terrainChunks)
        {
            GameObject chunkObject = chunk.Value;
            MeshFilter mf = chunkObject.GetComponent<MeshFilter>();

            if (mf == null || mf.sharedMesh == null) continue;

            Mesh mesh = mf.sharedMesh;
            Transform transform = chunkObject.transform;

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
                sb.AppendLine($"f {v1} {v2} {v3}");
            }

            vertexOffset += mesh.vertexCount; // increase vertex offset for next chunk
        }

        return sb.ToString();
    }
}
