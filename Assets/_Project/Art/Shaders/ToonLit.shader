// 적, 보스, 배경, 엄폐물의 툰 셰이더(SPEC 1.1절). 피격 플래시(SPEC 4절)도 여기서 덮는다.
// 음영
// - 주 조명 하나로 밝은 면과 그림자 면 2단만 나눈다. 그림자 면은 몸 색에 _ShadeColor 를 곱한다.
// - _ShadeThreshold: 경계 위치(빛과 면의 각도 코사인). 올리면 그림자 면이 넓어진다.
// - _ShadeSoftness: 경계 번짐 폭. 0에 가까울수록 칼로 자른 듯한 셀 경계다.
// - 받는 그림자도 같은 경계로 잘라 2단을 넘지 않게 한다.
// 외곽선
// - 뒷면만 그리는 패스를 화면 픽셀 단위(_OutlineWidth)로 부풀린다. 거리와 상관없이 굵기가 같아 먼 적도 실루엣이 읽힌다.
// - _OutlineFromCenter: 0이면 법선 방향, 1이면 물체 중심에서 바깥 방향으로 부풀린다.
//   상자처럼 면마다 법선이 갈라진 도형은 법선 방향이면 모서리가 끊겨서 1을 쓴다. 생성 메시는 0.
// 발광
// - _EmissionMap(마스크) x _EmissionColor 를 음영과 상관없이 더한다. 약점 코어처럼 밝기를 코드에서 바꿀 곳에 쓴다.
// 피격 플래시
// - _BaseColor: 몸 색. 상태효과 색(기절, 화상, 감속)은 EnemyUnit 이 섞어서 MaterialPropertyBlock 으로 넣는다.
// - _FlashAmount: 0~1 번쩍임 진행. 맞는 순간 1이 되고 줄어든다.
// - _FlashBlend: 번쩍일 때 음영 입힌 색을 _FlashColor 쪽으로 얼마나 덮는지. 1이면 1단계의 흰색 번쩍임과 같다.
// - _RimStrength, _RimPower: 번쩍일 때 윤곽(시선과 비스듬한 면)을 더 밝힌다.
Shader "AimCombat/ToonLit"
{
    Properties
    {
        [Header(Base)]
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _BaseMap("Base Map", 2D) = "white" {}

        [Header(Shade)]
        _ShadeColor("Shade Color", Color) = (0.46, 0.42, 0.62, 1)
        _ShadeThreshold("Shade Threshold", Range(-1, 1)) = 0.1
        _ShadeSoftness("Shade Softness", Range(0.001, 0.5)) = 0.02

        [Header(Outline)]
        _OutlineColor("Outline Color", Color) = (0.04, 0.03, 0.07, 1)
        _OutlineWidth("Outline Width (px)", Range(0, 8)) = 2
        _OutlineFromCenter("Outline From Center", Range(0, 1)) = 0

        [Header(Emission)]
        _EmissionMap("Emission Mask", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)

        [Header(Hit Flash)]
        _FlashColor("Flash Color", Color) = (1, 1, 1, 1)
        _FlashAmount("Flash Amount", Range(0, 1)) = 0
        _FlashBlend("Flash Blend", Range(0, 1)) = 1
        _RimStrength("Rim Strength", Range(0, 3)) = 0
        _RimPower("Rim Power", Range(0.5, 8)) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        // 모든 패스가 같은 머티리얼 버퍼를 써야 SRP Batcher 로 묶인다.
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _EmissionMap_ST;
            half4 _BaseColor;
            half4 _ShadeColor;
            half _ShadeThreshold;
            half _ShadeSoftness;
            half4 _OutlineColor;
            half _OutlineWidth;
            half _OutlineFromCenter;
            half4 _EmissionColor;
            half4 _FlashColor;
            half _FlashAmount;
            half _FlashBlend;
            half _RimStrength;
            half _RimPower;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_EmissionMap);
        SAMPLER(sampler_EmissionMap);
        ENDHLSL

        Pass
        {
            Name "ToonForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(input.uv, _BaseMap)).rgb;

                float3 normal = normalize(input.normalWS);
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));

                // 빛을 받는 각도와 받는 그림자를 같은 경계로 잘라 2단으로 만든다.
                half ndotl = dot(normal, mainLight.direction);
                half lit = smoothstep(_ShadeThreshold - _ShadeSoftness, _ShadeThreshold + _ShadeSoftness, ndotl);
                half shadow = smoothstep(0.5h - _ShadeSoftness, 0.5h + _ShadeSoftness, mainLight.shadowAttenuation);
                lit = min(lit, shadow);
                half3 color = albedo * lerp(_ShadeColor.rgb, mainLight.color, lit);

                color += _EmissionColor.rgb * SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, TRANSFORM_TEX(input.uv, _EmissionMap)).rgb;

                // 피격 플래시는 음영 위에 덮는다. 맞은 순간에는 그림자 면도 똑같이 번쩍인다.
                half amount = saturate(_FlashAmount);
                color = lerp(color, _FlashColor.rgb, amount * _FlashBlend);
                float3 view = normalize(GetWorldSpaceViewDir(input.positionWS));
                half rim = pow(1.0h - saturate(dot(normal, view)), _RimPower);
                color += _FlashColor.rgb * rim * _RimStrength * amount;
                return half4(color, 1.0h);
            }
            ENDHLSL
        }

        // 외곽선. 뒷면만 그리고 화면 픽셀 단위로 부풀린다.
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            float4 vert(Attributes input) : SV_POSITION
            {
                float3 fromCenter = SafeNormalize(input.positionOS.xyz);
                float3 directionOS = SafeNormalize(lerp(input.normalOS, fromCenter, _OutlineFromCenter));
                float3 directionWS = TransformObjectToWorldDir(directionOS);
                float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);

                // 방향을 화면에 투영해 픽셀 단위로 민다. 정면을 향한 방향은 화면에서 길이가 0이라 밀지 않는다.
                float2 directionCS = mul((float3x3)GetWorldToHClipMatrix(), directionWS).xy;
                float lengthCS = length(directionCS);
                directionCS = lengthCS > 1e-5 ? directionCS / lengthCS : float2(0, 0);
                positionCS.xy += directionCS * (_OutlineWidth * 2.0 / _ScreenParams.xy) * positionCS.w;
                return positionCS;
            }

            half4 frag() : SV_Target
            {
                return half4(_OutlineColor.rgb, 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            float4 vert(Attributes input) : SV_POSITION
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                return positionCS;
            }

            half4 frag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // 깊이 텍스처가 필요한 효과가 켜져도 빠지지 않게 깊이만 쓰는 패스.
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 vert(float4 positionOS : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(positionOS.xyz);
            }

            half frag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // PC 렌더러의 화면 공간 차폐(SSAO)가 쓰는 깊이와 법선 패스.
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
            #if defined(_GBUFFER_NORMALS_OCT)
                float3 normalWS = normalize(input.normalWS);
                float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
                float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
                return half4(PackFloat2To888(remappedOctNormalWS), 0.0);
            #else
                return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            #endif
            }
            ENDHLSL
        }
    }
}
