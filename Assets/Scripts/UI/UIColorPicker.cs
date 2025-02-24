using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIColorPicker : MonoBehaviour
{
    [SerializeField] private Material skyMaterial;
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private List<Material> treeeMaterials;

    [SerializeField] private Light mainLight;

    private Color currentColor;
    private Action<Color> currentAction;
    private Button currentGradientButton;

    public Slider colorSlider_red, colorSlider_green, colorSlider_blue;
    public VisualElement oldColorPreview;
    public VisualElement newColorPreview;


    public Button skyColorPicker;
    public Button fogColorPicker;
    public Button lightColorPicker;

    public GradientBuilder gradientBuilder = new();

    public TemplateContainer gradientEditor;
    public Button terrainGradientPicker;
    List<Button> gradientButtons;
    List<Slider> gradientCutoffs;

    VisualElement background;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var colorPicker = root.Q<TemplateContainer>("color-picker");
        gradientEditor = root.Q<TemplateContainer>("gradient-picker");

        background = root.Q<VisualElement>("background");

        colorSlider_red = colorPicker.Q<Slider>("red-slider");
        colorSlider_green = colorPicker.Q<Slider>("green-slider");
        colorSlider_blue = colorPicker.Q<Slider>("blue-slider");

        oldColorPreview = colorPicker.Q<VisualElement>("old-color-preview");
        newColorPreview = colorPicker.Q<VisualElement>("new-color-preview");

        colorSlider_red.RegisterValueChangedCallback(e => ColorChanged());
        colorSlider_green.RegisterValueChangedCallback(e => ColorChanged());
        colorSlider_blue.RegisterValueChangedCallback(e => ColorChanged());

        skyColorPicker = root.Q<Button>("sky-color-picker");
        skyColorPicker.style.backgroundColor = skyMaterial.GetColor("_Tint");
        skyColorPicker.clicked += () => currentAction = ChangeSkyColor;
        skyColorPicker.clicked += () => colorPicker.style.display = DisplayStyle.Flex;
        skyColorPicker.clicked += () => InitColorPicker(skyMaterial.GetColor("_Tint"));

        fogColorPicker = root.Q<Button>("fog-color-picker");
        fogColorPicker.style.backgroundColor = terrainMaterial.GetColor("_FogColor");
        fogColorPicker.clicked += () => currentAction = ChangeFogColor;
        fogColorPicker.clicked += () => colorPicker.style.display = DisplayStyle.Flex;
        fogColorPicker.clicked += () => InitColorPicker(terrainMaterial.GetColor("_FogColor"));

        lightColorPicker = root.Q<Button>("light-color-picker");
        lightColorPicker.style.backgroundColor = mainLight.GetComponent<TimeControler>().baseLightColor;
        lightColorPicker.clicked += () => currentAction = ChangeLightColor;
        lightColorPicker.clicked += () => colorPicker.style.display = DisplayStyle.Flex;
        lightColorPicker.clicked += () => InitColorPicker(mainLight.GetComponent<TimeControler>().baseLightColor);


        terrainGradientPicker = root.Q<Button>("terrain-gradient-picker");
        terrainGradientPicker.clicked += () => gradientEditor.style.display = DisplayStyle.Flex;

        gradientButtons = root.Query<Button>(className: "gradient-button").ToList();
        gradientCutoffs = root.Query<Slider>(className: "gradient-cutoff").ToList();

        foreach (Button button in gradientButtons) {
            button.clicked += () => colorPicker.style.display = DisplayStyle.Flex;
            button.clicked += () => currentAction = ChangeGradient;
            button.clicked += () => currentGradientButton = button;
            button.clicked += () => InitColorPicker(button.style.backgroundColor.value);
        }

        foreach (Slider gradientCutoff in gradientCutoffs) {
            gradientCutoff.RegisterValueChangedCallback(e => ChangeGradient(currentGradientButton.style.backgroundColor.value));
        }


        root.RegisterCallback<MouseDownEvent>(e =>
        {
            if (!colorPicker.worldBound.Contains(e.mousePosition)) {
                colorPicker.style.display = DisplayStyle.None;
                gradientEditor.style.display = DisplayStyle.None;
            }
        });
    }

    void InitColorPicker(Color color)
    {
        colorSlider_red.value = color.r;
        colorSlider_green.value = color.g;
        colorSlider_blue.value = color.b;

        UpdateColorPreview(color);
    }

    void UpdateColorPreview(Color newColor)
    {
        oldColorPreview.style.backgroundColor = newColor;
        newColorPreview.style.backgroundColor = newColor;
    }

    void ChangeSkyColor(Color newColor)
    {
        // newColor *= mainLight.color;

        skyMaterial.SetColor("_Tint", newColor);
        waterMaterial.SetColor("_SkyColor", newColor);
        foreach (var mat in treeeMaterials)
        {
            mat.SetColor("_SkyColor", newColor);
        }

        skyColorPicker.style.backgroundColor = newColor;

        // update environment lighting (for accurate terrain color)
        DynamicGI.UpdateEnvironment();
    }

    void ChangeFogColor(Color newColor)
    {
        terrainMaterial.SetColor("_FogColor", newColor);
        waterMaterial.SetColor("_FogColor", newColor);

        foreach (var mat in treeeMaterials)
        {
            mat.SetColor("_FogColor", newColor);
        }

        fogColorPicker.style.backgroundColor = newColor;
    }

    void ChangeLightColor(Color newColor)
    {
        // mainLight.color = newColor;
        mainLight.GetComponent<TimeControler>().SetBaseLightColor(newColor);
        lightColorPicker.style.backgroundColor = newColor;
    }


    void ChangeGradient(Color newColor) {
        currentGradientButton.style.backgroundColor = newColor;

        var gradient = new Gradient();
        var colors = new GradientColorKey[gradientButtons.Count];
        var alphas = new GradientAlphaKey[gradientButtons.Count];

        for (int i = 0; i < gradientButtons.Count; i++)
        {
           colors[i] = new GradientColorKey(gradientButtons[i].style.backgroundColor.value, gradientCutoffs[i].value);
           alphas[i] = new GradientAlphaKey(1f, gradientCutoffs[i].value);
        }

        gradient.SetKeys(colors, alphas);

        Texture2D tex = gradientBuilder.GenerateGradientTexture(gradient);
        terrainGradientPicker.style.backgroundImage = tex;

        terrainMaterial.SetTexture("_GradientTex", tex);
    }


    void ColorChanged()
    {
        Color newColor = new Color(colorSlider_red.value, colorSlider_green.value, colorSlider_blue.value);
        UpdateColorPreview(newColor);

        currentAction?.Invoke(newColor);
    }

}
