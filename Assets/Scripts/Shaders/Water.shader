Shader "Custom/Water"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.0, 0.5, 1.0, 1.0)
        _DepthStart ("Depth Start", Float) = 5.0
        _DepthEnd ("Depth End", Float) = 20.0
        _WaterLevel ("Water Level", float) = 10
        _Cubemap ("Reflection Cubemap", Cube) = "" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 normal : TEXCOORD4;
            };

            float4 _BaseColor;
            float _DepthStart;
            float _DepthEnd;
            float _WaterLevel;
            samplerCUBE _Cubemap;
            sampler2D _NormalMap;
            float _NormalStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos); // For depth lookup
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.normal = v.normal;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float terrainDepth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos))));

                float camDist = distance(i.worldPos, _WorldSpaceCameraPos);

                float waterDepth = terrainDepth - _WaterLevel;

                float depthFactor2 = saturate((waterDepth - _DepthStart) / (_DepthEnd - _DepthStart));

                float depthFactor = saturate((terrainDepth - _DepthStart) / (_DepthEnd - _DepthStart));

                float4 finalColor = _BaseColor * depthFactor2;
                finalColor.a = lerp(0.6, 1, depthFactor2);
                // finalColor *= depthFactor;
                // return finalColor;

                half3 normalTex = tex2D(_NormalMap, i.uv).rgb * 2.0 - 1.0;
                normalTex = normalize(normalTex) + _NormalStrength;

                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                half3 worldRefl = reflect(-_WorldSpaceLightPos0.xyz, normalTex);
                // half3 worldRefl = reflect(-worldViewDir.xyz, normalTex);

                // default skybox cubemap
                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);

                // custom cubemap
                // half4 skyData = texCUBE(_Cubemap, worldRefl);

                fixed4 c = 0;
                c.rgb = skyColor.rgb * 0.7 + _BaseColor * 0.3;
                c.rgb *= depthFactor2;

                c.a = lerp(0.6, 1, depthFactor2);
                return c;
                // return float4(i.normal.xxx + 0.5, 1);
                // return float4(depthFactor2.xxx * 0.5, 1);
                // return float4(0,0,0,0);

                // return float4(depthFactor.xxx, 1);

                // return float4(normalTex, 1);


            }
            ENDCG
        }

    }
    FallBack "Diffuse"
}
