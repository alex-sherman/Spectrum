#include "Common.fxh"
#define SKINNED_EFFECT_MAX_BONES   64



float4x3 Bones[SKINNED_EFFECT_MAX_BONES];


struct VSInputNmTxWeights
{
	CommonVSInput common;
	float4 Indices  : BLENDINDICES0;
	float4 Weights  : BLENDWEIGHT0;
};

void Skin(inout VSInputNmTxWeights vin, uniform int boneCount)
{
	float4x3 skinning = 0;
	[unroll]
	for (int i = 0; i < boneCount; i++)
	{
		skinning += Bones[vin.Indices[i]] * vin.Weights[i];
	}

	vin.common.Position.xyz = mul(vin.common.Position, skinning);
	vin.common.normal = mul(vin.common.normal, (float3x3)skinning);
	vin.common.tangent = mul(vin.common.tangent, (float3x3)skinning);
}
// Vertex shader: vertex lighting, four bones.
CommonVSOut CustomVL4(VSInputNmTxWeights vin)
{
	Skin(vin, 4);
	CommonVSOut output = (CommonVSOut)0;
	CommonVS((CommonVSInput)vin,output);
	//output.PositionPS = vin.Position;
	return output;
}
// Pixel shader: vertex lighting, no fog.
CommonPSOut CustomPSNoFog(CommonVSOut pin)
{
	DoClip(pin);
	float4 output = tex2D(customTexture, pin.textureCoordinate);
	output = PSLighting(output, pin);
	//if(glowEnabled) { output.rgb = output.rgb * .25f + glowColor * .75f; }
	return PSReturn(output, pin);
}
Technique SkinnedEffect
{
	Pass
	{
		vertexShader = compile vs_4_0 CustomVL4();
		pixelShader  = compile ps_4_0 CustomPSNoFog();
	}
}
