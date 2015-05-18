
#include "Common.fxh"
float4x4 rotation;
//light properties
float darkness = 0;
//material properties
float specularPower;
float specularIntensity;
//For AA
bool AAEnabled = true;
uniform extern texture AATarget;
float2 viewPort;
bool vingette = false;


struct VertexShaderOutputPerVertexDiffuse
{
	float4 Position : POSITION;
	float3 WorldNormal : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float4 textureCoord : TEXCOORD2;
	float4 Color : COLOR0;
};


struct PixelShaderInputPerVertexDiffuse
{
	float4 textureCoord : TEXCOORD2;
	float3 WorldNormal : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
	float4 Color: COLOR0;
};

sampler AASampler = sampler_state
{
	Texture = <AATarget>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter=LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};


//------------------------------------------------Shadow Map-----------------------------
struct ShadowVSInput
{
	float4 Position  : POSITION;
};
struct ShadowVSOutput
{
	float4 position  : POSITION;
	float4 position2d  : TEXCOORD;
};

ShadowVSOutput ShadowVS(
	ShadowVSInput input
	)
{
	ShadowVSOutput Out = (ShadowVSOutput)0;
	float4 worldPosition = mul(input.Position, world);
		Out.position = mul(worldPosition, lightViewProjectionMatrix);
	Out.position2d = Out.position;
	return Out;
}
float4 ShadowPS(ShadowVSOutput vsout) : COLOR
{
	float4 color = (float4)0;
		//color.r = .00196265;
		color.r = 600.0/(vsout.position2d.z-1000.0);
	//color.g = 
	color.a = 1;
	return color;
}

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
	float3 value = tex2D(AASampler,texCoord);
		float weight = .3;
	total+=4*weight;

	texCoord.x-=1/viewPort.x;
	value += tex2D(AASampler,texCoord)*weight;
	texCoord.x+=1/viewPort.x;

	texCoord.x+=1/viewPort.x;
	value += tex2D(AASampler,texCoord)*weight;
	texCoord.x-=1/viewPort.x;

	texCoord.y-=1/viewPort.y;
	value += tex2D(AASampler,texCoord)*weight;
	texCoord.y+=1/viewPort.y;

	texCoord.y+=1/viewPort.y;
	value += tex2D(AASampler,texCoord)*weight;
	texCoord.y-=1/viewPort.y;

	float4 color = (float4)1;
	color.rgb = value/total;
	return color;
}
float4 AAPS(float4 position : SV_Position, float4 inputColor : COLOR0, float2 texCoord : TEXCOORD0) : COLOR
{
	float3 value = tex2D(AASampler,texCoord);
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
	color.rgb = value*(1-darkness);
	if(vingette){
		color = VingetteShader(texCoord,color);
	}

	return color;
}

technique AAPP
{
	pass P0
	{
		pixelShader = compile ps_4_0 AAPS();
	}
}
technique ShadowMap
{
	pass P0
	{
		vertexShader = compile vs_4_0 ShadowVS();
		pixelShader  = compile ps_4_0 ShadowPS();
	}

}