Shader "Custom/WavesColors"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [Smoothness] _Smoothness("Smoothness", Range(0,1)) = 0.5
        [Metallic] _Metallic("Metallic", Range(0,1)) = 0.0
        [Range(0,1)] _Transparency ("Transparency", Float) = 0.5
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        [NoScaleOffset] _FlowMap ("Flow (RG, A noise)", 2D) = "black" {}
        [NoScaleOffset] _DerivHeightMap ("Deriv (AG) Height (B)", 2D) = "black" {}
        _UJump ("U jump per phase", Range(-0.25, 0.25)) = 0.25
		_VJump ("V jump per phase", Range(-0.25, 0.25)) = 0.25
        _Tiling ("Tiling", Float) = 1
        _Speed ("Speed", Float) = 1
        _FlowStrength ("Flow Strength", Float) = 1
        _FlowOffset ("Flow Offset", Float) = 0
		_HeightScale ("Height Scale, Constant", Float) = 0.25
		_HeightScaleModulated ("Height Scale, Modulated", Float) = 0.75

        [WaveA] _WaveA ("Wave A (dir, steepness, wavelength)", Vector) = (1,0,0.5,10)
        [WaveB] _WaveB ("Wave B", Vector) = (0,1,0.25,20)
        [WaveC] _WaveC ("Wave C", Vector) = (1,1,0.15,10)
       
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
            #include "Flow.cginc"
            

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

            TEXTURE2D(_FlowMap);
            SAMPLER(sampler_FlowMap);

            TEXTURE2D(_DerivHeightMap);
            SAMPLER(sampler_DerivHeightMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _Smoothness;
                float _Metallic;
                float _Transparency;
                float4 _WaveA;
                float4 _WaveB;
                float4 _WaveC;
                float _UJump;
                float _VJump;
                float _Tiling;
                float _Speed;
                float _FlowStrength;
                float _FlowOffset;
                float _HeightScale;
                float _HeightScaleModulated;
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

            float3 UnpackDerivativeHeight (float4 textureData) {
                float3 dh = textureData.agb;
                dh.xy = dh.xy * 2 - 1;
                return dh;
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
               
                float3 flow = SAMPLE_TEXTURE2D(_FlowMap,sampler_FlowMap, IN.uv);
                flow.xy = flow.xy * 2 - 1;
			    flow *= _FlowStrength;
                float noise = SAMPLE_TEXTURE2D(_FlowMap,sampler_FlowMap, IN.uv).a;
			    float time = _Time.y * _Speed + noise;
                float2 jump = float2(_UJump, _VJump);
                float3 uvwA = FlowUVW(IN.uv, flow.xy, jump, _FlowOffset, _Tiling, time, false);
                float3 uvwB = FlowUVW(IN.uv, flow.xy, jump, _FlowOffset, _Tiling, time, true);


                float finalHeightScale =
				length(flow.z) * _HeightScaleModulated + _HeightScale;
        		float3 dhA =
				    UnpackDerivativeHeight(SAMPLE_TEXTURE2D(_DerivHeightMap, sampler_DerivHeightMap, uvwA.xy)) * (uvwA.z * finalHeightScale);
			    float3 dhB =
				    UnpackDerivativeHeight(SAMPLE_TEXTURE2D(_DerivHeightMap, sampler_DerivHeightMap, uvwB.xy)) * (uvwB.z * finalHeightScale);
			    IN.normalWS = normalize(float3(-(dhA.xy + dhB.xy), 1));

                float4 texA = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvwA.xy) * uvwA.z;
                float4 texB = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvwB.xy) * uvwB.z;

                float4 c = (texA + texB) * _BaseColor;

                // Normal e direção da luz principal
                float3 normalWS = normalize(IN.normalWS);
                Light mainLight = GetMainLight();

                float3 lightDir = normalize(-mainLight.direction);
                float NdotL = saturate(dot(normalWS, lightDir));

                // Difusa
                float3 diffuse = c.rgb * NdotL * mainLight.color;

                // Especular simples
                float3 viewDir = normalize(GetWorldSpaceViewDir(IN.positionWS));
                float3 halfDir = normalize(lightDir + viewDir);
                float spec = pow(saturate(dot(normalWS, halfDir)), lerp(1, 128, _Smoothness));

                float3 specular = spec * mainLight.color * _Metallic;

                // Combina difusa + especular + ambient
                float3 f = diffuse + specular + c.rgb * 0.1;

                return half4(f, _Transparency * _BaseColor.a);
            }

            ENDHLSL
        }
    }
}
