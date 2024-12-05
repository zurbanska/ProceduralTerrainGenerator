using UnityEngine;
using UnityEngine.UI;

public class TerrainVariableSliders : MonoBehaviour
{

    private TerrainManager terrainManager;

    void Start()
    {
        terrainManager = gameObject.GetComponent<TerrainManager>();
    }

    public void SetPersistence(Slider slider)
    {
        terrainManager.persistence = slider.value;
        // terrainManager.UpdateChunks(new Vector2(-7, 0));
    }
}
