Shader "Custom/PesawatShaderSimpleURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Base Texture", 2D) = "white" {}
        [HDR]_EmissionColor ("Emission Color", Color) = (1,0.5,0.5,1)
        _ShootGlowIntensity ("Shoot Glow", Range(0,5)) = 0
    }

    SubShader
    {
        Tags { 
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float2 uv          : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EmissionColor;
                float  _ShootGlowIntensity;
            CBUFFER_END

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);

                half NdotL = max(0, dot(normalize(i.normalWS), lightDir));

                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BaseColor.rgb;
                half3 diffuse = albedo * NdotL * mainLight.color;

                half3 emission = _EmissionColor.rgb * _ShootGlowIntensity;

                return half4(diffuse + emission, 1);
            }
            ENDHLSL
        }
    }
}
