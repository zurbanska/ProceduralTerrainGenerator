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

    public Slider colorSlider_red, colorSlider_green, colorSlider_blue;
    public VisualElement oldColorPreview;
    public VisualElement newColorPreview;


    public Button skyColorPicker;
    public Button fogColorPicker;
    public Button lightColorPicker;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var colorPicker = root.Q<TemplateContainer>("color-picker");

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


        root.RegisterCallback<MouseDownEvent>(e =>
        {
            if (!colorPicker.worldBound.Contains(e.mousePosition)) colorPicker.style.display = DisplayStyle.None;
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


    void ColorChanged()
    {
        Color newColor = new Color(colorSlider_red.value, colorSlider_green.value, colorSlider_blue.value);
        UpdateColorPreview(newColor);

        currentAction?.Invoke(newColor);
    }

}
