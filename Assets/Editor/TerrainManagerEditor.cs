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
            terrain.GenerateChunks();
        }

        if (GUILayout.Button ("Generate"))
        {
            terrain.GenerateChunks();
        }
    }
}
