Shader "Hidden/Fluidity/Render3" 
{
	Subshader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Fog { Mode off }
		
		GrabPass { }

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off ZTest Off Cull front 

		// Three channel render
		Pass 
		{
			CGPROGRAM
      
			#pragma vertex RenderVS
			#pragma fragment RenderFluid3
			#pragma multi_compile FLUIDITY_ENABLE_NOISE_PETURBATION FLUIDITY_DISABLE_NOISE_PETURBATION
			#pragma multi_compile FLUIDITY_ENABLE_SELF_SHADOWING FLUIDITY_DISABLE_SELF_SHADOWING
			#pragma multi_compile FLUIDITY_ENABLE_HEAT_HAZE FLUIDITY_DISABLE_HEAT_HAZE
			#pragma multi_compile FLUIDITY_USE_MASK FLUIDITY_USE_EDGE_FADE FLUIDITY_USE_NONE
			#pragma multi_compile FLUIDITY_ADDITIVE_BLEND FLUIDITY_ALPHA_BLEND 

			#pragma target 5.0

			#include "UnityCG.cginc"
			#include "Fluidity.cginc"

			float4 RenderFluid3(PS_INPUT i) : SV_TARGET
			{
				const float depth = GetDepth( i.uv );

				uint steps;
				float3 near, far, direction;
				DecodeInfo( i.viewPosTS, i.viewDirTS, depth, near, far, direction, steps );

				const float4 output = Raymarch(i.uv, near, direction, steps, 3);

				return output;
			}	

			ENDCG
		}
	}

	Fallback Off
}
