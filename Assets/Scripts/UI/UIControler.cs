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


    private Toggle camMoveToggle;
    private Toggle autoUpdateToggle;

    private Toggle randomSeedToggle;
    private IntegerField seedField;
    private Slider isoLevelSlider;
    private SliderInt octavesSlider;
    private Slider persistenceSlider;
    private Slider lacunarityField;
    private FloatField scaleField;
    private Slider smoothnessSlider;

    private IntegerField renderDistField;
    private SliderInt lodSlider;
    private FloatField groundLevelField;
    private FloatField waterLevelField;
    private SliderInt objectDensitySlider;

    private Toggle terraformToggle;
    private FloatField brushSizeField;
    private Slider brushStrengthSlider;

    private Button genTerrainButton;
    private Button exportTerrainButton;
    private Button exportScreenshotButton;


    private Slider timeSlider;

    private Button settingsButton;
    private VisualElement settingsBox;

    private VisualElement loadingScreen;


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

        persistenceSlider = root.Q<Slider>("persistence-slider");
        persistenceSlider.RegisterValueChangedCallback(e => PersistenceChanged(e.newValue));
        persistenceSlider.value = terrainManager.terrainData.persistence;

        lacunarityField = root.Q<Slider>("lacunarity-slider");
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


    private void TabSwitched(Button chosenTabButton, ScrollView chosenPanel)
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

    private void CamMoveToggled(bool isOn)
    {
        cam.GetComponent<CameraMove>().allowMove = isOn;
    }


    private void RandomSeedToggled(bool isOn)
    {
        terrainManager.randomSeed = isOn;
    }
    private void SeedChanged(int newValue)
    {
        terrainManager.terrainData.seed = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    private void ISOLevelChanged(float newValue)
    {
        terrainManager.terrainData.isoLevel = newValue;
        if (autoUpdate) terrainManager.UpdateChunks(false);
    }

    private void OctavesChanged(int newValue)
    {
        terrainManager.terrainData.octaves = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    private void PersistenceChanged(float newValue)
    {
        terrainManager.terrainData.persistence = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    private void LacunarityChanged(float newValue)
    {
        terrainManager.terrainData.lacunarity = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    private void ScaleChanged(float newValue)
    {
        terrainManager.terrainData.scale = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    private void SmoothnessChanged(float newValue) {
        terrainManager.terrainData.smoothLevel = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }



    private void RenderDistChanged(int newValue)
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

    private void LODChanged(int newValue)
    {
        terrainManager.terrainData.lod = (int)Mathf.Pow(2, newValue - 1);
        if (autoUpdate) terrainManager.UpdateChunks(false);
    }

    private void GroundLevelChanged(float newValue)
    {
        terrainManager.terrainData.groundLevel = newValue;
        if (autoUpdate) terrainManager.UpdateChunks();
    }

    private void WaterLevelChanged(float newValue)
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

    private void ObjectDensityChanged(int newValue)
    {
        terrainManager.terrainData.objectDensity = newValue;
        if (autoUpdate) terrainManager.UpdateChunks(false);
    }



    private void TerraformToggled(bool isOn)
    {
        terrainManager.allowTerraforming = isOn;
    }

    private void BrushSizeChanged(float newValue)
    {
        if (newValue < 0)
        {
            newValue = 0;
            brushSizeField.value = newValue;
        }

        cam.GetComponent<TerraformingCamera>().brushSize = newValue;
    }

    private void BrushStrengthChanged(float newValue)
    {
        cam.GetComponent<TerraformingCamera>().brushStrength = newValue;
    }



    private void TimeChanged(float newValue)
    {
        mainLight.GetComponent<TimeControler>().SetTime(newValue);
    }


    private void GenTerrainButtonPressed()
    {
        terrainManager.GenerateChunks();
        seedField.value = terrainManager.terrainData.seed;
    }

    private void ExportTerrainButtonPressed()
    {
        StartCoroutine(ExportTerrainCoroutine());
    }

    private void ExportScreenshotButtonPressed()
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


    private void SettingsButtonPressed()
    {
        if (settingsBox.style.display == DisplayStyle.Flex)
        {
            settingsBox.style.display = DisplayStyle.None;
        } else settingsBox.style.display = DisplayStyle.Flex;
    }

}
