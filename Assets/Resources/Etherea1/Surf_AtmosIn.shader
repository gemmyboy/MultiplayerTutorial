//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//
// this file is based on sean oneil's atmospheric scattering algorithms

Shader "Surf_AtmosIn"
{
	Properties
	{
		_frame ("frame", Float) = 0.0

		_camPos ("Camera Position", Vector) = (0,0,0)
		_sunDir ("Sun Direction", Vector) = (0,0,0)
		_camHeight ("Camera Height", Float) = 0

		// atmospheric scattering
		_atmosRadius ("Atmosphere Radius", Float) = 17
		_atmosMainColor ("Main Atmosphere Color", Color) = (.2, .4, .8, 1.)
		_atmosDayHorizonColor ("Main Atmosphere Color", Color) = (.8, .9, 1., 1.)
		_atmosSetHorizonColor ("Main Atmosphere Color", Color) = (.2, .4, .8, 1.)
		_horizonPower ("Horizon Power", Float) = 4.
		_horizonDensity ("Horizon Density", Float) = .05
		_maxAtmosFactor ("Max Atmosphere Factor", Float) = .75
		_sunGlowColor ("Sun Glow Color", Color) = (1, 1, .9, 1)
		_sunGlowAmount ("Sun Glow Amount", Float) = 1.5
		_sunGlowPower ("Sun Glow Power", Float) = 12.

		_planetRadius ("planetRadius", Float) = 16.0

		_waterLevel ("waterLevel", Float) = 0.0
		_waterTex ("waterTex", 2D) = "black" {}
		_waterNormalTex ("waterNormalTex", 2D) = "black" {}
		_tilingWater ("tilingWater", Float) = 1.0
		_tilingWaterNormal ("tilingWaterNormal", Float) = 1.0
		_colorWater ("colorWater", Color) = ( 1, 1, 1, 1 )
		_waterSpecular ("waterSpecular", Float) = 1.0
		_waterSpecularPower ("waterSpecularPower", Float) = 16.0
		_waterDensity ("waterDensity", Float) = 0.1
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" }

		Cull Off
		Blend One One
		ZWrite Off

		CGPROGRAM

		#pragma target 3.0
		#pragma surface surf Lambert vertex:vert nolightmap nodirlightmap

		uniform float _frame;
		uniform float3 _sunDir;
		uniform float3 _camPos;

		uniform float _atmosRadius;
		uniform float3 _atmosMainColor;
		uniform float3 _atmosDayHorizonColor;
		uniform float3 _atmosSetHorizonColor;
		uniform float _horizonPower;
		uniform float _horizonDensity;
		uniform float _maxAtmosFactor;
		uniform float3 _sunGlowColor;
		uniform float _sunGlowAmount;
		uniform float _sunGlowPower;

		uniform float _planetRadius;

		uniform float _waterLevel;
		uniform sampler2D _waterTex;
		uniform sampler2D _waterNormalTex;
		uniform float _tilingWater;
		uniform float _tilingWaterNormal;
		uniform float4 _colorWater;
		uniform float _waterSpecular;
		uniform float _waterSpecularPower;
		uniform float _waterDensity;

		struct Input
		{
			float4 tangent;
			float3 normal;
			float3 pixelPos;
		};

		#include "intersect.cginc"

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);

			o.pixelPos = v.vertex.xyz * _atmosRadius;
			o.normal = v.normal;

			o.tangent = v.tangent;
			o.pixelPos = v.vertex.xyz * _atmosRadius;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
#if 0
			o.Albedo = float3(0,0,0);
			o.Normal = float3(0,0,1);
#else
			float3 eyeDir = normalize(IN.pixelPos - _camPos);
			float3 planet2CamRay = normalize(_camPos);
			float rimDot = clamp(dot(planet2CamRay, IN.normal), 0, 1);
			float camHeight = length(_camPos) - _planetRadius;

			// determine 'time' of day and 'time' at the place we're looking at
			float camTime = 2. * dot(_sunDir, planet2CamRay);							// 12:00h == 1
			float eyeTime = clamp(dot(planet2CamRay, eyeDir), 0., 1.);					// looking up == 1
			float atmosFactor = 1 - clamp(camHeight / (_atmosRadius - _planetRadius), 0., 1.);
			float atmosCutOff = clamp(camTime + 1., 0., 1.);

			camTime = clamp(camTime, 0., 1.);

			// determine if the horizon line has day or afternoon color
			float3 horizonColor = lerp(_atmosSetHorizonColor, _atmosDayHorizonColor, camTime);

			// interpolate atmosphere gradient from top of the sky to the horizon
			float3 sglow = (_sunGlowAmount * _sunGlowColor * pow(clamp(dot(_sunDir, eyeDir), 0., 1.), _sunGlowPower));
			o.Albedo.rgb = lerp(_atmosMainColor, horizonColor, pow(1-eyeTime, _horizonPower)) * atmosFactor + sglow;

			float3 binormal = cross(normalize(IN.pixelPos), IN.tangent.xyz) * IN.tangent.w;
			float pixelDist2Cam = length(_camPos - IN.pixelPos);

			float dst0, dst1;
			float3 hitpoint, waterNormal;
			intersectWater(IN.pixelPos, eyeDir, dst0, dst1, hitpoint, waterNormal);

			float2 wateruv;
			wateruv.x = .5 + atan2(waterNormal.z, waterNormal.x) / (2. * 3.1415927);
			wateruv.y = .5 + asin(waterNormal.y) / 3.1415927;

			// first water normal perturbation
			float3 upWater = float3(0.,0.,0.);
			float3 wn = UnpackNormal(tex2D(_waterNormalTex, wateruv * _tilingWaterNormal - _frame));
			float3 wdx = wn.r * IN.tangent;
			float3 wdy = wn.g * binormal;
			upWater += wdx;
			upWater += wdy;

			// second water normal perturbation
			#define wnrm2scale 1.
			float3 wn2 = UnpackNormal(tex2D(_waterNormalTex, wateruv * _tilingWaterNormal*wnrm2scale + _frame*wnrm2scale));
			float3 wdx2 = wn2.r * IN.tangent;
			float3 wdy2 = wn2.g * binormal;
			upWater += wdx2;
			upWater += wdy2;

			// add both perturbations
			waterNormal = normalize(waterNormal + upWater);

			float3x3 ww2t = float3x3(IN.tangent.xyz, binormal.xyz, waterNormal);
			waterNormal = normalize(mul(ww2t, waterNormal));

			float3 waterColor = tex2D(_waterTex, wateruv * _tilingWater).rgb + _colorWater.rgb;

			if (camHeight <= _waterLevel)
			{
				//
				// sky viewed from underwater
				//

				float fade = clamp(pow(pixelDist2Cam,1.5) * _waterDensity, _colorWater.a, 1.);
				o.Albedo = lerp(o.Albedo.rgb, waterColor, fade) * atmosCutOff;
				o.Normal = waterNormal;
				o.Specular = _waterSpecular;
				o.Gloss = _waterSpecularPower;
			}
			else
			{
				//
				// sky viewed from above water level
				//

				o.Normal = float3(0,0,1);
			}
#endif
		}

		ENDCG

	} 

	FallBack "None"
}
