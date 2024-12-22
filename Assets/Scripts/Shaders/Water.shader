Shader "Custom/Water"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.0, 0.5, 1.0, 1.0)
        _DeepColor ("Deep Color", Color) = (0.0, 0.5, 1.0, 1.0)
        _DepthStart ("Depth Start", Float) = 5.0
        _DepthEnd ("Depth End", Float) = 20.0
        _WaterLevel ("Water Level", float) = 10
        // _Cubemap ("Reflection Cubemap", Cube) = "" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 1)) = 0.5

        _DepthFadeDist ("Depth Fade Distance", Float) = 0.5
        _WaveFrequency ("Wave Frequency", Float) = 0.5
        _WaveSpeed ("Wave Speed", Float) = 0.5
        _WaveStrength ("Wave Strength", Float) = 0.5
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
                float4 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 normal : TEXCOORD4;
            };

            float4 _BaseColor;
            float4 _DeepColor;
            float _DepthStart;
            float _DepthEnd;
            float _WaterLevel;
            // samplerCUBE _Cubemap;
            sampler2D _NormalMap;
            float _NormalStrength;

            float _DepthFadeDist;

            float _WaveFrequency;
            float _WaveSpeed;
            float _WaveStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos); // For depth lookup
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyzw;

                float wave = sin(o.worldPos.x * _WaveFrequency + _Time.y * _WaveSpeed) +
                             cos(o.worldPos.z * _WaveFrequency + _Time.y * _WaveSpeed);
                // o.worldPos.y += wave;
                // o.screenPos.y += wave;
                o.pos.y += wave * _WaveStrength;

                float dx = cos(o.worldPos.x * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveFrequency;
                float dz = -sin(o.worldPos.z * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveFrequency;
                float3 vnormal = normalize(v.normal);
                float3 normal = float3(vnormal.x - vnormal.y * dx * _WaveStrength, vnormal.y, vnormal.z - vnormal.y * dz * _WaveStrength);

                // float3 normal = normalize(v.normal);
                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.normal = normal;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float terrainDepth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos))));

                float camDist = distance(i.worldPos, _WorldSpaceCameraPos);

                float waterDepth1 = terrainDepth - _WaterLevel;

                float depthFactor2 = saturate((waterDepth1 - _DepthStart) / (_DepthEnd - _DepthStart));

                // float depthFactor = saturate((terrainDepth - _DepthStart) / (_DepthEnd - _DepthStart));

                // float4 finalColor = _BaseColor * depthFactor2;
                // finalColor.a = lerp(0.6, 1, depthFactor2);
                // // finalColor *= depthFactor;
                // // return finalColor;

                half3 normalTex = tex2D(_NormalMap, i.uv).rgb * 2.0 - 1.0;
                normalTex = normalize(normalTex) + _NormalStrength;

                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                // half3 worldRefl = reflect(-_WorldSpaceLightPos0.xyz, normalTex);
                half3 worldRefl = reflect(-worldViewDir.xyz, normalTex);

                // default skybox cubemap
                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);

                float3 normal = _WorldSpaceLightPos0.xyz * i.normal;
                // float3 normal = i.normal;
                float3 baseColor = _BaseColor.rgb + normal.xxx + normal.zzz + normal.yyy;

                fixed4 c = 0;
                c.rgb = skyColor.rgb * 0.7 + baseColor * 0.3;
                c.rgb *= depthFactor2;

                c.a = lerp(0.6, 1, depthFactor2);
                return c;


                // return float4((i.normal * 0.5 + 0.5), 1);


                // // Sample depth at the current screen position
                // float screenDepth = Linear01Depth(tex2D(_CameraDepthTexture, i.screenPos.xy / i.screenPos.w).r);

                // // Map depth to a factor for color blending
                // float depthFactor = saturate(screenDepth * _DepthFadeDist);

                // // Interpolate between shallow and deep water colors
                // float4 waterColor = lerp(_BaseColor, float4(0,0,0,1), screenDepth * 2000);

                // return waterColor;

                float sceneDepth = Linear01Depth(tex2D(_CameraDepthTexture, i.screenPos.xy / i.screenPos.w)) * 1300;



                // float sceneDepth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos))));

                sceneDepth = saturate(sceneDepth);

                // float4 screenPos = i.screenPos;

                // half waterDepth = sceneDepth - screenPos.a;

                // waterDepth /= _DepthFadeDist;
                // waterDepth = saturate(waterDepth);
                // waterDepth = 1 - waterDepth;

                float waterDepth = i.worldPos.a - sceneDepth;
                // waterDepth /= _DepthFadeDist;
                waterDepth = saturate(waterDepth);
                waterDepth = 1 - waterDepth;

                float depthFactor = saturate((waterDepth - _DepthStart) / (_DepthEnd - _DepthStart));

                // skyColor = skyColor.rgb + normal.xxx + normal.zzz;
                float3 waterColor = lerp(baseColor, skyColor * 0.8 + baseColor * 0.2, waterDepth);
                // waterColor.a = lerp(0, 1, waterDepth);

                float4 finalColor;
                finalColor.rgb = waterColor;
                finalColor.a = lerp(0.6, 0.9, sceneDepth * waterDepth);
                // finalColor.a = 1;

                return finalColor;
                // return float4(waterDepth.xxx, 1);

                // // float3 viewVector = normalize(_WorldSpaceCameraPos - i.worldPos);
                // // return float4(viewVector,1);


                // return float4(sceneDepth.xxx, 1);

            }
            ENDCG
        }

    }
    FallBack "Diffuse"
}
