
#include "Common.fxh"
float4x4 rotation;
//light properties
float darkness = 0;
//material properties
float specularPower;
float specularIntensity;
//For AA
bool AAEnabled = true;
float depthBlurStart = 0.95f;
float depthBlurScale = 0.5f;
uniform extern texture AATarget;
uniform extern texture DepthTarget;
float2 viewPort;
bool vingette = false;

sampler AASampler = sampler_state
{
	Texture = <AATarget>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter=LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};

sampler DepthSampler = sampler_state
{
	Texture = <DepthTarget>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter=LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};

float R = 3.0f; // Radius of vingette
float4 VingetteShader(float2 coord, float4 color)
{     
    float p = length(coord.xy-0.5)*R;            // Squared distance
    float d = 1 - 0.2f*p - 0.2*p*p;     // Fall off function
	//color.r = d;
    color.rgb = saturate(color.rgb*d);
    return color;
}

//------------------------------------------------Fast Post Process AA-----------------------------
float4 CalcAA(float2 texCoord)
{
	float total = 1;
	float3 value = tex2D(AASampler,texCoord).rgb;
	float weight = .3;
	total+=4*weight;

	texCoord.x-=1/viewPort.x;
	value += tex2D(AASampler, texCoord).rgb*weight;
	texCoord.x+=1/viewPort.x;

	texCoord.x+=1/viewPort.x;
	value += tex2D(AASampler, texCoord).rgb*weight;
	texCoord.x-=1/viewPort.x;

	texCoord.y-=1/viewPort.y;
	value += tex2D(AASampler, texCoord).rgb*weight;
	texCoord.y+=1/viewPort.y;

	texCoord.y+=1/viewPort.y;
	value += tex2D(AASampler, texCoord).rgb*weight;
	texCoord.y-=1/viewPort.y;

	float4 color = (float4)1;
	color.rgb = value/total;
	return color;
}
float getDepth(float2 coord) {
	return clamp((tex2D(DepthSampler, coord) - depthBlurStart) * depthBlurScale, 0, 1);
}
float3 Blur(float3 color, float2 texCoord)
{
	float3 output = (float3)0;
	float centerDepth = getDepth(texCoord);
	if (centerDepth == 0) { return color; }
	float weightSum = 0;
	for(int i = -2; i <= 2; i++) {
		for(int j = -2; j <= 2; j++) {
			float depth = getDepth(texCoord + float2(i / viewPort.x, j / viewPort.y));
			if (centerDepth < depth) continue;
			float weight = i == 0 && j == 0 ? (1 - depth) : (depth);
			float3 lerpColor = i == 0 && j == 0 ? color : tex2D(AASampler, texCoord + float2(i/viewPort.x, j/viewPort.y)).rgb;
			output += lerpColor * weight;
			weightSum += weight;
		}
	}
	return output / weightSum;
}
float4 AAPS(float4 position : SV_Position, float4 inputColor : COLOR0, float2 texCoord : TEXCOORD0) : COLOR
{
	float3 value = tex2D(AASampler, texCoord).rgb;
	float4 color = (float4)1;
	float2 orgCoord = texCoord;
	if(AAEnabled){
		float threshold = .1;
		texCoord.x-=1/viewPort.x;
		float3 other = tex2D(AASampler,texCoord);
		if(length(value-other)>threshold){ value = CalcAA(orgCoord); }
		texCoord.x+=2/viewPort.x;
		other = tex2D(AASampler,texCoord);
		if(length(value-other)>threshold){ value = CalcAA(orgCoord); }
		texCoord.x-=1/viewPort.x;
		texCoord.y-=1/viewPort.y;
		other = tex2D(AASampler,texCoord);
		if(length(value-other)>threshold){ value = CalcAA(orgCoord); }
		texCoord.y+=2/viewPort.y;
		other = tex2D(AASampler,texCoord);
		if(length(value-other)>threshold){ value = CalcAA(orgCoord); }
		texCoord.y-=1/viewPort.y;
	}
	color.rgb = Blur(value, texCoord)*(1-darkness);
	if(vingette){
		color = VingetteShader(texCoord,color);
	}
	return color;
}
sampler TextureSampler : register(s0);
CommonPSOut PassThrough2D(float4 position : SV_Position, float4 inputColor : COLOR0, float2 texCoord : TEXCOORD0)
{
	CommonPSOut output = (CommonPSOut)0;
	//I have no idea how alpha blending works
	output.color = inputColor * tex2D(TextureSampler, texCoord);
	output.depth = output.color.a > 0 ? 0 : 1;
	return output;
}

technique AAPP
{
	pass P0
	{
		pixelShader = compile ps_4_0 AAPS();
	}
}
technique PassThrough
{
	pass P0
	{
		pixelShader = compile ps_4_0 PassThrough2D();
	}
}