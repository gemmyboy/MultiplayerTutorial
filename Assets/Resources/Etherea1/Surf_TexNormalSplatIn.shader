//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "Surf_TexNormalSplatIn"
{
	Properties
	{
		_hmap ("hmap", 2D) = "black" {}
		_hmapmul ("hmapmul", Float) = 1.0
		_rndmap ("rndmap", 2D) = "black" {}
		_frame ("frame", Float) = 0.0

		// ---

		_planetRadius ("planetRadius", Float) = 16.0
		_temperature ("temperature", Float) = 0.0
		_blendLut ("blendLut", 2D) = "black" {}

		_waterLevel ("waterLevel", Float) = 0.0
		_waterTex ("waterTex", 2D) = "black" {}
		_waterNormalTex ("waterNormalTex", 2D) = "black" {}
		_tilingWater ("tilingWater", Float) = 1.0
		_tilingWaterNormal ("tilingWaterNormal", Float) = 1.0
		_colorWater ("colorWater", Color) = ( 1, 1, 1, 1 )
		_waterSpecular ("waterSpecular", Float) = 1.0
		_waterSpecularPower ("waterSpecularPower", Float) = 16.0
		_waterDensity ("waterDensity", Float) = 0.1

		_snowTex ("snowTex", 2D) = "black" {}
		_tilingSnow ("tilingSnow", Float) = 1.0
		_colorSnow ("colorSnow", Color) = ( 1, 1, 1, 1 )

		_diffuseBase ("diffuseBase", 2D) = "black" {}
		_tilingBase ("tilingBase", Float) = 1.0
		_colorBase ("colorBase", Color) = ( 1, 1, 1, 1 )

		_diffuse1 ("diffuse1", 2D) = "black" {}
		_tiling1 ("tiling1", Float) = 1.0
		_colorDiffuse1 ("colorDiffuse1", Color) = ( 1, 1, 1, 1 )

		_diffuse2 ("diffuse2", 2D) = "black" {}
		_tiling2 ("tiling2", Float) = 1.0
		_colorDiffuse2 ("colorDiffuse2", Color) = ( 1, 1, 1, 1 )

		_diffuse3 ("diffuse3", 2D) = "black" {}
		_tiling3 ("tiling3", Float) = 1.0
		_colorDiffuse3 ("colorDiffuse3", Color) = ( 1, 1, 1, 1 )

		_diffuse4 ("diffuse4", 2D) = "black" {}
		_tiling4 ("tiling4", Float) = 1.0
		_colorDiffuse4 ("colorDiffuse4", Color) = ( 1, 1, 1, 1 )

		// ---

		_sunDir ("sunDir", Vector) = (0, 1, 0)
		_camPos ("camPos", Vector) = (0, 0, 0)				// The camera's current position

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
		CGPROGRAM

		#pragma target 3.0
		#pragma profileoption MaxLocalParams=64
		#pragma surface surf SimpleSpecular vertex:vert nolightmap nodirlightmap

		uniform sampler2D _hmap;
		uniform float _hmapmul;
		uniform sampler2D _rndmap;
		uniform float _frame;
		uniform float _planetRadius;
		uniform float _temperature;

		// 256x256 lut with 4 channels, one diffuse blending weight per channel
		uniform sampler2D _blendLut;

		//
		// textures blend
		//

		uniform float _waterLevel;
		uniform sampler2D _waterTex;		// every planet has a water texture, that may be modulated or not according to the height and temperature
		uniform sampler2D _waterNormalTex;
		uniform float _tilingWater;
		uniform float _tilingWaterNormal;
		uniform float4 _colorWater;
		uniform float _waterSpecular;
		uniform float _waterSpecularPower;
		uniform float _waterDensity;

		uniform sampler2D _snowTex;			// every planet has a snow texture, that may be modulated or not according to the height and temperature
		uniform float _tilingSnow;
		uniform float3 _colorSnow;

		uniform sampler2D _diffuseBase;	// the complementar base texture (when no other texture fits, will complete with this one)
		uniform float _tilingBase;
		uniform float3 _colorBase;

		uniform sampler2D _diffuse1;		// lut-based texture
		uniform float _tiling1;
		uniform float3 _colorDiffuse1;

		uniform sampler2D _diffuse2;		// lut-based texture
		uniform float _tiling2;
		uniform float3 _colorDiffuse2;

		uniform sampler2D _diffuse3;		// lut-based texture
		uniform float _tiling3;
		uniform float3 _colorDiffuse3;

		uniform sampler2D _diffuse4;		// lut-based texture
		uniform float _tiling4;
		uniform float3 _colorDiffuse4;

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

		struct Input
		{
			float4 tangent;							// vertex tangent (x,y,z,w = tangent)
			float4 texcoords;						// (x,y = heightmap uv coords), (z = vertex slope), (w = vertex pos x)
			float4 facecoords;						// (x,y = face uv coords), (z,w = vertex pos y,z)
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);

			float3 normal = normalize(v.vertex.xyz);

#if 1
			// scale down distant vertices, so very far vertices will look flat
			float height = length(v.vertex.xyz) - _planetRadius;
			float dist = length(v.vertex.xyz - _camPos);
			float scale = 1 - clamp(dist / (_planetRadius*.20), .2, 1.);
			v.vertex.xyz = normal * (_planetRadius + height * scale);
#endif

			o.tangent = v.tangent;
			o.texcoords.xy = v.texcoord.xy;
			o.facecoords.xy = v.texcoord1.xy;

			o.texcoords.z = v.color.x;		// vertex slope

			o.texcoords.w = v.vertex.x;
			o.facecoords.zw = v.vertex.yz;
		}

		// ---

		#include "intersect.cginc"

		half4 LightingSimpleSpecular(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			half3 h = normalize (lightDir + viewDir);

			half diff = max (0, dot (s.Normal, lightDir));

			float nh = max (0, dot (s.Normal, h));
			float spec = s.Specular * pow (nh, s.Gloss);

			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten * 2);
			c.a = s.Alpha;
			return c;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
#if 0
			o.Albedo = float3(0,0,0);
			o.Normal = float3(0,0,1);
#else
			float4 map = tex2D(_hmap, IN.texcoords.xy);
			float camHeight = length(_camPos) - _planetRadius;
			float camHeightFactor = clamp(camHeight / (_atmosRadius - _planetRadius)*3., 0, 1);
			float3 pixelPos = float3(IN.texcoords.w, IN.facecoords.z, IN.facecoords.w);
			float pixelDist2Cam = length(_camPos - pixelPos);

			// pseudo-random height variation
			float var = 0; //(tex2D(_rndmap, IN.facecoords.xy * 0.1).a - 0.5) * map.b;

			float3 exploded = normalize(pixelPos) * _planetRadius;
			float snowWeight = max(0.0, abs(exploded.y)/_planetRadius - 0.5) * 4.0 - _temperature*4.0 + map.a;
			snowWeight = clamp(snowWeight, 0.0, 1.0);

			float3 eyeDir = normalize(pixelPos - _camPos);
			float3 planet2CamRay = normalize(_camPos);

			// height variation is now summed to the height
			var = clamp(map.a + var, 0.0, 1.0);

			// weights of the four blending textures
			// use the random height variation to sample weights
			float4 weights = tex2D(_blendLut, float2((IN.texcoords.z*.4+map.b*.6)*2., var)) * (1.0 - snowWeight);

			// weights for the base texture is the remaining non-used weight until 1.0
			float baseWeight = max(0.0, 1.0 - (snowWeight + weights.r + weights.g + weights.b + weights.a));

			float tileFactor = clamp(pixelDist2Cam / 50, 0, 1);

			float3 diffuse = (
							tex2D(_diffuse1, IN.facecoords.xy * _tiling1) * _colorDiffuse1 * weights.x +
							tex2D(_diffuse2, IN.facecoords.xy * _tiling2) * _colorDiffuse2 * weights.y +
							tex2D(_diffuse3, IN.facecoords.xy * _tiling3) * _colorDiffuse3 * weights.z +
							tex2D(_diffuse4, IN.facecoords.xy * _tiling4) * _colorDiffuse4 * weights.w +
							tex2D(_snowTex, IN.facecoords.xy * _tilingSnow) * _colorSnow * snowWeight +
							tex2D(_diffuseBase, IN.facecoords.xy * _tilingBase) * _colorBase * baseWeight ).rgb;

			float3 diffuse2 = (
							tex2D(_diffuse1, IN.facecoords.xy * _tiling1*.1) * _colorDiffuse1 * weights.x +
							tex2D(_diffuse2, IN.facecoords.xy * _tiling2*.1) * _colorDiffuse2 * weights.y +
							tex2D(_diffuse3, IN.facecoords.xy * _tiling3*.1) * _colorDiffuse3 * weights.z +
							tex2D(_diffuse4, IN.facecoords.xy * _tiling4*.1) * _colorDiffuse4 * weights.w +
							tex2D(_snowTex, IN.facecoords.xy * _tilingSnow*.1) * _colorSnow * snowWeight +
							tex2D(_diffuseBase, IN.facecoords.xy * _tilingBase*.1) * _colorBase * baseWeight ).rgb;

			// land color
			diffuse = lerp(diffuse, diffuse2, clamp(tileFactor, .25, .75));

			// ---

			float3 binormal = cross(normalize(pixelPos), IN.tangent.xyz) * IN.tangent.w;

			float3 pixelNormal = cross(IN.tangent.xyz, binormal.xyz);
			float3 dx = float3(map.r, map.r, map.r) * IN.tangent.xyz * _hmapmul;
			float3 dy = float3(map.g, map.g, map.g) * binormal.xyz * _hmapmul;
			float3 landNormal = normalize(pixelNormal + dx + dy);
			float3x3 w2t = float3x3(IN.tangent.xyz, binormal.xyz, landNormal);
			landNormal = normalize(mul(w2t, landNormal));

			float pixelHeight = lerp(length(pixelPos) - _planetRadius, map.a, camHeightFactor);

			float dst0, dst1;
			float3 hitpoint, waterNormal;
			intersectWater(pixelPos, -eyeDir, dst0, dst1, hitpoint, waterNormal);

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

			if (camHeight >= _waterLevel)
			{
				if (pixelHeight <= _waterLevel)
				{
					//
					// underwater land viewed from above the water level
					//

					float fade = clamp(pow(pixelDist2Cam,1.5) * _waterDensity, _colorWater.a, 1.);
					o.Albedo = lerp(diffuse.rgb, waterColor, fade);
					o.Normal = waterNormal;
					o.Specular = _waterSpecular;
					o.Gloss = _waterSpecularPower;
					pixelDist2Cam = length(_camPos - hitpoint);
				}
				else
				{
					//
					// above water level land viewed from above the water level
					//

					o.Albedo = diffuse.rgb;
					o.Normal = landNormal;
					o.Specular = 0.;
					o.Gloss = 0.;
				}
			}
			else
			{
				if (pixelHeight <= _waterLevel)
				{
					//
					// underwater land viewed from underwater
					//

					float fade = clamp(pow(pixelDist2Cam,1.5) * _waterDensity, _colorWater.a, 1.);
					o.Albedo = lerp(diffuse.rgb, _colorWater.rgb, fade);
					o.Normal = lerp(landNormal, float3(0,0,1), fade);
					o.Specular = 0.;
					o.Gloss = 0.;
				}
				else
				{
					//
					// above water level land viewed from underwater
					//

					float fade = clamp(pow(pixelDist2Cam,1.5) * _waterDensity, _colorWater.a, 1.);
					o.Albedo = lerp(diffuse.rgb, waterColor, fade);
					o.Normal = waterNormal;
					o.Specular = 0.;
					o.Gloss = 0.;
				}
			}

			//
			// atmospheric scattering
			//

			// determine 'time' of day and 'time' at the place we're looking at
			float camTime = 2. * dot(_sunDir, normalize(pixelPos));						// 12:00h == 1
			float eyeTime = clamp(dot(planet2CamRay, eyeDir), 0., 1.);					// looking up == 1
			float atmosCutOff = clamp(camTime + 1., 0., 1.);

			camTime = clamp(camTime, 0., 1.);

			// determine if the horizon line has day or afternoon color
			float3 horizonColor = lerp(_atmosSetHorizonColor, _atmosDayHorizonColor, camTime);

			// interpolate atmosphere gradient from top of the sky to the horizon
			float3 sglow = (_sunGlowAmount * _sunGlowColor * pow(clamp(dot(_sunDir, eyeDir), 0., 1.), _sunGlowPower));
			float atmosFactor = clamp(1.-exp(-pow(_horizonDensity*pixelDist2Cam,2.)), 0., _maxAtmosFactor) * atmosCutOff;
			float3 atmosColor = lerp(_atmosMainColor, horizonColor, pow(1-eyeTime, _horizonPower)) * atmosFactor + sglow;

			o.Normal = lerp(o.Normal, float3(0,0,1), atmosFactor);

			// ---

			// composite final color
			o.Albedo = lerp(o.Albedo, atmosColor, atmosFactor);

#endif
		}

		ENDCG

	}

	FallBack "Diffuse"
}
