Shader "Custom/Multi Light Cel Shader"
{
    Properties
    {
        [MainTexture][NoScaleOffset] _BaseMap ("Base Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0.35, 0.35, 0.4, 1)

        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.45
        _ShadowSmoothness ("Shadow Smoothness", Range(0.001, 0.5)) = 0.04

        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.25
        _LightIntensity ("Overall Light Intensity", Range(0, 4)) = 1.0
        _DirectionalLightIntensity ("Directional Light Intensity", Range(0, 4)) = 1.0
        _AdditionalLightIntensity ("Additional Light Intensity", Range(0, 4)) = 1.0
        _LightFalloff ("Light Falloff", Range(0.1,8)) = 2

        _RimColor ("Rim Color", Color) = (0.6, 0.8, 1.0, 1)
        _RimSize ("Rim Size", Range(0.01, 10)) = 2
        _RimLightingCutoff ("Rim Lighting Cutoff", Range(0, 1)) = 0.4
        _RimFalloff ("Rim Falloff Amount", Range(0.001, 1)) = 0.1
        _RimIntensity ("Rim Intensity", Range(0, 4)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirectionWS : TEXCOORD2;
                float2 baseUV : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _ShadowColor;

                float _ShadowThreshold;
                float _ShadowSmoothness;

                float _AmbientStrength;
                float _LightIntensity;
                float _DirectionalLightIntensity;
                float _AdditionalLightIntensity;
                float _LightFalloff;

                float4 _RimColor;
                float _RimSize;
                float _RimLightingCutoff;
                float _RimFalloff;
                float _RimIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalize(normalInputs.normalWS);
                output.viewDirectionWS = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                output.baseUV = input.uv;

                return output;
            }

            float GetCelBand(float ndotl)
            {
                ndotl = saturate(ndotl);

                return smoothstep(
                    _ShadowThreshold - _ShadowSmoothness,
                    _ShadowThreshold + _ShadowSmoothness,
                    ndotl
                );
            }

            float3 CalculateCelLight(float3 normalWS, Light light)
            {
                float ndotl = dot(normalWS, normalize(light.direction));
                float band = GetCelBand(ndotl);

                float attenuation = pow(saturate(light.distanceAttenuation), _LightFalloff);
                attenuation *= light.shadowAttenuation;

                return light.color * band * attenuation;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirectionWS);

                float2 baseUV = saturate(input.baseUV);
                float4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV);
                float3 baseColor = _BaseColor.rgb * baseTex.rgb;

                float3 totalLight = 0;

                Light mainLight = GetMainLight();
                totalLight += CalculateCelLight(normalWS, mainLight) * _DirectionalLightIntensity;

                float fresnelPower = 1.0 / max(_RimSize, 0.001);
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), fresnelPower);

                float mainNdotL = saturate(dot(normalWS, normalize(mainLight.direction)));

                // Lit-side rim
                float lightMask = mainNdotL;

                // Shadow-side rim alternative:
                // float lightMask = 1.0 - mainNdotL;

                fresnel *= lightMask;

                float rim = smoothstep(
                    _RimLightingCutoff,
                    _RimLightingCutoff + _RimFalloff,
                    fresnel
                );

                rim *= _RimIntensity;

                #if defined(_ADDITIONAL_LIGHTS)

                    InputData inputData = (InputData)0;
                    inputData.positionWS = input.positionWS;
                    inputData.normalWS = normalWS;
                    inputData.viewDirectionWS = viewDirWS;
                    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                    #if USE_CLUSTER_LIGHT_LOOP
                        UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                        {
                            Light light = GetAdditionalLight(lightIndex, input.positionWS, half4(1,1,1,1));
                            totalLight += CalculateCelLight(normalWS, light) * _AdditionalLightIntensity;
                        }
                    #endif

                    uint additionalLightCount = GetAdditionalLightsCount();

                    LIGHT_LOOP_BEGIN(additionalLightCount)
                        Light light = GetAdditionalLight(lightIndex, input.positionWS, half4(1,1,1,1));
                        totalLight += CalculateCelLight(normalWS, light) * _AdditionalLightIntensity;
                    LIGHT_LOOP_END

                #endif

                totalLight *= _LightIntensity;

                float lightAmount = saturate(length(totalLight));
                lightAmount = max(lightAmount, _AmbientStrength);

                float3 finalColor = lerp(
                    baseColor * _ShadowColor.rgb,
                    baseColor * max(totalLight, _AmbientStrength),
                    lightAmount
                );

                finalColor += _RimColor.rgb * rim;

                return float4(finalColor, _BaseColor.a * baseTex.a);
            }

            ENDHLSL
        }
    }
}