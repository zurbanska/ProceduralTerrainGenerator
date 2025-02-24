using UnityEngine;

[CreateAssetMenu(fileName = "TerrainData")]
public class TerrainData : ScriptableObject
{

    // terrain
    public float waterLevel;
    public float groundLevel;
    public int lod;


    // marching cubes
    public float isoLevel;

    // noise
    public int seed;
    public int octaves;
    public float persistence;
    public float lacunarity;
    public float scale;


    public float offsetX;
    public float offsetZ;


    // objects
    public int objectDensity;


    public Gradient gradient;

}