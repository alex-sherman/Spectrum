#include "Common.fxh"

float4x4 reflView;
float4x4 reflProj;
uniform extern texture WaterBump;
uniform extern texture WaterBumpBase;
uniform extern texture Reflection;
uniform extern texture Refraction;
float2 windDirection;
float waterTime;
float waterPerturbCoef = 1.4;
float waveHeight = 10;
sampler waterReflection = sampler_state
{
	Texture = <Reflection>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter=LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};
sampler waterRefraction = sampler_state
{
	Texture = <Refraction>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter=LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};
sampler waterBump = sampler_state
{
	Texture = <WaterBump>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter=LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};
sampler waterBumpBase = sampler_state
{
	Texture = <WaterBumpBase>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter=LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};
//------------------------------------------------Draw Water-----------------------------
struct WaterVSOutput
{
	float4 position  : SV_Position;
	float4 vPosition : TEXCOORD1;
	float4 textureCoordinate : TEXCOORD0;
	float2 BumpMapSamplingPos        : TEXCOORD2;
	float depth		 : TEXCOORD3;
	float fog		 : TEXCOORD4;
	float3 lightDirection : TEXCOORD5;
	float2 BumpMapBasePos : TEXCOORD6;
};
WaterVSOutput WaterVS(
	float4 Position  : SV_Position, 
	float4 TextureCoordinate : TEXCOORD0)
{
	WaterVSOutput Out = (WaterVSOutput)0;
	float4x4 preViewProjection = mul (view, proj);
	float4x4 preWorldViewProjection = mul (world, preViewProjection);
	Out.position = mul(Position, preWorldViewProjection);
	Out.vPosition = mul(Position, world);
	Out.depth = length(cameraPosition-Position);
	float4x4 reflectionViewProj = mul(reflView, reflProj);
	float4x4 reflectionWorldViewProj = mul(world, reflectionViewProj);
	Out.textureCoordinate = mul(Position,reflectionWorldViewProj);
	Out.BumpMapBasePos = Out.vPosition.xz/120.0f+windDirection*waterTime/3000.0f;
	Out.BumpMapSamplingPos = Out.vPosition.xz/60.0f+windDirection*waterTime/1000.0f;
	Out.fog = clamp(1-(fogDistance-fogWidth-length(Out.vPosition-cameraPosition))/fogWidth,0,1);
	Out.lightDirection = Out.vPosition.xyz/Out.vPosition.w-lightPosition;
	Out.position.y += sin(length(Out.vPosition.x)/20+waterTime/50)*waveHeight;
	return Out;
}
float4 ApplyWaterTexture(WaterVSOutput vsout) : COLOR
{
	if(vsout.fog >=.99f){ clip(-1); }
	float2 reflTexCoord;
	float B = 0.8f;
	float4 bumpColor = tex2D(waterBump, vsout.BumpMapSamplingPos)*(1-B);
	bumpColor += tex2D(waterBumpBase, vsout.BumpMapBasePos)*B;
	float2 perturbation = (bumpColor.rg - 0.5f)*waterPerturbCoef;

	float2 ProjectedRefrTexCoords;
	ProjectedRefrTexCoords = vsout.textureCoordinate/vsout.textureCoordinate.w/2.0f+.5f;
	reflTexCoord = ProjectedRefrTexCoords;
	reflTexCoord.y = (1- (reflTexCoord.y));
	float2 perturbatedTexCoords = reflTexCoord +  perturbation*.1f;
	float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords +  perturbation*.1f; 
    //This sorts out the first bottom edge problem
	//if(!(perturbatedTexCoords.x>=1 || perturbatedTexCoords.y<=-1 || perturbatedTexCoords.y>=0 || perturbatedTexCoords.x<=0)) { return float4(1,1,1,0); }
	float4 refractiveColor;

	float3 eyeVector = normalize(cameraPosition - vsout.vPosition);

	float fresnelTerm = dot(eyeVector, float3(0,1,0));

	float3 normal = float3(0,1,0);
	normal.xz = (bumpColor.rg - .5f)*waterPerturbCoef;
	float3 reflectionVector = reflect(normalize(vsout.lightDirection), normalize(normal));
	float specular = dot(normalize(reflectionVector), normalize(eyeVector));
	specular = pow(specular, 256);    

	float4 reflectionColor;
	float4 refractionColor;
	float4 color;
	reflectionColor = tex2D(waterReflection,perturbatedTexCoords);
	refractionColor = tex2D(waterRefraction,perturbatedRefrTexCoords);
	if(aboveWater){
		color = lerp(reflectionColor, refractionColor, fresnelTerm);
	}
	else{color = refractionColor;}
	color = lerp(color,float4(.3f, 0.3f, 1.0f, 0),.2f);
    //TODO: This is broken for some reason in DX11
	//color.rgb += specular*specularLightColor;
	color.a = refractionColor.a;
	color.a = min(color.a, 1-vsout.fog);
	color.rgb*=color.a;
    return color;
}
technique WaterEffect
{
	pass P0
	{
		vertexShader = compile vs_4_0 WaterVS();
		pixelShader  = compile ps_4_0 ApplyWaterTexture();
	}
}