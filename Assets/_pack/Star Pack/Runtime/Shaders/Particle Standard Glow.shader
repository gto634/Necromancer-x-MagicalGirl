
Shader "Andtech/Star Pack/Particle Standard Glow" {

	Properties{
		// Main Options
		[HDR] [Gamma] _Color("_Color", Color) = (1.0, 0.0, 0.0, 1.0)
		[NoScaleOffset]
		_MainTex("Glow Mask", 2D) = "white" {}
		_SoftParticlesNearFadeDistance("_SoftParticlesNearFadeDistance", Float) = 0.0
		_SoftParticlesFarFadeDistance("_SoftParticlesFarFadeDistance", Float) = 1.0
		_CameraNearFadeDistance("_CameraNearFadeDistance", Float) = 1.0
		_CameraFarFadeDistance("_CameraFarFadeDistance", Float) = 2.0
		_Brightness("Brightness", Range(0.0, 1.0)) = 1.0
		// Twinkle Options
		_TwinkleAmount("Twinkle Amount", Range(0.0, 1.0)) = 0.8
		_TwinkleSpeed("Twinkle Speed", Range(1.0, 10.0)) = 5.0
		// Internal
		[HideInInspector]
		[Toggle] _SoftParticlesEnabled("_SoftParticlesEnabled", Float) = 0.0
		[HideInInspector]
		_SoftParticleFadeParams("_SoftParticleFadeParams", Vector) = (0.0, 0.0, 0.0, 0.0)
		[HideInInspector]
		[Toggle] _CameraFadingEnabled("_CameraFadingEnabled", Float) = 0.0
		[HideInInspector]
		_CameraFadeParams("_CameraFadeParams", Vector) = (0.0, 0.0, 0.0, 0.0)
		[HideInInspector]
		[Toggle] _TwinkleEnabled("_TwinkleEnabled", Float) = 0.0
	}

	SubShader {
		Blend SrcAlpha One
		Cull Off
		ZWrite Off
		ColorMask RGB

		Tags {
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderPipeline" = "UniversalPipeline"
			"PreviewType" = "Plane"
		}

		LOD 200

		Pass {
			Name "StarGlow"
			Tags { "LightMode" = "UniversalForward" }
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#pragma shader_feature_local _FADING_ON
			#pragma shader_feature_local _TWINKLE_ON

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

			struct appdata {
				float4 vertex : POSITION;
				half4 color : COLOR;
				float3 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half4 color : COLOR;
				float fogFactor : TEXCOORD0;
				float eyeDepth : TEXCOORD1;
				float2 uv : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			CBUFFER_START(UnityPerMaterial)
				half4 _Color;
				half _Brightness;
				half _TwinkleAmount;
				half _TwinkleSpeed;
				float _SoftParticlesEnabled;
				float _CameraFadingEnabled;
				float4 _SoftParticleFadeParams;
				float4 _CameraFadeParams;
			CBUFFER_END

			v2f vert(appdata v) {
				v2f o = (v2f)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				VertexPositionInputs position = GetVertexPositionInputs(v.vertex.xyz);
				o.vertex = position.positionCS;
				o.eyeDepth = -position.positionVS.z;
				o.color = v.color * _Color;
				o.uv = v.uv.xy;
#if _TWINKLE_ON
				o.color.a *= 1.0 + _TwinkleAmount * cos((_TwinkleSpeed * (_Time[1] + v.uv.z)) * 6.28);
#endif
				o.fogFactor = ComputeFogFactor(o.vertex.z);
				return o;
			}

			half4 frag(v2f i) : SV_Target{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				half4 mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				half4 col = mask.r * i.color + mask.g * _Brightness;
				col.a = i.color.a * mask.a;
#if defined(_FADING_ON)
				if (_CameraFadingEnabled > 0.5)
					col.a *= saturate((i.eyeDepth - _CameraFadeParams.x) * _CameraFadeParams.y);

				if (_SoftParticlesEnabled > 0.5)
				{
					float rawDepth = SampleSceneDepth(GetNormalizedScreenSpaceUV(i.vertex));
					float sceneZ = unity_OrthoParams.w > 0.5
						? LinearDepthToEyeDepth(rawDepth) : LinearEyeDepth(rawDepth, _ZBufferParams);
					col.a *= saturate((sceneZ - i.eyeDepth - _SoftParticleFadeParams.x) * _SoftParticleFadeParams.y);
				}
#endif

				col.rgb = MixFogColor(col.rgb, half3(0.0, 0.0, 0.0), i.fogFactor);

				return col;
			}
			ENDHLSL
		}
	}

	Fallback Off
	CustomEditor "Andtech.StarPack.Editor.ParticleStandardGlowShaderGUI"
}
