#include "Common.fxh"
#define SKINNED_EFFECT_MAX_BONES   72



float4x3 Bones[SKINNED_EFFECT_MAX_BONES];


struct VSInputNmTxWeights
{
	float4 Position : SV_Position;
	float2 TexCoord : TEXCOORD0;
	float3 Normal   : NORMAL;
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

	vin.Position.xyz = mul(vin.Position, skinning);
	vin.Normal = mul(vin.Normal, (float3x3)skinning);
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
float4 CustomPSNoFog(CommonVSOut pin) : SV_Target0
{
	DoClip(pin);
	float3 lightEffect = PSCalculateLight(pin);
	float4 output = tex2D(customTexture, pin.textureCoordinate);
	output.rgb *= lightEffect;
	//if(glowEnabled) { output.rgb = output.rgb * .25f + glowColor * .75f; }
	return output;
}
Technique SkinnedEffect
{
	Pass
	{
		vertexShader = compile vs_4_0 CustomVL4();
		pixelShader  = compile ps_4_0 CustomPSNoFog();
	}
}
