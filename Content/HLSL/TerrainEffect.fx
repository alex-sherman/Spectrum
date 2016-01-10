#include "Common.fxh"

uniform extern texture MultiTextureA;
uniform extern texture MultiTextureB;
uniform extern texture MultiTextureC;
uniform extern texture MultiTextureD;
uniform extern float VertexBlend = 0;

sampler sand = sampler_state
{
	Texture = <MultiTextureA>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = POINT;
	AddressU = wrap;
	AddressV = wrap;
};
sampler grass = sampler_state
{
	Texture = <MultiTextureB>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = POINT;
	AddressU = wrap;
	AddressV = wrap;
};
sampler rock = sampler_state
{
	Texture = <MultiTextureC>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = POINT;
	AddressU = wrap;
	AddressV = wrap;
};
sampler snow = sampler_state
{
	Texture = <MultiTextureD>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = POINT;
	AddressU = wrap;
	AddressV = wrap;
};
//----------------------------------------------------MultiTex-----------------------------------
struct MultiTex_VS_OUT
{
	float4 position  : SV_Position;
	float3 worldPosition : POSITION1;
	float2 textureCoordinate : TEXCOORD0;
	float4 Pos2DAsSeenByLight : TEXCOORD1;
	float clipDistance : TEXCOORD2;
	float depth : TEXCOORD3;
	float fog	: COLOR0;
	float light : COLOR1;
	float4 blend : TEXCOORD4;
	float4 depthBlend : TEXCOORD5;
};
struct MultiTex_VS_IN
{
	float4 Position  : SV_Position;
	float2 TextureCoordinate : TEXCOORD0;
	float3 normal : NORMAL;
	float4 blend : TEXCOORD1;
    float4 Position2 : TEXCOORD2;
	float4 blend2 : TEXCOORD3;
};
MultiTex_VS_OUT TransformMulti(MultiTex_VS_IN vin)
{
	MultiTex_VS_OUT Out = (MultiTex_VS_OUT)0;
    vin.Position = lerp(vin.Position, vin.Position2, VertexBlend);
	float4 worldPosition = 	CommonVS((CommonVSInput)vin, (CommonVSOut)Out);
	Out.blend = lerp(vin.blend, vin.blend2, VertexBlend);
	float blendDistance1 = 300;
	Out.depthBlend.x = clamp((Out.depth)/blendDistance1, 0, 1);
	return Out;
}
CommonPSOut ApplyMultiTexture(MultiTex_VS_OUT vsout, SamplerState samp, int blendIndex)
{
	DoClip((CommonVSOut)vsout);
	float texw = vsout.blend.x;
	float lodw = vsout.depthBlend.x;
	float2 coorda = vsout.textureCoordinate*2;
	float2 coordb = vsout.textureCoordinate/4;
	float3 sampled = 0;
	//sampled += lerp(tex2D(snow,coorda).rgb,tex2D(snow,coordb),lodw) * vsout.blend.w;
	//sampled += lerp(tex2D(rock,coorda).rgb,tex2D(rock,coordb),lodw) * vsout.blend.z;
	//sampled += lerp(tex2D(sand, coorda).rgb, tex2D(sand, coordb), lodw) * vsout.blend.y;
	sampled += tex2D(samp, coorda) * vsout.blend[blendIndex];
	float4 toReturn = (float4)0;
	toReturn.rgb = sampled;

	toReturn.rgb = lerp(toReturn.rgb, (float4)0,vsout.fog);
	toReturn.a = 1-vsout.fog;
	if(!aboveWater){
		toReturn.b+=.1f;
	}
	return PSReturn(toReturn, (CommonVSOut)vsout);
}
CommonPSOut ApplyMultiTextureSand(MultiTex_VS_OUT vsout)
{
	return ApplyMultiTexture(vsout, grass, 1);
}
technique MultiTexture
{
	pass P0
	{
		vertexShader = compile vs_4_0 TransformMulti();
		pixelShader = compile ps_4_0 ApplyMultiTextureSand();
	}
}
