Shader "Custom/Terrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (0.5, 0.5, 0.5, 1)
        _LightColor ("Light Color", Color) = (0.5, 0.5, 0.5, 1)
        _FogStart ("Fog Start Distance", Float) = 10
        _FogEnd ("Fog End Distance", Float) = 20
        _DepthLevel ("Depth Level", Range(1, 3)) = 2
        _WaterLevel ("Water Level", Float) = 10
        _LightPosX ("World Light Pos X", float) = 0
        _LightPosY ("World Light Pos Y", float) = 0
        _LightPosZ ("World Light Pos Z", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
        LOD 100

        // Main Pass
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            #include "UnityLightingCommon.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

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
                float3 color : COLOR0;
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                fixed3 diff : COLOR1;
                fixed3 ambient : COLOR2;
                float3 normal: TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _FogColor;
            float _FogStart;
            float _FogEnd;
            float _WaterLevel;
            float _LightPosX;
            float _LightPosY;
            float _LightPosZ;
            float _LightColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color = v.color;
                o.uv = v.uv;
                o.normal = v.normal;

                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
                // compute shadows data
                TRANSFER_SHADOW(o)

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {

                float3 normal = normalize(i.normal);
                // float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightDir = normalize(float3(_LightPosX, _LightPosY, _LightPosZ));

                float3 lightFallOff = max(0.3, dot(lightDir, normal));
                float3 lightColor = _LightColor0.rgb;

                float3 terrainColor = i.color * lightFallOff * lightColor;
                // float directDiffuseLight = lightColor * lightFalloff;

                float distance = length(_WorldSpaceCameraPos - i.worldPos);
                float fogFactor = saturate((distance - _FogStart) / (_FogEnd - _FogStart));

                // fixed4 col = tex2D(_MainTex, i.uv);
                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow + i.ambient;
                // col.rgb *= lighting;
                terrainColor *= lighting;

                float3 finalColor = lerp(terrainColor, _FogColor.rgb, fogFactor);

                bool isUnderwater = (_WorldSpaceCameraPos.y < _WaterLevel);

                if (isUnderwater)
                {
                    float underwaterFog = saturate(distance / (_FogEnd - _FogStart));
                    finalColor = lerp(terrainColor, _FogColor.rgb, underwaterFog);
                }

                return float4(finalColor, 1.0);
            }
            ENDCG
        }

        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

    }
    FallBack "Diffuse"
}
