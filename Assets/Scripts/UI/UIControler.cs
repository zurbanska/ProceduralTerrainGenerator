using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UIControler : MonoBehaviour
{
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private Camera cam;
    [SerializeField] private Light mainLight;
    private bool autoUpdate = false;

    private Button generalTabButton;
    private Button shaderTabButton;

    private ScrollView generalSettingsPanel;
    private ScrollView shaderSettingsPanel;

    private List<Button> Tabs;
    private List<ScrollView> Panels;


    public Toggle camMoveToggle;
    public Toggle autoUpdateToggle;

    public Toggle randomSeedToggle;
    public IntegerField seedField;
    public Slider isoLevelSlider;
    public SliderInt octavesSlider;
    public FloatField persistenceField;
    public FloatField lacunarityField;
    public FloatField scaleField;
    public Slider smoothnessSlider;

    public IntegerField renderDistField;
    public SliderInt lodSlider;
    public FloatField groundLevelField;
    public FloatField waterLevelField;
    public SliderInt objectDensitySlider;

    public Toggle terraformToggle;
    public FloatField brushSizeField;
    public Slider brushStrengthSlider;

    public Button genTerrainButton;
    public Button exportTerrainButton;
    public Button exportScreenshotButton;


    public Slider timeSlider;

    public Button settingsButton;
    public VisualElement settingsBox;

    public VisualElement loadingScreen;


    void Start()
    {
        Tabs = new();
        Panels = new();

        var root = GetComponent<UIDocument>().rootVisualElement;

        loadingScreen = root.Q<VisualElement>("loading-screen");
        loadingScreen.style.display = DisplayStyle.None;

        generalSettingsPanel = root.Q<ScrollView>("settings-panel");
        shaderSettingsPanel = root.Q<ScrollView>("shader-settings-panel");

        Panels.Add(generalSettingsPanel);
        Panels.Add(shaderSettingsPanel);

        generalTabButton = root.Q<Button>("general-tab-button");
        generalTabButton.clicked += () => TabSwitched(generalTabButton, generalSettingsPanel);

        shaderTabButton = root.Q<Button>("shader-tab-button");
        shaderTabButton.clicked += () => TabSwitched(shaderTabButton, shaderSettingsPanel);

        Tabs.Add(generalTabButton);
        Tabs.Add(shaderTabButton);


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

        smoothnessSlider = root.Q<Slider>("smoothness-slider");
        smoothnessSlider.RegisterValueChangedCallback(e => SmoothnessChanged(e.newValue));
        smoothnessSlider.value = terrainManager.terrainData.smoothLevel;


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

        brushStrengthSlider = root.Q<Slider>("brush-strength-slider");
        brushStrengthSlider.RegisterValueChangedCallback(e => BrushStrengthChanged(e.newValue));
        brushStrengthSlider.value = cam.GetComponent<TerraformingCamera>().brushStrength;


        genTerrainButton = root.Q<Button>("generate-terrain-button");
        genTerrainButton.clicked += GenTerrainButtonPressed;

        exportTerrainButton = root.Q<Button>("export-terrain-button");
        exportTerrainButton.clicked += ExportTerrainButtonPressed;

        exportScreenshotButton = root.Q<Button>("export-screenshot-button");
        exportScreenshotButton.clicked += ExportScreenshotButtonPressed;


        timeSlider = root.Q<Slider>("time-slider");
        timeSlider.RegisterValueChangedCallback(e => TimeChanged(e.newValue));
        timeSlider.value = mainLight.GetComponent<TimeControler>().time;


        settingsButton = root.Q<Button>("settings-button");
        settingsButton.clicked += SettingsButtonPressed;

        settingsBox = root.Q<VisualElement>("settings-box");
    }


    void TabSwitched(Button chosenTabButton, ScrollView chosenPanel)
    {
        foreach (var tab in Tabs)
        {
            tab.AddToClassList("tab-button-inactive");
        }

        foreach (var panel in Panels)
        {
            panel.style.display = DisplayStyle.None;
        }

        chosenTabButton.RemoveFromClassList("tab-button-inactive");
        chosenPanel.style.display = DisplayStyle.Flex;
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

    void SmoothnessChanged(float newValue) {
        terrainManager.terrainData.smoothLevel = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }



    void RenderDistChanged(int newValue)
    {
        if (newValue < 0)
        {
            newValue = 0;
            renderDistField.value = newValue;
        }
        if (newValue > 10) // max recommended render dist
        {
            newValue = 10;
            renderDistField.value = newValue;
        }

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
        if (newValue < 0)
        {
            newValue = 0;
            waterLevelField.value = newValue;
        }
        if (newValue > terrainManager.chunkHeight)
        {
            newValue = terrainManager.chunkHeight;
            waterLevelField.value = newValue;
        }

        terrainManager.terrainData.waterLevel = newValue;
        if (autoUpdate) terrainManager.UpdateChunks(false);
    }

    void ObjectDensityChanged(int newValue)
    {
        terrainManager.terrainData.objectDensity = newValue;
        if (autoUpdate) terrainManager.UpdateChunks(false);
    }



    void TerraformToggled(bool isOn)
    {
        terrainManager.allowTerraforming = isOn;
    }

    void BrushSizeChanged(float newValue)
    {
        if (newValue < 0)
        {
            newValue = 0;
            brushSizeField.value = newValue;
        }

        cam.GetComponent<TerraformingCamera>().brushSize = newValue;
    }

    void BrushStrengthChanged(float newValue)
    {
        cam.GetComponent<TerraformingCamera>().brushStrength = newValue;
    }



    void TimeChanged(float newValue)
    {
        mainLight.GetComponent<TimeControler>().SetTime(newValue);
    }


    void GenTerrainButtonPressed()
    {
        terrainManager.GenerateChunks();
        seedField.value = terrainManager.terrainData.seed;
    }

    void ExportTerrainButtonPressed()
    {
        StartCoroutine(ExportTerrainCoroutine());
    }

    void ExportScreenshotButtonPressed()
    {
        StartCoroutine(ExportScreenshotCoroutine());
    }

    private IEnumerator ExportTerrainCoroutine()
    {
        loadingScreen.style.display = DisplayStyle.Flex;
        yield return null;
        terrainManager.ExportTerrainMesh();
        loadingScreen.style.display = DisplayStyle.None;
    }

    private IEnumerator ExportScreenshotCoroutine()
    {
        settingsButton.style.display = DisplayStyle.None;
        settingsBox.style.display = DisplayStyle.None;

        yield return new WaitForEndOfFrame();

        terrainManager.ExportScreenshot();

        yield return new WaitForEndOfFrame();

        settingsButton.style.display = DisplayStyle.Flex;
        settingsBox.style.display = DisplayStyle.Flex;
    }


    void SettingsButtonPressed()
    {
        if (settingsBox.style.display == DisplayStyle.Flex)
        {
            settingsBox.style.display = DisplayStyle.None;
        } else settingsBox.style.display = DisplayStyle.Flex;
    }

}
