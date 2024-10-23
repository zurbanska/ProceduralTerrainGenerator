using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TerrainManager))]
public class TerrainManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainManager terrain = (TerrainManager)target;

        if (DrawDefaultInspector ())
        {
            terrain.CreateChunk();
        }

        if (GUILayout.Button ("Generate"))
        {
            terrain.CreateChunk();
        }
    }
}
