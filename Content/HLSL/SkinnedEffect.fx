#include "Common.fxh"

#define SKINNED_EFFECT_MAX_BONES   64
float4x3 Bones[SKINNED_EFFECT_MAX_BONES];

void Skin(inout CommonVSInput vin, float4 Indices, float4 Weights, uniform int boneCount)
{
	float4x3 skinning = 0;
	[unroll]
	for (int i = 0; i < boneCount; i++)
	{
		skinning += Bones[Indices[i]] * Weights[i];
	}

	vin.Position.xyz = mul(vin.Position, skinning);
	vin.normal = mul(vin.normal, (float3x3)skinning);
	vin.tangent = mul(vin.tangent, (float3x3)skinning);
}

CommonVSOut SkinnedVS(CommonVSInput input, float4 Indices : BLENDINDICES0, float4 Weights : BLENDWEIGHT0)
{
	Skin(input, Indices, Weights, 4);
	return Transform(input);
}

CommonVSOut DepthTransform(CommonVSInput vin, float4 Indices : BLENDINDICES0, float4 Weights : BLENDWEIGHT0) {
	Skin(vin, Indices, Weights, 4);
	CommonVSOut Out = (CommonVSOut)0;
	float4 worldPosition = CommonVS((CommonVSInput)vin, (CommonVSOut)Out);
	return Out;
}
float4 Depth(CommonVSOut vsout) : COLOR0 {
	return float4(1 - vsout.Pos2DAsSeenByLight.z / vsout.Pos2DAsSeenByLight.w, 0, 0, 1);
}
technique Standard
{
	pass P0
	{
		vertexShader = compile vs_4_0 SkinnedVS();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
}
technique ShadowMap
{
    pass P0
    {
        vertexShader = compile vs_4_0 DepthTransform();
        pixelShader = compile ps_4_0 Depth();
    }
}
