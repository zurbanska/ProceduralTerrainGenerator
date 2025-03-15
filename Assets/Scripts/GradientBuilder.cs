using UnityEngine;

public class GradientBuilder
{
    public int textureWidth = 256;

    public Texture2D GenerateGradientTexture(Gradient gradient)
    {
        if (gradient == null) return null;

        Texture2D texture = new Texture2D(textureWidth, 1, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < textureWidth; x++)
        {
            float t = x / (float)(textureWidth - 1); // normalize x to [0,1]
            Color color = gradient.Evaluate(t);
            texture.SetPixel(x, 0, color);
        }

        texture.Apply();
        return texture;
    }


}
