Shader "Custom/SimpleObjects"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogStart ("Fog Start Distance", Float) = 10
        _FogEnd ("Fog End Distance", Float) = 20
        _FogColor ("Fog Color", Color) = (0,0,0,0)
        _SkyColor ("Sky Color", Color) = (0,0,0,0)
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
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _SkyColor;

            float4 _FogColor;
            float _FogStart;
            float _FogEnd;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // apply texture of object
                fixed4 textureColor = tex2D(_MainTex, i.uv);

                // light color from global main light
                float4 lightColor = _LightColor0;

                float4 skyColor = _SkyColor;

                float distance = length(_WorldSpaceCameraPos - i.worldPos);
                float fogFactor = saturate((distance - _FogStart) / (_FogEnd - _FogStart));

                float4 baseColor = lightColor * skyColor;
                baseColor *= textureColor * 1.5;
                baseColor = saturate(baseColor);
                // col *= lightColor;
                // col *= skyColor * 0.5;

                float3 finalColor = lerp(baseColor, _FogColor.rgb, fogFactor);

                return float4(finalColor, 0);
            }
            ENDCG
        }
    }
}
