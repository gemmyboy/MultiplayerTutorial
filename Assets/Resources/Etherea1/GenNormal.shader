//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "GenNormal"
{
	Properties
	{
		_uvStep("uvStep", Float) = 1.0
		_hmap("hmap", 2D) = "black" {}
		_heightscale("heightscale", Float) = 1.0
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM

				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				//#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				uniform float _uvStep;
				uniform sampler2D _hmap;
				uniform float _heightscale;

				//
				// vertex
				//

				struct vtx_out
				{
					float4 position : POSITION;
					float2 uv : TEXCOORD0;
				};

				vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0)
				{
					vtx_out OUT;
					OUT.position = position;
					OUT.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, uv).xy;
					return OUT;
				}

				//
				// fragment
				//

				float2 sobel(float2 uv)
				{
					float u = uv.x;
					float v = uv.y;

					float tl = tex2D(_hmap, float2(u - _uvStep, v - _uvStep)).a * _heightscale;
					float l = tex2D(_hmap, float2(u - _uvStep, v)).a * _heightscale;
					float bl = tex2D(_hmap, float2(u - _uvStep, v + _uvStep)).a * _heightscale;
					float b = tex2D(_hmap, float2(u, v + _uvStep)).a * _heightscale;
					float br = tex2D(_hmap, float2(u + _uvStep, v + _uvStep)).a * _heightscale;
					float r = tex2D(_hmap, float2(u + _uvStep, v)).a * _heightscale;
					float tr = tex2D(_hmap, float2(u + _uvStep, v - _uvStep)).a * _heightscale;
					float t = tex2D(_hmap, float2(u, v - _uvStep)).a * _heightscale;

					float dX = tr + 2.0 * r + br - tl - 2.0 * l - bl;
					float dY = bl + 2.0 * b + br - tl - 2.0 * t - tr;

					return float2(dX, dY);
				}

				float4 frag(vtx_out i) : COLOR
				{
					float4 prev = tex2D(_hmap, i.uv);

					float2 d = sobel(i.uv);
					float3 n = normalize(float3(-d.x, -d.y, 2.));
					float slope = max(abs(n.x), abs(n.y));

					// return the packed heightmap with normals
					// r = normalmap dx
					// g = normalmap dy
					// b = slope
					// a = height
					return float4(n.x, n.y, slope, prev.a);
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
