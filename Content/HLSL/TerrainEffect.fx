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
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};
sampler grass = sampler_state
{
	Texture = <MultiTextureB>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};
sampler rock = sampler_state
{
	Texture = <MultiTextureC>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};
sampler snow = sampler_state
{
	Texture = <MultiTextureD>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};
//----------------------------------------------------MultiTex-----------------------------------
struct MultiTex_VS_OUT
{
	float4 position  : SV_Position;
	float3 normal        : NORMAL0;
	float3 tangent : TANGENT0;
	float3 binormal : TANGENT1;
	float3 worldPosition : POSITION1;
	float2 textureCoordinate : TEXCOORD0;
	float4 Pos2DAsSeenByLight : TEXCOORD1;
	float clipDistance : TEXCOORD2;
	float depth : TEXCOORD3;
	float color : COLOR0;
	float fog : COLOR1;
	float4 blend : TEXCOORD4;
	float4 depthBlend : TEXCOORD5;
};
struct MultiTex_VS_IN
{
	float4 Position  : SV_Position;
	float2 TextureCoordinate : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT0;
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
	float blendDistance1 = 100;
	Out.depthBlend.x = clamp((Out.depth - blendDistance1) / blendDistance1, 0, 1);
	return Out;
}
CommonPSOut ApplyMultiTexture(MultiTex_VS_OUT vsout)
{
	DoClip((CommonVSOut)vsout);
	float4 toReturn = (float4)0;
	if (UseTexture) {
		float texw = vsout.blend.x;
		float lodw = vsout.depthBlend.x;
		float2 coorda = vsout.textureCoordinate * 2;
		float2 coordb = vsout.textureCoordinate / 4;
		float3 sampled = 0;
		sampled += tex2D(sand, coorda) * vsout.blend[0] * (1 - vsout.depthBlend.x);
		sampled += tex2D(sand, coordb) * vsout.blend[0] * vsout.depthBlend.x;
		sampled += tex2D(grass, coorda) * vsout.blend[1] * (1 - vsout.depthBlend.x);
		sampled += tex2D(grass, coordb) * vsout.blend[1] * vsout.depthBlend.x;
		sampled += tex2D(rock, coorda) * vsout.blend[2] * (1 - vsout.depthBlend.x);
		sampled += tex2D(rock, coordb) * vsout.blend[2] * vsout.depthBlend.x;
		sampled += tex2D(snow, coorda) * vsout.blend[3] * (1 - vsout.depthBlend.x);
		sampled += tex2D(snow, coordb) * vsout.blend[3] * vsout.depthBlend.x;
		toReturn.rgb = sampled;
	}

	toReturn.rgb = lerp(toReturn.rgb, (float4)0,vsout.fog);
	toReturn.a = 1-vsout.fog;
	if(!aboveWater){
		toReturn.b+=.1f;
	}
	toReturn = PSLighting(toReturn, (CommonVSOut)vsout);
	return PSReturn(toReturn, (CommonVSOut)vsout);
}
technique MultiTexture
{
	pass P0
	{
		vertexShader = compile vs_4_0 TransformMulti();
		pixelShader = compile ps_4_0 ApplyMultiTexture();
	}
}
