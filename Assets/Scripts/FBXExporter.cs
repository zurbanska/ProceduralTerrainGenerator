#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;

public class FBXExporter : MonoBehaviour
{

    public void ExportToFBX(List<GameObject> objectList)
    {
        if (objectList == null || objectList.Count == 0) return;

        // empty GameObject to hold combined objects
        GameObject combinedObject = new();

        foreach (var obj in objectList)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();

            if (mf == null || mf.sharedMesh == null) continue;

            // create a copy of the object and parent it to combinedObject
            GameObject objCopy = Instantiate(obj, combinedObject.transform);
        }


        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop); // path to desktop
        string fileName = "ExportedTerrain";
        string filePath = Path.Combine(path, fileName + ".fbx");

        // ensure unique file name
        int count = 1;
        while (File.Exists(filePath))
        {
            filePath = Path.Combine(path, fileName + $"({count}).fbx");
            count++;
        }

        ModelExporter.ExportObject(filePath, combinedObject);
        Destroy(combinedObject);

        Debug.Log("Terrain exported to: " + filePath);

    }

}
#endif