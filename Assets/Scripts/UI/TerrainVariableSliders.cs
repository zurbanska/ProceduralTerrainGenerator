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
        terrainManager.terrainData.persistence = slider.value;
    }

    public void SetTerraforming(Toggle toggle)
    {
        terrainManager.allowTerraforming = toggle.isOn;
    }
}
