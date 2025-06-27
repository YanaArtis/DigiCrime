Shader "Custom/FakePuddleCubemap"
{
    Properties
    {
        _Cubemap ("Reflection Cubemap", CUBE) = "" {}
        _NormalMap ("Distortion Normal Map", 2D) = "bump" {}
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.1
        _Alpha ("Alpha", Range(0, 1)) = 0.6
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            samplerCUBE _Cubemap;
            sampler2D _NormalMap;
            float _DistortionStrength;
            float _Alpha;
            float _FresnelPower;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.positionHCS = TransformWorldToHClip(positionWS.xyz);
                OUT.worldPos = positionWS.xyz;
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Камера в мире
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);

                // Получаем нормаль с текстуры (нормаль должна быть tangent-space)
                float3 normalTS = UnpackNormal(tex2D(_NormalMap, IN.uv));
                float3 distortedNormal = normalize(IN.worldNormal + normalTS * _DistortionStrength);

                // Вычисляем отражённый вектор
                float3 refl = reflect(-viewDir, distortedNormal);

                // Берем из кубмапы отражение
                float4 reflection = texCUBE(_Cubemap, refl);

                // Fresnel (чем ближе к краю — тем ярче)
                float fresnel = pow(1.0 - saturate(dot(viewDir, distortedNormal)), _FresnelPower);

                // Итог
                float alpha = _Alpha * fresnel;
                return float4(reflection.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
