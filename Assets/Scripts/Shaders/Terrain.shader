Shader "Custom/Terrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (0.5, 0.5, 0.5, 1)
        _FogStart ("Fog Start Distance", Float) = 10
        _FogEnd ("Fog End Distance", Float) = 20
        _DepthLevel ("Depth Level", Range(1, 3)) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // Main Pass
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
                float3 worldPos : TEXCOORD1;
                float3 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 normal: TEXCOORD2;
            };

            float4 _FogColor;
            float _FogStart;
            float _FogEnd;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color = v.color;
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {

                float3 normal = normalize(i.normal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightFallOff = max(0.0, dot(lightDir, normal));
                float3 terrainColor = i.color * lightFallOff + float3(0.1, 0.15, 0);

                float distance = length(_WorldSpaceCameraPos - i.worldPos);
                float fogFactor = saturate((distance - _FogStart) / (_FogEnd - _FogStart));

                float3 finalColor = lerp(terrainColor, _FogColor.rgb, fogFactor);
                
                return float4(finalColor, 1.0);
            }
            ENDCG
        }

        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
        // ShadowCaster Pass
        Pass
        {
            Name "ShadowCaster"

            Tags { "LightMode" = "ShadowCaster" }

            Fog {Mode Off}
            ZWrite On ZTest LEqual Cull Off
            Offset 1, 1

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform sampler2D_float _CameraDepthTexture;
            uniform fixed _DepthLevel;
            uniform half4 _MainTex_TexelSize;

            struct uinput
            {
                float4 pos : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct uoutput
            {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
            };

            uoutput vert(uinput i)
            {
                uoutput o;
                o.pos = UnityObjectToClipPos(i.pos);
                o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, i.uv);
                return o;
            }

            fixed4 frag(uoutput o) : COLOR
            {
                float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, o.uv));
                depth = pow(Linear01Depth(depth), _DepthLevel);
                return depth;
            }
            ENDCG
        }
    }
    FallBack "VertexLit"
}
