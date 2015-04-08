//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "Surf_AtmosOut"
{
	Properties
	{
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
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" }

		Cull Off
		Blend OneMinusDstColor One
		ZWrite Off

		CGPROGRAM

		#pragma target 3.0
		#pragma surface surf Lambert vertex:vert nolightmap nodirlightmap

		uniform float3 _camPos;
		uniform float3 _sunDir;
		uniform float _camHeight;

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

		struct Input
		{
			float3 pixelPos;
			float3 normal;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);

			o.pixelPos = v.vertex.xyz * _atmosRadius;
			o.normal = v.normal;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			float3 eyeDir = normalize(IN.pixelPos - _camPos);
			float3 planet2CamRay = normalize(_camPos);
			float rimDot = clamp((_camHeight/_atmosRadius)*dot(planet2CamRay, IN.normal), 0., 1.);
			float outRim = clamp(rimDot, 0, 1);

			// determine 'time' of day and 'time' at the place we're looking at
			float camTime = 2. * dot(_sunDir, IN.normal);							// 12:00h == 1
			float eyeTime = clamp(dot(planet2CamRay, eyeDir), 0., 1.);				// looking up == 1
			float atmosCutOff = clamp(camTime + 1., 0., 1.);

			camTime = clamp(camTime, 0., 1.);

			// determine if the horizon line has day or afternoon color
			float3 horizonColor = lerp(_atmosSetHorizonColor, _atmosDayHorizonColor, camTime);

			// interpolate atmosphere gradient from top of the sky to the horizon
			o.Albedo.rgb = lerp(_atmosMainColor, horizonColor, pow(1-eyeTime, _horizonPower)) * atmosCutOff;
			o.Albedo.rgb *= clamp(pow(1.-rimDot, 2.), 0,1) * 10. * pow(rimDot,8.)*10;

			o.Normal = float3(0,0,1);
		}

		ENDCG

	}

	FallBack "None"
}
