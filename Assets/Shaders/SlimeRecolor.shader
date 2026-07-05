Shader "HuckAmok/SlimeRecolor"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ColorA ("Primary Color", Color) = (0.2, 0.8, 0.2, 1)
        _ColorB ("Secondary Color", Color) = (0.2, 0.8, 0.2, 1)
        _UseGradient ("Use Gradient", Float) = 0
        _GradientAxis ("Gradient Axis", Vector) = (0, 1, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _ColorA;
            fixed4 _ColorB;
            float _UseGradient;
            float4 _GradientAxis;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
                output.color = input.color * _Color;
                output.worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
                return output;
            }

            fixed4 SampleRecolored(fixed4 texColor, float gradientT)
            {
                if (texColor.a <= 0.001)
                    return fixed4(0, 0, 0, 0);

                float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                float strongestNonGreen = max(texColor.r, texColor.b);
                float greenDominance = texColor.g - strongestNonGreen;

                // Preserve outlines, transparent matte pixels, and any non-green detail.
                if (luminance < 0.08 || greenDominance < 0.04)
                    return texColor;

                float greenMask = saturate(greenDominance * 2.5);
                float shade = saturate(luminance * 0.75 + greenMask * 0.25);

                fixed3 targetColor = lerp(_ColorA.rgb, _ColorB.rgb, gradientT);
                fixed3 highlight = targetColor * 1.25;
                fixed3 shadow = targetColor * 0.55;
                fixed3 recolored = lerp(shadow, highlight, shade);

                return fixed4(recolored, texColor.a);
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, input.texcoord);
                float gradientT = 0;

                if (_UseGradient > 0.5)
                {
                    float3 axis = normalize(_GradientAxis.xyz + float3(0.0001, 0.0001, 0.0001));
                    gradientT = saturate(dot(input.worldPos, axis) * 4.0 + 0.5);
                }

                fixed4 recolored = SampleRecolored(texColor, gradientT);
                recolored *= input.color;
                recolored.rgb *= recolored.a;
                return recolored;
            }
            ENDCG
        }
    }
}
