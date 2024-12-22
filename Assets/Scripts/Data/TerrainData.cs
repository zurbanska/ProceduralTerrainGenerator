using UnityEngine;

[CreateAssetMenu(fileName = "NoiseData2")]
public class TerrainData : ScriptableObject
{
    public float seed;

    public float waterLevel;
    public float groundLevel;


    public int octaves;
    public float persistence;
    public float lacunarity;
    public float scale;


    public float offsetX;
    public float offsetZ;

}