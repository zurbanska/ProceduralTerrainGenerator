Shader "Custom/Water"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.0, 0.5, 1.0, 1.0)
        _DepthStart ("Depth Start", Float) = 5.0
        _DepthEnd ("Depth End", Float) = 20.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

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
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            float4 _BaseColor;
            float _DepthStart;
            float _DepthEnd;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos); // For depth lookup
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float depth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos))));

                float depthFactor = saturate((depth - _DepthStart) / (_DepthEnd - _DepthStart));

                float4 finalColor = _BaseColor * depthFactor;
                finalColor.a = lerp(0.6, 1, depthFactor);
                // finalColor *= depthFactor;
                return finalColor;


            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
