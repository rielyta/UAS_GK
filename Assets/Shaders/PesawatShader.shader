Shader "Custom/PesawatShaderURP"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1.0, 0.85, 0.9, 1.0)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Float) = 1.0
        
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.7
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.1
        _MetallicGlossMap("Metallic Gloss Map", 2D) = "white" {}
        
        _OcclusionMap("Occlusion Map", 2D) = "white" {}
        _OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionMap("Emission Map", 2D) = "white" {}
        _EmissiveIntensity("Emissive Intensity", Float) = 0.0
        
        // Animation properties - untuk di-trigger dari script
        _RollIntensity("Roll Intensity", Range(0.0, 1.0)) = 0.0
        _ShootGlowIntensity("Shoot Glow Intensity", Range(0.0, 5.0)) = 0.0
        
        // Blending
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry" 
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType" = "Lit"
        }
        LOD 300

        // ===== FORWARD LIGHTING PASS =====
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma vertex vert
            #pragma fragment frag
            
            // Shader Features
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSIONMAP
            #pragma shader_feature_local _EMISSION
            
            // Multi-compile
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Textures
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            
            TEXTURE2D(_MetallicGlossMap);
            SAMPLER(sampler_MetallicGlossMap);
            
            TEXTURE2D(_OcclusionMap);
            SAMPLER(sampler_OcclusionMap);
            
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            // Properties
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _EmissionColor;
                
                float _NormalScale;
                float _Smoothness;
                float _Metallic;
                float _OcclusionStrength;
                float _EmissiveIntensity;
                
                // Animation properties - bisa di-set dari script
                float _RollIntensity;
                float _ShootGlowIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.viewDirWS = GetCameraPositionWS() - OUT.positionWS;
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                
                // ===== TEXTURE SAMPLING =====
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 normalMap = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv);
                half4 metallicMap = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, IN.uv);
                half4 occlusionMap = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, IN.uv);
                half4 emissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv);
                
                // ===== NORMAL MAPPING =====
                half3 normalTS = UnpackNormal(normalMap);
                normalTS.xy *= _NormalScale;
                
                // Konversi dari tangent space ke world space
                half3 bitangent = cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w;
                half3 normalWS = normalize(
                    normalTS.x * IN.tangentWS.xyz +
                    normalTS.y * bitangent +
                    normalTS.z * IN.normalWS
                );
                
                // ===== SURFACE PROPERTIES =====
                half3 albedo = baseMap.rgb * _BaseColor.rgb;
                half metallic = metallicMap.r * _Metallic;
                half smoothness = metallicMap.a * _Smoothness;
                half occlusion = LerpWhiteTo(occlusionMap.rgb, _OcclusionStrength);
                
                // ===== ANIMATION EFFECTS =====
                // Roll effect: semakin besar roll, semakin metallic
                metallic = lerp(metallic, 1.0, _RollIntensity * 0.5);
                
                // Emission dari shoot glow - BOOSTED untuk lebih dramatis
                half3 emission = emissionMap.rgb * _EmissionColor.rgb * (_EmissiveIntensity + _ShootGlowIntensity * 3.0);
                
                // ===== LIGHTING CALCULATION =====
                half3 viewDir = normalize(IN.viewDirWS);
                
                // Main light
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(IN.positionWS));
                half3 lightDir = normalize(mainLight.direction);
                half3 halfDir = normalize(lightDir + viewDir);
                
                // Ambient
                half3 ambient = half3(0.3, 0.3, 0.35) * albedo * occlusion;
                
                // Diffuse
                half NdotL = max(0.0, dot(normalWS, lightDir));
                half3 diffuse = albedo * mainLight.color * NdotL * mainLight.shadowAttenuation;
                
                // Specular (PBR-style dengan Fresnel)
                half NdotH = max(0.0, dot(normalWS, halfDir));
                half NdotV = max(0.0, dot(normalWS, viewDir));
                
                // Fresnel effect
                half3 F0 = lerp(half3(0.04, 0.04, 0.04), albedo, metallic);
                half fresnel = pow(1.0 - NdotV, 5.0);
                half3 F = lerp(F0, half3(1.0, 1.0, 1.0), fresnel);
                
                // Specular power dari smoothness
                half specularPower = exp2(lerp(1.0, 11.0, smoothness));
                half3 specular = F * pow(NdotH, specularPower) * mainLight.color * mainLight.shadowAttenuation;
                
                // ===== ADDITIONAL LIGHTS =====
                #if defined(_ADDITIONAL_LIGHTS)
                uint additionalLightsCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(additionalLightsCount)
                    Light light = GetAdditionalLight(lightIndex, IN.positionWS);
                    
                    half3 addLightDir = normalize(light.direction);
                    half addNdotL = max(0.0, dot(normalWS, addLightDir));
                    half3 addDiffuse = albedo * light.color * addNdotL * light.shadowAttenuation;
                    
                    half3 addHalfDir = normalize(addLightDir + viewDir);
                    half addNdotH = max(0.0, dot(normalWS, addHalfDir));
                    half3 addSpecular = F * pow(addNdotH, specularPower) * light.color * light.shadowAttenuation;
                    
                    diffuse += addDiffuse;
                    specular += addSpecular;
                LIGHT_LOOP_END
                #endif
                
                // ===== COMBINE =====
                half3 finalColor = ambient + diffuse + specular + emission;
                
                return half4(finalColor, _BaseColor.a);
            }
            ENDHLSL
        }

        // ===== SHADOW CASTER PASS =====
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionHCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));
                
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }

        // ===== DEPTH PASS =====
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
