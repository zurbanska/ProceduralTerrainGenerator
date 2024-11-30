Shader "Custom/normals"
{
    Properties
    {
        _Intensity ("Intensity", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(v.normal); // Pass the normal to the fragment shader
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 color = (i.normal * 0.5 + 0.5) * _Intensity;
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}