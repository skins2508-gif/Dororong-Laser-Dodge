Shader "UI/NyanCatChromaKey"
{
    Properties
    {
        [PerRendererData] _MainTex ("Video", 2D) = "white" {}
        _Tolerance ("Background Tolerance", Range(0.01, 0.5)) = 0.16
        _Softness ("Edge Softness", Range(0.001, 0.2)) = 0.055
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Tolerance;
            float _Softness;

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 pixel = tex2D(_MainTex, input.uv) * input.color;

                // 영상 네 모서리의 평균색을 현재 프레임의 배경색으로 사용한다.
                fixed3 key = (
                    tex2D(_MainTex, float2(0.01, 0.01)).rgb +
                    tex2D(_MainTex, float2(0.99, 0.01)).rgb +
                    tex2D(_MainTex, float2(0.01, 0.99)).rgb +
                    tex2D(_MainTex, float2(0.99, 0.99)).rgb
                ) * 0.25;

                float distanceFromBackground = distance(pixel.rgb, key);
                float alpha = smoothstep(_Tolerance, _Tolerance + _Softness, distanceFromBackground);
                pixel.a *= alpha;
                return pixel;
            }
            ENDCG
        }
    }
}
