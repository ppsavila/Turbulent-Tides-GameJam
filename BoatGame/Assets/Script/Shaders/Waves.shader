Shader "Custom/Waves"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [Smoothness] _Smoothness("Smoothness", Range(0,1)) = 0.5
        [Metallic] _Metallic("Metallic", Range(0,1)) = 0.0
        [WaveA] _WaveA ("Wave A (dir, steepness, wavelength)", Vector) = (1,0,0.5,10)
        [WaveB] _WaveB ("Wave B", Vector) = (0,1,0.25,20)
        [WaveC] _WaveC ("Wave C", Vector) = (1,1,0.15,10)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : NORMAL;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _Smoothness;
                float _Metallic;
                float4 _WaveA;
                float4 _WaveB;
                float4 _WaveC;
            CBUFFER_END

            float3 GerstnerWave (float4 wave, float3 p, inout float3 tangent, inout float3 binormal) 
            {
                float steepness = wave.z;
                float wavelength = wave.w;
                float k = 2 * PI / wavelength;
                float c = sqrt(9.8 / k);
                float2 d = normalize(wave.xy);
                float f = k * (dot(d, p.xz) - c * _Time.y);
                float a = steepness / k;
                
                tangent += float3(
                    -d.x * d.x * (steepness * sin(f)),
                    d.x * (steepness * cos(f)),
                    -d.x * d.y * (steepness * sin(f))
                );
                binormal += float3(
                    -d.x * d.y * (steepness * sin(f)),
                    d.y * (steepness * cos(f)),
                    -d.y * d.y * (steepness * sin(f))
                );
                
                return float3(
                    d.x * (a * cos(f)),
                    a * sin(f),
                    d.y * (a * cos(f))
                );
		    }  

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 gridPoint = TransformObjectToWorld(IN.positionOS.xyz);
                float3 tangent = float3(1, 0, 0);
                float3 binormal = float3(0, 0, 1);
                float3 p = gridPoint;
                p += GerstnerWave(_WaveA, gridPoint, tangent, binormal);
                p += GerstnerWave(_WaveB, gridPoint, tangent, binormal);
                p += GerstnerWave(_WaveC, gridPoint, tangent, binormal);
                float3 normal = normalize(cross(binormal, tangent));

                OUT.positionWS = p;
                OUT.positionHCS = TransformWorldToHClip(p);
                OUT.normalWS = normal;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Textura e cor base
                float4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                // Normal e direção da luz principal
                float3 normalWS = normalize(IN.normalWS);
                Light mainLight = GetMainLight();

                float3 lightDir = normalize(mainLight.direction);
                float NdotL = saturate(dot(normalWS, lightDir));

                // Difusa
                float3 diffuse = albedo.rgb * NdotL * mainLight.color;

                // Especular simples
                float3 viewDir = normalize(GetWorldSpaceViewDir(IN.positionWS));
                float3 halfDir = normalize(lightDir + viewDir);
                float spec = pow(saturate(dot(normalWS, halfDir)), lerp(1, 128, _Smoothness));

                float3 specular = spec * mainLight.color * _Metallic;

                // Combina difusa + especular + ambient
                float3 color = diffuse + specular + albedo.rgb * 0.1;

                return half4(color, albedo.a);
            }
            ENDHLSL
        }
    }
}
