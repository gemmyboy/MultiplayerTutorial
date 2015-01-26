Shader "Hidden/Fluidity/VolumeLight" 
{
	SubShader 
	{
		Pass
		{
			Tags { "Queue"="Transparent" "RenderType"="Transparent" }
			Fog { Mode off }

			ZWrite Off Fog { Mode Off }
			Blend one one

			CGPROGRAM
			#pragma vertex LightVS
			#pragma fragment LightPS

			#pragma target 5.0

			#include "UnityCG.cginc"
			#include "Fluidity.cginc"
		
			float4x4 _CameraToWorld;
			Texture2D _CameraDepthNormalsTexture;
			
			struct PS_LIGHTINPUT 
			{
				float4 pos	: SV_POSITION;
				float4 uv	: TEXCOORD0;
				float3 ray	: TEXCOORD1;
			};

			PS_LIGHTINPUT LightVS(VS_INPUT v) 
			{
				PS_LIGHTINPUT o;
		
				o.pos = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 1));
				o.uv = ComputeScreenPos (o.pos);

				o.ray = mul (UNITY_MATRIX_MV, v.vertex).xyz * float3( -1, -1, 1 );

				return o;
			}

			float4 LightPS(PS_LIGHTINPUT i) : SV_TARGET
			{
				i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
				float2 uv = i.uv.xy / i.uv.w;

				float4 depthnormal = _CameraDepthNormalsTexture.SampleLevel( PointClampedSampler, uv, 0 );

				float d;
				float3 viewNorm;
				DecodeDepthNormal (depthnormal, d, viewNorm);
				//depth = Linear01Depth( UNITY_SAMPLE_DEPTH(depth) );

				float depth = UNITY_SAMPLE_DEPTH(_CameraDepthTexture.SampleLevel(PointClampedSampler, uv, 0));
				depth = Linear01Depth (depth);

				float4 posVS = float4( i.ray * depth, 1 );
				float3 posWS = mul ( _CameraToWorld, posVS ).xyz;

				float3 posLS = ( mul( _World2Object, float4( posWS, 1 ) ).xyz);

				const float edgeFadePower = 4.0f;
				const float3 uvw = posLS*0.5f + 0.5f;
				const float3 edgeFade = saturate( uvw * edgeFadePower * (1..xxx-uvw) * edgeFadePower );
				const float fadeModifier = 1-saturate( length(posLS)* 2 );//saturate( edgeFade.x * edgeFade.y * edgeFade.z );
				
				const float4 reactionCoord = _Volume.SampleLevel(sampler_Volume, uvw, 0);
				const float4 rCoord = saturate(reactionCoord * _Intensity) * (1 - kGradientTexelSize) + kGradientHalfTexelSize;
				float4 col = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.x, 0.5f), 0) * 5;
				
				const float3 lightDir = normalize( posLS * 2 - 1 );
				float ndotl = saturate( dot(viewNorm, lightDir) ); 


				return float4(col.rgb * col.a * fadeModifier, col.a * fadeModifier );//
			}	

			ENDCG
		} 
	}
}
