// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader "Lit/Diffuse With Shadows"
// {
//     Properties
//     {
//         [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
//     }
//     SubShader
//     {
//         Pass
//         {
//             Tags {"LightMode"="ForwardBase"}
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #include "UnityCG.cginc"
//             #include "Lighting.cginc"

//             // compile shader into multiple variants, with and without shadows
//             // (we don't care about any lightmaps yet, so skip these variants)
//             #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
//             // shadow helper functions and macros
//             #include "AutoLight.cginc"

//             struct v2f
//             {
//                 float2 uv : TEXCOORD0;
//                 SHADOW_COORDS(1) // put shadows data into TEXCOORD1
//                 fixed3 diff : COLOR0;
//                 fixed3 ambient : COLOR1;
//                 float4 pos : SV_POSITION;
//             };
//             v2f vert (appdata_base v)
//             {
//                 v2f o;
//                 o.pos = UnityObjectToClipPos(v.vertex);
//                 o.uv = v.texcoord;
//                 half3 worldNormal = UnityObjectToWorldNormal(v.normal);
//                 half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
//                 o.diff = nl * _LightColor0.rgb;
//                 o.ambient = ShadeSH9(half4(worldNormal,1));
//                 // compute shadows data
//                 TRANSFER_SHADOW(o)
//                 return o;
//             }

//             sampler2D _MainTex;

//             fixed4 frag (v2f i) : SV_Target
//             {
//                 fixed4 col = tex2D(_MainTex, i.uv);
//                 // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
//                 fixed shadow = SHADOW_ATTENUATION(i);
//                 // darken light's illumination with shadow, keep ambient intact
//                 fixed3 lighting = i.diff * shadow + i.ambient;
//                 col.rgb *= lighting;
//                 return col;
//             }
//             ENDCG
//         }

//         // shadow casting support
//         UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
//     }
// }


Shader "Unlit/SkyReflection Per Pixel"
{
    Properties
    {
        _Cubemap ("Reflection Cubemap", Cube) = "" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        // Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f {
                float3 worldPos : TEXCOORD0;
                half3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 pos : SV_POSITION;
            };

            samplerCUBE _Cubemap;
            sampler2D _NormalMap;
            float _NormalStrength;

            v2f vert (float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.worldPos = mul(unity_ObjectToWorld, vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.uv = uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                half3 normalTex = tex2D(_NormalMap, i.uv).rgb * 2.0 - 1.0;
                normalTex = normalize(normalTex) * _NormalStrength;

                half3 perturbedNormal = normalize(i.worldNormal + _NormalStrength * normalTex);
                // compute view direction and reflection vector
                // per-pixel here
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                half3 worldRefl = reflect(-worldViewDir, normalTex);

                // same as in previous shader
                half4 skyData = texCUBE(_Cubemap, worldRefl);
                fixed4 c = 0;
                c.rgb = skyData.rgb; // No HDR decode needed for custom cubemaps
                return c;
                // return float4(normalTex,1);
            }
            ENDCG
        }
    }
}