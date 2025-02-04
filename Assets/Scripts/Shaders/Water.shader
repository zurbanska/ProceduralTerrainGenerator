Shader "Custom/Water"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.0, 0.5, 1.0, 1.0)
        _DepthStart ("Depth Start", Float) = 5.0
        _DepthEnd ("Depth End", Float) = 20.0
        _WaterLevel ("Water Level", float) = 10

        _SkyColor ("Sky Color", Color) = (1.0, 1.0, 1.0, 1.0)

        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _TextureDistort("Texture Distort", range(0,1)) = 0.1
        _TextureScale("Texture Scale", float) = 1.5

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
            float _DepthStart;
            float _DepthEnd;
            float _WaterLevel;

            float4 _SkyColor;

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float _TextureDistort;
            float _TextureScale;

            float _WaveFrequency;
            float _WaveSpeed;
            float _WaveStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos); // for terrain depth lookup
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyzw;

                // wave movement
                o.pos.y += sin(_Time.z * _WaveSpeed + (o.pos.x * o.pos.z * _WaveFrequency)) * _WaveStrength;

                float dx = cos(_Time.z * _WaveSpeed + (o.pos.x * o.pos.z * _WaveFrequency)) * _WaveStrength;
                float dz = -sin(_Time.z * _WaveSpeed + (o.pos.x * o.pos.z * _WaveFrequency)) * _WaveStrength;

                float3 vnormal = normalize(v.normal);
                float3 normal = float3(vnormal.x - vnormal.y * dz, vnormal.y, vnormal.z - vnormal.y * dx);

                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.normal = normal;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {

                // water texture
                float distort = tex2D(_NoiseTex, (i.worldPos.xz * _TextureScale) + (_Time.x * 2)).r;
                float4 waterTex = tex2D(_MainTex, (i.worldPos.xz * _TextureScale) - (distort * _TextureDistort)) * i.normal.y;

                // distance from camera to terrain under water
                float terrainDepth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos))));

                float waterDepth = terrainDepth - _WaterLevel;

                float depthFactor = saturate((waterDepth - _DepthStart) / (_DepthEnd - _DepthStart));

                float4 skyColor = _SkyColor;

                float3 normal = _WorldSpaceLightPos0.xyz * i.normal * 0.4;
                float3 baseColor = _BaseColor.rgb + normal.xxx + normal.zzz + normal.yyy;

                float4 finalColor = 1;
                finalColor.rgb = baseColor * skyColor;
                finalColor.rgb += waterTex * 0.1;

                finalColor.a = lerp(0.7, 1, depthFactor);

                return finalColor;

            }
            ENDCG
        }

    }
    FallBack "Diffuse"
}
