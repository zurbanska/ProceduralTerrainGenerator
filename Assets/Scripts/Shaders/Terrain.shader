Shader "Custom/Terrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Texture1 ("Texture 1", 2D) = "white" {}
        _Texture2 ("Texture 2", 2D) = "white" {}

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

            #include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 normal: TEXCOORD1;
            };

            sampler2D _Texture1;
            sampler2D _Texture2;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                fixed4 tex1 = tex2D(_Texture1, i.uv);
                fixed4 tex2 = tex2D(_Texture2, i.uv);
                fixed4 blendedTexture = lerp(tex1, tex2, i.color.r);

                float3 normal = i.normal;
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float3 newNormal = float3(i.normal.x, i.normal.y, i.normal.z) * 0.3;
                float3 lightFallOff = max(0.4, dot(lightDir, normal));


                return float4(i.color * lightFallOff + float3(0.1, 0.15, 0), 0);
                // return float4(i.color, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
