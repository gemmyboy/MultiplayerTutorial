//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "GenNoiseTurbulence"
{
	Properties
	{
		_planetRadius("planetRadius", Float) = 16.0
		_noiseOffset("noiseOffset", Vector) = (0.0, 0.0, 0.0)
		_frequency("frequency", Range(0.0, 1.0)) = 0.0
		_offset("offset", Range(0.0, 1.0)) = 1.0
		_amp("amp", Float) = 0.5
		_contribution("contribution", Float) = 1.0

		_permTexture("permTexture", 2D) = "white" {}
		_simplexTexture("simplexTexture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			Blend One One

			CGPROGRAM

				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				//#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				uniform float _planetRadius;
				uniform float3 _noiseOffset;
				uniform float _frequency;
				uniform float _offset;
				uniform float _amp;
				uniform float _contribution;
				uniform sampler2D _permTexture;
				uniform sampler2D _simplexTexture;

				#include "noise.cginc"

				//
				// vertex
				//

				struct vtx_out
				{
					float4 position : POSITION;
					float2 uv : TEXCOORD0;
					float3 vol : TEXCOORD1;
				};

				vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0, float3 vol : TEXCOORD1)
				{
					vtx_out OUT;
					OUT.position = position;
					OUT.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, uv).xy;
					OUT.vol = float3(vol.x, vol.y, vol.z);
					return OUT;
				}

				//
				// fragment
				//

				float4 frag(vtx_out i) : COLOR
				{
					// generate turbulence noise
					float a = snoise(normalize(i.vol) * _frequency + _noiseOffset) + _offset;	// spherical noise sampling without radius

					// do the accumulation with the previous fixed-point height
					a = (a*_contribution) * _amp;

					// return the heightmap
					// r = normalmap dx (not filled yet -- will be filled by the normal generator)
					// g = normalmap dy (not filled yet -- will be filled by the normal generator)
					// b = slope (not filled yet -- will be filled by the normal generator)
					// a = height
					return float4(0, 0, 0, a);
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
