#define kMaxSteps 200

static const float kGradientResolution = 128;
static const float kGradientTexelSize = (1.0f/kGradientResolution);
static const float kGradientHalfTexelSize = (0.5f/kGradientResolution);

SamplerState PointClampedSampler
{
	Filter = MIN_MAG_MIP_POINT;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};
	
SamplerState BilinearClampedSampler
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};

SamplerState BilinearWrappedSampler
{
	Filter = MIN_MAG_MIP_POINT;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
};

struct VS_INPUT 
{
	float4					vertex		: POSITION;
};

struct PS_INPUT  
{
	float4					pos			: SV_POSITION;
	noperspective float2	uv			: TEXCOORD0;
	noperspective float3	viewPosTS	: TEXCOORD1;
	noperspective float3	viewDirTS	: TEXCOORD2;
};


Texture2D _HeatHazeOffsetTex;

Texture2D _GrabTexture;

Texture2D _CameraDepthTexture;
float4 _CameraDepthTexture_TexelSize;

Texture3D<float4> _Volume;  
SamplerState sampler_Volume;

Texture2D<float4> _GradientTex;

Texture2D _Output;
	
Texture2D _RenderMaskTex;

Texture3D<float> _OpacityMap;
SamplerState sampler_OpacityMap;

Texture3D<float> _NoiseVolume;
SamplerState sampler_NoiseVolume;

cbuffer StaticParams : register( b8 )
{
	float _RenderQuality;
	float _IsPerspectiveProjection;
	float _IsDeferred;
};

cbuffer ViewParams : register( b9 )
{
	float3 _EyePositionTS;
	float3 _EyeDirectionTS;
	float4x4 _TextureToView;
	float4x4 _WorldToTexture;
};

cbuffer DynamicParams : register( b10 )
{
	float _EdgeFadePower;
	float _Intensity;
	float _SaneMaxSteps;
	float _Opacity;

	float _NoiseFrequency;
	float _NoiseAmplitude;
	float3 _NoiseAnimation;
};

cbuffer HeatHaze : register( b11 )
{
	float _HeatHazeDiffusionTaps;
	float _RecipHeatHazeDiffusionTaps;
	float _HeatHazeSpread;
	float _HeatHazeStrength;
};

inline float2 ProjectionToTexcoord (const float4 pos) 
{
	float2 uv = 0.5f * ( pos.xy/pos.w ) + 0.5f;

	[flatten]
	if ( _IsDeferred > 0.5f )
	{
		uv.y = 1-uv.y;
	}

	return uv;
}

#ifndef COMPUTE_SHADER_INCLUDE
PS_INPUT RenderVS(VS_INPUT v) 
{
	PS_INPUT o;
		
	o.pos = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 1));
	o.uv = ProjectionToTexcoord( o.pos );

	float3 posWS = mul( _Object2World, v.vertex ).xyz;

	[flatten]
	if( _IsPerspectiveProjection > 0.5f )
	{
		float3 viewDirWS = posWS - _WorldSpaceCameraPos.xyz;
		float viewZ = dot( viewDirWS, -UNITY_MATRIX_V[2].xyz );

		o.viewPosTS = _EyePositionTS;
		o.viewDirTS = mul( _WorldToTexture, float4(viewDirWS/viewZ, 0)).xyz;
	}
	else
	{
		float viewZ = mul( UNITY_MATRIX_V, float4(posWS, 1) ).z;
		float3 camPosWS = posWS - viewZ * UNITY_MATRIX_V[2].xyz;

		o.viewPosTS = mul( _WorldToTexture, float4(camPosWS.xyz, 1)).xyz;
		o.viewDirTS = _EyeDirectionTS;
	}

	return o;
}

float GetDepth( float2 uv )
{
	return LinearEyeDepth(_CameraDepthTexture.SampleLevel( PointClampedSampler, uv, 0 ).r) ;
}

#endif

inline float4 Blend(float4 src, float4 dst)
{
	float4 col = 0..xxxx;

#ifndef FLUIDITY_ADDITIVE_BLEND
	const float t = mad( dst.a, -src.a, dst.a );
	
	col.rgb = mad( dst.rgb, t, src.rgb );
	col.a	= src.a + t;
#else
	col.rgb = mad( dst.rgb, dst.a, src.rgb );
	col.a	= mad( (1-src.a), dst.a, src.a );
#endif

	return col;
}

float4 ColourAtPos( float3 uvw, uint numChannels )
{
#	if !defined( FLUIDITY_DISABLE_NOISE_PETURBATION ) && !defined( COMPUTE_SHADER_INCLUDE )
	const float offset = _NoiseAmplitude * abs(_NoiseVolume.SampleLevel(sampler_NoiseVolume, uvw*_NoiseFrequency + _NoiseAnimation, 0 ));
#	else
	const float offset = 1;
#	endif

	const float4 reactionCoord = _Volume.SampleLevel(sampler_Volume, uvw, 0) * offset.xxxx;
	const float4 rCoord = saturate(reactionCoord * _Intensity) * (1 - kGradientTexelSize) + kGradientHalfTexelSize;
	float4 col = 0..xxxx;

	if( numChannels == 1 )
	{
		col = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.x, 0.5f), 0);
	}
	else if( numChannels == 2 )
	{
		col = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.x, 0.25f), 0);
		const float4 col1 = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.y, 0.75f), 0);
		col = Blend(col, col1);
	}
	else if( numChannels == 3 )
	{
		col = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.x, 0.1666f), 0);
		const float4 col1 = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.y, 0.5f), 0);
		col = Blend(col, col1);
		const float4 col2 = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.z, 0.7333f), 0);
		col = Blend(col, col2);
	}
	else
	{
		col = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.x, 0.125f), 0);
		const float4 col1 = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.y, 0.365f), 0);
		col = Blend(col, col1);
		const float4 col2 = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.z, 0.625f), 0);
		col = Blend(col, col2);
		const float4 col3 = _GradientTex.SampleLevel(PointClampedSampler, float2(rCoord.w, 0.865f), 0);
		col = Blend(col, col3);
	}

	return col;
}
	
