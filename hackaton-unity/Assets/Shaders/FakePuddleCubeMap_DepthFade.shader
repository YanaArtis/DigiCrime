Shader "Custom/FakePuddleCubemap_DepthFade"
{
    Properties
    {
        _Cubemap("Reflection Cubemap", CUBE) = "" {}
        _Color("Tint Color", Color) = (1,1,1,1)
        _Smoothness("Smoothness", Range(0,1)) = 0.8
        _FresnelPower("Fresnel Power", Range(0.1, 8)) = 3
        _DepthFadeDistance("Depth Fade Distance", Range(0.01, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 worldNormal  : TEXCOORD0;
                float3 worldPos     : TEXCOORD1;
                float2 uv           : TEXCOORD2;
                float4 screenPos    : TEXCOORD3;
            };

            samplerCUBE _Cubemap;
            float4 _Color;
            float _Smoothness;
            float _FresnelPower;
            float _DepthFadeDistance;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                OUT.worldPos = worldPos;
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
                float3 normal = normalize(IN.worldNormal);

                // Fresnel term
                float fresnel = pow(1.0 - saturate(dot(viewDir, normal)), _FresnelPower);

                // Reflection
                float3 reflDir = reflect(-viewDir, normal);
                float3 reflection = SAMPLE_TEXTURECUBE(_Cubemap, sampler_Cubemap, reflDir).rgb;

                // Depth fade
                float sceneDepth = LinearEyeDepth(SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, IN.screenPos.xy / IN.screenPos.w, 0).r, _ZBufferParams);
                float pixelDepth = LinearEyeDepth(IN.screenPos.z / IN.screenPos.w, _ZBufferParams);
                float depthDiff = saturate((sceneDepth - pixelDepth) / _DepthFadeDistance);
                float fade = 1.0 - depthDiff; // 1 = полностью видно, 0 = исчезает

                // Combine
                float3 finalColor = lerp(_Color.rgb, reflection, fresnel);
                float alpha = fresnel * fade;

                return float4(finalColor, alpha);
            }

            ENDHLSL
        }
    }

    FallBack "Unlit/Transparent"
}
