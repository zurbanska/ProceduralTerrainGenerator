using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DepthTexture : MonoBehaviour
{

    public Material mat;
    public float DepthLevel = 1.0F;

    private void Awake() {
        if(Camera.main.depthTextureMode != DepthTextureMode.Depth)
            Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }

    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        mat.SetFloat("_DepthLevel", DepthLevel);
        Graphics.Blit(source, destination, mat);
    }

}
