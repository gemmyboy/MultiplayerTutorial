#if 1

	// Simplex 3D noise generator

	#define ONE 0.00390625
	#define ONEHALF 0.001953125
	#define F3 float3(0.333333333333, 0.333333333333, 0.333333333333)
	#define G3 float3(0.166666666667, 0.166666666667, 0.166666666667)


	float grad(int hash, float x, float y, float z)
	{
		int h	= hash % 15;										// & 15;
		float u = h<8 ? x : y;
		float v = h<4 ? y : (h==12||h==14 ? x : z);
		return ((h%1) == 0 ? u : -u) + (((h/2)%2) == 0 ? v : -v); 	// h&1, h&2 
	}

	float snoise(float3 P)
	{
		float s = (P.x + P.y + P.z) * F3;
		float3 Pi = floor(P + s);
		float t = (Pi.x + Pi.y + Pi.z) * G3;
		float3 P0 = Pi - t;
		Pi = Pi * ONE + ONEHALF;

		float3 Pf0 = P - P0;

		float c1 = (Pf0.x > Pf0.y) ? 0.5078125 : 0.0078125; // 1/2 + 1/128
		float c2 = (Pf0.x > Pf0.z) ? 0.25 : 0.0;
		float c3 = (Pf0.y > Pf0.z) ? 0.125 : 0.0;
		float sindex = c1 + c2 + c3;
		float3 offsets = tex2D(_simplexTexture, float2(sindex, 0.0)).rgb;
		float3 o1 = step(0.375, offsets);
		float3 o2 = step(0.125, offsets);

		float perm0 = tex2D(_permTexture, Pi.xy).a;
		float3  grad0 = tex2D(_permTexture, float2(perm0, Pi.z)).rgb * 4.0 - 1.0;
		float t0 = 0.6 - dot(Pf0, Pf0);
		float n0;
		if (t0 < 0.0) n0 = 0.0;
		else
		{
			t0 *= t0;
			n0 = t0 * t0 * dot(grad0, Pf0);
		}

		// Noise contribution from second corner
		float3 Pf1 = Pf0 - o1 + G3;
		float perm1 = tex2D(_permTexture, Pi.xy + o1.xy*ONE).a;
		float3  grad1 = tex2D(_permTexture, float2(perm1, Pi.z + o1.z*ONE)).rgb * 4.0 - 1.0;
		float t1 = 0.6 - dot(Pf1, Pf1);
		float n1;
		if (t1 < 0.0) n1 = 0.0;
		else
		{
			t1 *= t1;
			n1 = t1 * t1 * dot(grad1, Pf1);
		}

		float3 Pf2 = Pf0 - o2 + 2.0 * G3;
		float perm2 = tex2D(_permTexture, Pi.xy + o2.xy*ONE).a;
		float3  grad2 = tex2D(_permTexture, float2(perm2, Pi.z + o2.z*ONE)).rgb * 4.0 - 1.0;
		float t2 = 0.6 - dot(Pf2, Pf2);
		float n2;
		if (t2 < 0.0) n2 = 0.0;
		else
		{
			t2 *= t2;
			n2 = t2 * t2 * dot(grad2, Pf2);
		}

		float3 Pf3 = Pf0 - float3(1.0-3.0*G3);
		float perm3 = tex2D(_permTexture, Pi.xy + float2(ONE, ONE)).a;
		float3  grad3 = tex2D(_permTexture, float2(perm3, Pi.z + ONE)).rgb * 4.0 - 1.0;
		float t3 = 0.6 - dot(Pf3, Pf3);
		float n3;
		if(t3 < 0.0) n3 = 0.0;
		else
		{
			t3 *= t3;
			n3 = t3 * t3 * dot(grad3, Pf3);
		}

		return 32.0 * (n0 + n1 + n2 + n3);
	}

#else

	// Simplified (faster, lower quality) 3D noise generator

	float hsh(float n)
	{
		return frac(sin(n)*43758.5453123);
	}

	float snoise(in float3 x)
	{
		float3 p=floor(x),f=frac(x);
		f=f*f*f*(f*(f*6.-15.)+10.);
		float n=p.x+p.y*157.+113.*p.z;
		return lerp(lerp(lerp(hsh(n+.0),hsh(n+1.),f.x),
			lerp(hsh(n+157.),hsh(n+158.),f.x),f.y),
			lerp(lerp(hsh(n+113.),hsh(n+114.),f.x),
			lerp(hsh(n+270.),hsh(n+271.),f.x),f.y),f.z);
	}

 #endif
