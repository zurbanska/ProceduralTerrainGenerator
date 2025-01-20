using UnityEngine;
using UnityEngine.UIElements;

public class UIColorPicker : MonoBehaviour
{
    [SerializeField] private Material skyMaterial;
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private Material waterMaterial;

    public Slider skyColorSlider_red, skyColorSlider_green, skyColorSlider_blue;
    public VisualElement oldSkycolorPreview;
    public VisualElement newSkycolorPreview;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var colorPicker = root.Q<TemplateContainer>("sky-color-picker");

        skyColorSlider_red = colorPicker.Q<Slider>("red-slider");
        skyColorSlider_red.value = skyMaterial.GetColor("_Tint").r;

        skyColorSlider_green = colorPicker.Q<Slider>("green-slider");
        skyColorSlider_green.value = skyMaterial.GetColor("_Tint").g;

        skyColorSlider_blue = colorPicker.Q<Slider>("blue-slider");
        skyColorSlider_blue.value = skyMaterial.GetColor("_Tint").b;

        oldSkycolorPreview = colorPicker.Q<VisualElement>("old-color-preview");
        oldSkycolorPreview.style.backgroundColor = skyMaterial.GetColor("_Tint");
        newSkycolorPreview = colorPicker.Q<VisualElement>("new-color-preview");
        newSkycolorPreview.style.backgroundColor = skyMaterial.GetColor("_Tint");

        skyColorSlider_red.RegisterValueChangedCallback(e => SkyColorChanged());
        skyColorSlider_green.RegisterValueChangedCallback(e => SkyColorChanged());
        skyColorSlider_blue.RegisterValueChangedCallback(e => SkyColorChanged());
    }

    void SkyColorChanged()
    {
        Color newColor = new Color(skyColorSlider_red.value, skyColorSlider_green.value, skyColorSlider_blue.value);
        skyMaterial.SetColor("_Tint", newColor);
        waterMaterial.SetColor("_SkyColor", newColor);
        oldSkycolorPreview.style.backgroundColor = newColor;
        newSkycolorPreview.style.backgroundColor = newColor;

        // update environment lighting (for accurate terrain color)
        DynamicGI.UpdateEnvironment();
    }

}
