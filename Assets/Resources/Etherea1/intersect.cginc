// everything is assumed inverse transformed
bool intersectWater(in float3 rayorigin, in float3 raydir, out float dst0, out float dst1, out float3 hitpoint, out float3 hitnormal)
{
	float3 l = -rayorigin;
	float tca = dot(l, raydir);

	float radius2 = (_planetRadius+_waterLevel)*(_planetRadius+_waterLevel);
	float d2 = dot(l, l) - tca * tca;
	float thc = sqrt(radius2 - d2);

	dst0 = tca - thc;
	dst1 = tca + thc;
	if (dst0 < 0.) dst0 = dst1;

	hitpoint = rayorigin + raydir * dst0;
	hitnormal = normalize(hitpoint);

	return true;
}
