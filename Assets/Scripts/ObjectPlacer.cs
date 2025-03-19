using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    private GameObject tree1;
    private GameObject tree2;

    private MeshFilter meshFilter;
    private List<Vector3> validPositions;


    void Awake()
    {
        LoadResources();
    }

    public void PlaceObjects(TerrainData terrainData)
    {
        System.Random newRandom = new(1);
        meshFilter = GetComponent<MeshFilter>();

        if (this == null || meshFilter == null || meshFilter.sharedMesh == null || terrainData == null) return;

        Mesh mesh = meshFilter.sharedMesh;

        DestroyObjects();
        LoadResources();

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] normals = mesh.normals;

        Transform meshTransform = meshFilter.transform;

        validPositions = new List<Vector3>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = meshTransform.TransformPoint(vertices[triangles[i]]);
            Vector3 v1 = meshTransform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 v2 = meshTransform.TransformPoint(vertices[triangles[i + 2]]);

            Vector3 normal = normals[triangles[i]];

            Vector3 centerPoint = (v0 + v1 + v2) / 3f;
            centerPoint.y -= 0.2f;

            if (Vector3.Angle(normal, Vector3.up) < 45f && centerPoint.y > terrainData.waterLevel + 1f ) validPositions.Add(centerPoint);
        }

        validPositions.Sort((a, b) =>
        {
            int compareX = a.x.CompareTo(b.x);
            if (compareX != 0) return compareX;

            int compareY = a.y.CompareTo(b.y);
            if (compareY != 0) return compareY;

            return a.z.CompareTo(b.z);
        });

        foreach (Vector3 pos in validPositions)
        {
            if (newRandom.Next(0,1000) < terrainData.objectDensity)
            {
                GameObject treeToInstantiate = Random.value < 0.5f ? tree1 : tree2;

                GameObject newTree = Instantiate(treeToInstantiate);
                newTree.transform.position = pos;
                newTree.transform.Rotate(0, Random.value * 360, 0);
                newTree.layer = LayerMask.NameToLayer("Objects");
                newTree.transform.parent = transform;
                newTree.transform.localScale = Vector3.one * 1.5f;
            }
        }
    }


    private void DestroyObjects()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.layer == LayerMask.NameToLayer("Objects"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    public void DestroyObjectsInArea(Bounds area)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.layer == LayerMask.NameToLayer("Objects") && area.Contains(child.gameObject.transform.position))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private void LoadResources()
    {
        tree1 = Resources.Load("Prefabs/tree1", typeof(GameObject)) as GameObject;
        tree2 = Resources.Load("Prefabs/tree2", typeof(GameObject)) as GameObject;
    }
}