float SelfShadowing( float3 uvw, uint numChannels )
{
	return _OpacityMap.SampleLevel( sampler_OpacityMap, uvw, 0);
}

void IntersectBox(float3 ro, float3 rd, out float tnear, out float tfar)
{
	const float3 boxmin = 0..xxx;
	const float3 boxmax = 1..xxx;

	// compute intersection of ray with all six bboxplanes
	const float3 invR = 1.0f  / rd;
	const float3 tbot = invR * (boxmin.xyz-ro);
	const float3 ttop = invR * (boxmax.xyz-ro);
   
	// re-order intersections to find smallest and largest on each axis
	const float3 tmin = min(ttop, tbot);
	const float3 tmax = max(ttop, tbot);
   
	// find the largest tmin and the smallest tmax
	float2 t0 = max (tmin.xx, tmin.yz);
	tnear = max(0, max (t0.x, t0.y));

	t0 = min (tmax.xx, tmax.yz);
	tfar = max(0, min (t0.x, t0.y));
}

inline float4 GetEdgeMask( float3 uvw )
{
	float4 col = 1..xxxx;

#ifndef FLUIDITY_USE_EDGE_FADE
	col = _RenderMaskTex.SampleLevel(BilinearClampedSampler, uvw.xz, 0).xxxx;
#else
	const float3 edgeFade = saturate( uvw * _EdgeFadePower * (1..xxx-uvw) * _EdgeFadePower );
	col =  (edgeFade.x * edgeFade.y * edgeFade.z).xxxx;
#endif 

	return col;
}


float4 Raymarch( const float2 uv, const float3 pos, const float3 dir, uint steps, const uint numChannels )
{
	float4 output = 0..xxxx;
	float3 uvw = pos;

	while( steps-- > 0 && output.a <= 0.99f  )
	{
		float4 col = ColourAtPos( uvw, numChannels );

#if defined(FLUIDITY_ENABLE_SELF_SHADOWING	)	
		col.rgb *= SelfShadowing( uvw, numChannels );
#endif

		col.a *= GetEdgeMask(uvw).r * _Opacity;
		output = Blend(output, col);
		
		uvw += dir;
	}	 

#if defined(FLUIDITY_ENABLE_HEAT_HAZE)	
	const float2 RAND_SAMPLES[8] = 
	{
		float2(  0.003f,  0.003f ),
		float2(  0.003f, -0.003f ),
		float2( -0.003f, -0.003f ),
		float2( -0.003f,  0.003f ),
		float2(  0.01f,  0.01f ),
		float2(  0.01f, -0.01f ),
		float2( -0.01f, -0.01f ),
		float2( -0.01f,  0.01f ),
	};

	float heatHazeAmount = output.a;
	float4 accumulation = 0..xxxx;
	for(uint i=0 ; i<_HeatHazeDiffusionTaps ; ++i)
	{
		float2 uvOffset = _HeatHazeOffsetTex.Sample(BilinearWrappedSampler, (uv + _Time.xx * 100) ).rg;
		uvOffset = uvOffset * 2 - 1..xx;
		accumulation += _GrabTexture.SampleLevel( PointClampedSampler, uv + (uvOffset * _HeatHazeSpread + RAND_SAMPLES[i]) * heatHazeAmount, 0 );
	}

	float4 haze = accumulation * _RecipHeatHazeDiffusionTaps;
	return lerp( output, haze, saturate( (1-heatHazeAmount) *  _HeatHazeStrength * haze.a ) );
#endif
	 
	return output;
}

float4 ShowRaymarchMask(float3 pos, float3 dir, uint steps)
{
	float4 output = 0..xxxx;
	float3 uvw = pos;

	for (uint i=0 ; i<steps; ++i)
	{
		uvw += dir;

		float4 col = GetEdgeMask( uvw );

		const float invAlpha = (1.0f - output.a);
		const float cAlpha = col.a * invAlpha;

		output.rgb += col.rgb * cAlpha;
		output.a += cAlpha;
	}	 

	return output;
}

inline const float GetTextureToZ01( float3 posTS )
{
	return mul( _TextureToView, float4(posTS, 1) ).z;
}

void DecodeInfo( const float3 viewPosTS, float3 viewDirTS, const float depth, out float3 near, out float3 far, out float3 direction, out uint steps )
{
	float nearD, farD;
	IntersectBox( viewPosTS, viewDirTS, nearD, farD );

	near = mad( viewDirTS, nearD, viewPosTS );
	far  = mad( viewDirTS, farD,  viewPosTS );
		
	const float frontZ01 = GetTextureToZ01( near );
	const float backZ01  = GetTextureToZ01( far  );

	const float csDepthDelta = backZ01 - frontZ01;
	const float ratioDepth =  (max(backZ01, -depth) - frontZ01) / csDepthDelta;  

	const float tsDepthDelta = abs(nearD - farD);
	const float marchLength = tsDepthDelta * ratioDepth; 

	float s = min( marchLength * _SaneMaxSteps * _RenderQuality, kMaxSteps );

	direction = ( viewDirTS * marchLength ) / s;
	steps = (uint)s + 0.5f;

	steps *= step(frontZ01, depth);
}

