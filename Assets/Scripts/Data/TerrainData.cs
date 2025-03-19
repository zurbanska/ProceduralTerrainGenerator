using UnityEngine;

[CreateAssetMenu(fileName = "TerrainData")]
public class TerrainData : ScriptableObject
{

    // terrain
    public float waterLevel = 16;
    public float groundLevel = 20;
    public float smoothLevel = 0;
    public int lod = 1;


    // marching cubes
    public float isoLevel = 0.9f;

    // noise
    public int seed = 0;
    public int octaves = 3;
    public float persistence = 0.7f;
    public float lacunarity = 2.1f;
    public float scale = 10;


    public float offsetX = 0;
    public float offsetZ = 0;


    // objects
    public int objectDensity = 10;


    public Gradient gradient = new();

}