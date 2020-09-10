
float4x4 rotation;
//light properties
float darkness = 0;
//material properties
float specularPower;
float specularIntensity;
uniform float3 cameraPosition = 0;
//For AA
uniform bool AAEnabled = true;
uniform float aaThreshold = .7f;
uniform float aaBlurFactor = .35f;
// Depth Blur
uniform float depthBlurStart = 100;
uniform float depthBlurScale = 0.001f;
uniform float blurSigma = 1.2f;
// Render Targets
uniform extern texture AATarget;
uniform extern texture PositionTarget;
uniform extern texture NormalTarget;
// HBAO
uniform extern float3 hbaoSamples[16];

float2 viewPort;
bool vingette = false;
static const float PI = 3.14159265f;

sampler AASampler = sampler_state
{
	Texture = <AATarget>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};

sampler PositionSampler = sampler_state
{
	Texture = <PositionTarget>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};

float R = 3.0f; // Radius of vingette
float4 VingetteShader(float2 coord, float4 color)
{
	float p = length(coord.xy - 0.5) * R; // Squared distance
	float d = 1 - 0.2f * p - 0.2 * p * p; // Fall off function
	//color.r = d;
	color.rgb = saturate(color.rgb * d);
	return color;
}

float getDepth(float2 coord)
{
	return clamp((length(tex2D(PositionSampler, coord).xyz - cameraPosition) - depthBlurStart) * depthBlurScale, 0, 1);
}

float3 Blur(float3 color, float2 texCoord, float blurFactor, float centerWeight = -1)
{
	float3 output = (float3) 0;
	if (blurFactor == 0)
	{
		return color;
	}
	float weightSum = 0;
	[unroll]
	for (int i = -2; i <= 2; i++)
	{
		[unroll]
		for (int j = -2; j <= 2; j++)
		{
			float weight = (i == 0 && j == 0 && centerWeight >= 0)
				? centerWeight
				: exp(-(pow(pow(i, 2) + pow(j, 2), 0.5)) / 2 / blurSigma / blurFactor) / (2 * PI * blurSigma * blurFactor);
			float3 lerpColor = i == 0 && j == 0 ? color : tex2D(AASampler, texCoord + float2(i / viewPort.x, j / viewPort.y)).rgb;
			output += lerpColor * weight;
			weightSum += weight;
		}
	}
	return output / weightSum;
}

float4 AAPS(float4 position : SV_Position, float4 inputColor : COLOR0, float2 texCoord : TEXCOORD0) : COLOR
{
	float3 value = tex2D(AASampler, texCoord).rgb;
	float4 color = (float4) 1;
	float2 orgCoord = texCoord;
	if (AAEnabled)
	{
		float diffSum = 0;
		texCoord.x -= 1 / viewPort.x;
		float3 other = tex2D(AASampler, texCoord);
		diffSum += length(value - other);
		texCoord.x += 2 / viewPort.x;
		other = tex2D(AASampler, texCoord);
		diffSum += length(value - other);
		texCoord.x -= 1 / viewPort.x;
		texCoord.y -= 1 / viewPort.y;
		other = tex2D(AASampler, texCoord);
		diffSum += length(value - other);
		texCoord.y += 2 / viewPort.y;
		other = tex2D(AASampler, texCoord);
		diffSum += length(value - other);
		texCoord.y -= 1 / viewPort.y;
		if (diffSum > aaThreshold)
		{
			value = Blur(value, texCoord, aaBlurFactor);
		}
	}
	color.rgb = Blur(value, texCoord, getDepth(texCoord)) * (1 - darkness);
	if (vingette)
	{
		color = VingetteShader(texCoord, color);
	}
	return color;
}
sampler TextureSampler : register(s0);

struct CommonPSOut
{
	float4 color : COLOR0;
	float4 depth : COLOR1;
};
CommonPSOut PassThrough2D(float4 position : SV_Position, float4 inputColor : COLOR0, float2 texCoord : TEXCOORD0)
{
	CommonPSOut output = (CommonPSOut) 0;
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