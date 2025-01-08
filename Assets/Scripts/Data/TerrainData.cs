using UnityEngine;

[CreateAssetMenu(fileName = "TerrainData")]
public class TerrainData : ScriptableObject
{
    public int seed;

    public float waterLevel;
    public float groundLevel;

    public float isoLevel;

    public int lod;

    public int octaves;
    public float persistence;
    public float lacunarity;
    public float scale;


    public float offsetX;
    public float offsetZ;


}