// 피격 플래시 셰이더(SPEC 4절). URP 무조명. 임시 도형 적과 보스가 쓴다.
// - _BaseColor: 몸 색. 상태효과 색(기절, 화상, 감속)과 보스 경고색은 스크립트가 섞어서 넣는다.
// - _FlashAmount: 0~1 번쩍임 진행. 맞는 순간 1이 되고 줄어든다(EnemyUnit 이 MaterialPropertyBlock 으로 넣는다).
// - _FlashBlend: 번쩍일 때 몸 색을 _FlashColor 쪽으로 얼마나 덮는지. 1이면 1단계의 흰색 번쩍임과 같다.
// - _RimStrength, _RimPower: 번쩍일 때 윤곽(시선과 비스듬한 면)을 더 밝힌다. 맞은 대상이 배경에서 떨어져 보인다.
Shader "AimCombat/HitFlashUnlit"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _FlashColor("Flash Color", Color) = (1, 1, 1, 1)
        _FlashAmount("Flash Amount", Range(0, 1)) = 0
        _FlashBlend("Flash Blend", Range(0, 1)) = 1
        _RimStrength("Rim Strength", Range(0, 3)) = 0
        _RimPower("Rim Power", Range(0.5, 8)) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _FlashColor;
                half _FlashAmount;
                half _FlashBlend;
                half _RimStrength;
                half _RimPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(position.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half amount = saturate(_FlashAmount);
                half3 color = lerp(_BaseColor.rgb, _FlashColor.rgb, amount * _FlashBlend);

                float3 normal = normalize(input.normalWS);
                float3 view = normalize(input.viewDirWS);
                half rim = pow(1.0h - saturate(dot(normal, view)), _RimPower);
                color += _FlashColor.rgb * rim * _RimStrength * amount;
                return half4(color, 1.0h);
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _FlashColor;
                half _FlashAmount;
                half _FlashBlend;
                half _RimStrength;
                half _RimPower;
            CBUFFER_END

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
    }
}
