using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UIControler : MonoBehaviour
{
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private Camera cam;
    private bool autoUpdate = false;


    public Toggle camMoveToggle;
    public Toggle autoUpdateToggle;

    public Toggle randomSeedToggle;
    public IntegerField seedField;
    public Slider isoLevelSlider;
    public SliderInt octavesSlider;
    public FloatField persistenceField;
    public FloatField lacunarityField;
    public FloatField scaleField;

    public IntegerField renderDistField;
    public SliderInt lodSlider;
    public FloatField groundLevelField;
    public FloatField waterLevelField;
    public SliderInt objectDensitySlider;

    public Toggle terraformToggle;
    public FloatField brushSizeField;

    public Button genTerrainButton;

    public Button settingsButton;
    public VisualElement settingsBox;


    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        camMoveToggle = root.Q<Toggle>("movement-toggle");
        camMoveToggle.RegisterValueChangedCallback(e => CamMoveToggled(e.newValue));
        camMoveToggle.value = cam.GetComponent<CameraMove>().allowMove;

        autoUpdateToggle = root.Q<Toggle>("auto-update-toggle");
        autoUpdateToggle.RegisterValueChangedCallback(e => autoUpdate = !autoUpdate);


        randomSeedToggle = root.Q<Toggle>("random-seed-toggle");
        randomSeedToggle.RegisterValueChangedCallback(e => RandomSeedToggled(e.newValue));
        randomSeedToggle.value = terrainManager.randomSeed;

        seedField = root.Q<IntegerField>("seed-input");
        seedField.RegisterValueChangedCallback(e => SeedChanged(e.newValue));
        seedField.value = terrainManager.terrainData.seed;

        isoLevelSlider = root.Q<Slider>("iso-level-slider");
        isoLevelSlider.RegisterValueChangedCallback(e => ISOLevelChanged(e.newValue));
        isoLevelSlider.value = terrainManager.terrainData.isoLevel;

        octavesSlider = root.Q<SliderInt>("octaves-slider");
        octavesSlider.RegisterValueChangedCallback(e => OctavesChanged(e.newValue));
        octavesSlider.value = terrainManager.terrainData.octaves;

        persistenceField = root.Q<FloatField>("persistence-input");
        persistenceField.RegisterValueChangedCallback(e => PersistenceChanged(e.newValue));
        persistenceField.value = terrainManager.terrainData.persistence;

        lacunarityField = root.Q<FloatField>("lacunarity-input");
        lacunarityField.RegisterValueChangedCallback(e => LacunarityChanged(e.newValue));
        lacunarityField.value = terrainManager.terrainData.lacunarity;

        scaleField = root.Q<FloatField>("scale-input");
        scaleField.RegisterValueChangedCallback(e => ScaleChanged(e.newValue));
        scaleField.value = terrainManager.terrainData.scale;


        renderDistField = root.Q<IntegerField>("render-distance-input");
        renderDistField.RegisterValueChangedCallback(e => RenderDistChanged(e.newValue));
        renderDistField.value = terrainManager.renderDistance;

        lodSlider = root.Q<SliderInt>("lod-slider");
        lodSlider.RegisterValueChangedCallback(e => LODChanged(e.newValue));
        lodSlider.value = terrainManager.terrainData.lod;

        groundLevelField = root.Q<FloatField>("ground-level-input");
        groundLevelField.RegisterValueChangedCallback(e => GroundLevelChanged(e.newValue));
        groundLevelField.value = terrainManager.terrainData.groundLevel;

        waterLevelField = root.Q<FloatField>("water-level-input");
        waterLevelField.RegisterValueChangedCallback(e => WaterLevelChanged(e.newValue));
        waterLevelField.value = terrainManager.terrainData.waterLevel;

        objectDensitySlider = root.Q<SliderInt>("object-density-slider");
        objectDensitySlider.RegisterValueChangedCallback(e => ObjectDensityChanged(e.newValue));
        objectDensitySlider.value = terrainManager.terrainData.objectDensity;


        terraformToggle = root.Q<Toggle>("terraforming-toggle");
        terraformToggle.RegisterValueChangedCallback(e => TerraformToggled(e.newValue));
        terraformToggle.value = terrainManager.allowTerraforming;

        brushSizeField = root.Q<FloatField>("brush-size-input");
        brushSizeField.RegisterValueChangedCallback(e => BrushSizeChanged(e.newValue));
        brushSizeField.value = cam.GetComponent<TerraformingCamera>().brushSize;


        genTerrainButton = root.Q<Button>("generate-terrain-button");
        genTerrainButton.clicked += GenTerrainButtonPressed;


        settingsButton = root.Q<Button>("settings-button");
        settingsButton.clicked += SettingsButtonPressed;

        settingsBox = root.Q<VisualElement>("settings-box");
    }

    void CamMoveToggled(bool isOn)
    {
        cam.GetComponent<CameraMove>().allowMove = isOn;
    }


    void RandomSeedToggled(bool isOn)
    {
        terrainManager.randomSeed = isOn;
    }
    void SeedChanged(int newValue)
    {
        terrainManager.terrainData.seed = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    void ISOLevelChanged(float newValue)
    {
        terrainManager.terrainData.isoLevel = newValue;
        if (autoUpdate) terrainManager.UpdateChunks(false);
    }

    void OctavesChanged(int newValue)
    {
        terrainManager.terrainData.octaves = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    void PersistenceChanged(float newValue)
    {
        terrainManager.terrainData.persistence = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    void LacunarityChanged(float newValue)
    {
        terrainManager.terrainData.lacunarity = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    void ScaleChanged(float newValue)
    {
        terrainManager.terrainData.scale = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }



    void RenderDistChanged(int newValue)
    {
        terrainManager.renderDistance = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    void LODChanged(int newValue)
    {
        terrainManager.terrainData.lod = (int)Mathf.Pow(2, newValue - 1);
        if (autoUpdate) terrainManager.UpdateChunks(false);
    }

    void GroundLevelChanged(float newValue)
    {
        terrainManager.terrainData.groundLevel = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    void WaterLevelChanged(float newValue)
    {
        terrainManager.terrainData.waterLevel = newValue;
        if (autoUpdate) terrainManager.UpdateChunks(false);
    }

    void ObjectDensityChanged(int newValue)
    {
        terrainManager.terrainData.objectDensity = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }



    void TerraformToggled(bool isOn)
    {
        terrainManager.allowTerraforming = isOn;
    }

    void BrushSizeChanged(float newValue)
    {
        cam.GetComponent<TerraformingCamera>().brushSize = newValue;
    }



    void GenTerrainButtonPressed()
    {
        terrainManager.GenerateChunks();
        seedField.value = terrainManager.terrainData.seed;
    }

    void SettingsButtonPressed()
    {
        if (settingsBox.style.visibility == Visibility.Visible)
        {
            settingsBox.style.visibility = Visibility.Hidden;
        } else settingsBox.style.visibility = Visibility.Visible;
    }

}
