using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NoiseData", menuName = "Terrain/Noise Data", order = 1)]
public class NoiseData : UpdatableData
{
    public Vector2 moreOffset = Vector2.zero;
    public float waterLevel = 10f;

    protected override void OnValidate() {
		base.OnValidate ();
	}
}