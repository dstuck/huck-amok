Shader "HuckAmok/SlimeRecolor"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ColorCount ("Color Count", Float) = 1
        _Colors ("Slot Colors", Color) = (0.2, 0.8, 0.2, 1)
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
        Blend SrcAlpha OneMinusSrcAlpha

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _ColorCount;
            fixed4 _Colors[3];

            // Sampled from sprSlimeIdle.png body tones.
            static const float3 SrcHighlight = float3(0.58, 0.88, 0.12);
            static const float3 SrcBody = float3(0.18, 0.62, 0.11);
            static const float3 SrcShadow = float3(0.04, 0.24, 0.04);
            static const float3 ToneRatioHi = SrcHighlight / SrcBody;
            static const float3 ToneRatioLo = SrcShadow / SrcBody;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            fixed3 PickSlotColor(float bandT)
            {
                int count = max(1, (int)round(_ColorCount));
                if (count <= 1)
                    return _Colors[0].rgb;

                int band = min((int)floor(bandT * count), count - 1);
                return _Colors[band].rgb;
            }

            int ClassifySourceTone(float3 rgb)
            {
                float3 dHi = rgb - SrcHighlight;
                float3 dMid = rgb - SrcBody;
                float3 dLo = rgb - SrcShadow;

                float distHi = dot(dHi, dHi);
                float distMid = dot(dMid, dMid);
                float distLo = dot(dLo, dLo);

                if (distHi <= distMid && distHi <= distLo)
                    return 0;

                if (distLo <= distMid)
                    return 2;

                return 1;
            }

            fixed3 BuildTargetTone(fixed3 targetColor, int tone)
            {
                if (tone == 0)
                    return saturate(targetColor * ToneRatioHi);

                if (tone == 2)
                    return targetColor * ToneRatioLo;

                return targetColor;
            }

            fixed4 SampleRecolored(fixed4 texColor, float bandT)
            {
                if (texColor.a <= 0.001)
                    return fixed4(0, 0, 0, 0);

                float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                float strongestNonGreen = max(texColor.r, texColor.b);
                float greenDominance = texColor.g - strongestNonGreen;

                if (luminance < 0.06 || greenDominance < 0.03)
                    return texColor;

                fixed3 targetColor = PickSlotColor(bandT);
                int tone = ClassifySourceTone(texColor.rgb);
                fixed3 recolored = BuildTargetTone(targetColor, tone);

                return fixed4(recolored, texColor.a);
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, input.texcoord);
                float bandT = saturate(input.texcoord.y);
                fixed4 recolored = SampleRecolored(texColor, bandT);
                recolored *= input.color;
                return recolored;
            }
            ENDCG
        }
    }
}
