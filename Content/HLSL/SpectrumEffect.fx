#include "Common.fxh"

#define SKINNED_EFFECT_MAX_BONES   64
float4x3 Bones[SKINNED_EFFECT_MAX_BONES];

CommonVSOut Transform(CommonVSInput input) {
	CommonVSOut Out = (CommonVSOut)0;
	float4 worldPosition = CommonVS((CommonVSInput)input, (CommonVSOut)Out);
	return Out;
}
CommonVSOut InstanceTransform(CommonVSInput input, float4x4 instanceWorld : POSITION1) {
	CommonVSOut Out = (CommonVSOut)0;
	CommonVSInput tInput = (CommonVSInput)input;
	//tInput.Position = mul(tInput.Position, transpose(instanceWorld));
	//tInput.normal = normalize(mul(tInput.normal, transpose(instanceWorld)));
	float4 worldPosition = CommonVS((CommonVSInput)tInput, mul(transpose(instanceWorld), world), (CommonVSOut)Out);
	Out.clipDistance = length(worldPosition - cameraPosition) < 120 ? 1 : -1;
	return Out;
}
CommonPSOut ApplyTexture(CommonVSOut vsout)
{
	if(Clip) { clip(vsout.clipDistance); }
	if(vsout.fog >=.99f){ clip(-1); }
	float4 color;
	if (UseTexture) {
		if (UseTransparency) {
			color.rgb = tex2D(customTexture, vsout.textureCoordinate).rgb;
			color.a = 1 - tex2D(transparencySampler, vsout.textureCoordinate).r;
			color.rgb *= color.a;
		}
		else
			color = tex2D(customTexture, vsout.textureCoordinate);
	}
	else {
		color = diffuseColor;
	}
	clip(color.a <= 0 ? -1:1);
	if(!aboveWater){
		color.b+=.1f;
	}
	color = PSLighting(color, vsout);
	color.a *= 1-vsout.fog;
	return PSReturn(color, vsout);
}

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

technique TextureDraw
{
	pass P0
	{
		vertexShader = compile vs_4_0 Transform();
		pixelShader  = compile ps_4_0 ApplyTexture();
	}
}
technique InstanceTextureDraw
{
	pass P0
	{
		vertexShader = compile vs_4_0 InstanceTransform();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
}
technique Skinned
{
	pass P0
	{
		vertexShader = compile vs_4_0 SkinnedVS();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
}