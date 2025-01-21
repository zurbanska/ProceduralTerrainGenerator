using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeControler : MonoBehaviour
{
    public float time;
    public Color baseLightColor;
    private Light mainLight;
    [SerializeField] private Gradient lightColorGradient;


    private void Start() {
        mainLight = GetComponent<Light>();
        baseLightColor = Color.white;
    }


    public void SetTime(float time)
    {
        this.time = time;
        transform.localRotation = Quaternion.Euler((time * 15f) - 90, time * 2 + 170, 0);

        mainLight.color = lightColorGradient.Evaluate(time / 24) * baseLightColor;
    }

    public void SetBaseLightColor(Color color)
    {
        baseLightColor = color;
        SetTime(time);
    }

}
