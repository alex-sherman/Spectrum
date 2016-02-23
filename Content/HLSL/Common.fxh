float4x4 view;
float4x4 world;
float4x4 proj;
float4 ClipPlane;
float3 lightPosition;
float mixLerp;
float3 mixColor;
float3 cameraPosition;
float fogDistance = 6400;
float fogWidth = 100;
bool Clip = false;
uniform extern texture Texture;
float4 ambientLightColor = float4(0,0,0,1);
float4 diffuseLightColor = float4(1,1,1,1);
float4 specularLightColor = float4(1,1,1,1);
bool aboveWater = true;
float4x4 lightViewProjectionMatrix;
bool shadowMapEnabled = false;
uniform extern texture ShadowMapTex;
bool lightingEnabled = true;

sampler customTexture = sampler_state
{
	Texture = <Texture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};
sampler shadowMapSampler = sampler_state
{
	Texture = <ShadowMapTex>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter=LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};

struct CommonVSInput
{
	float4 Position  : SV_Position;
	float2 TextureCoordinate : TEXCOORD0;
	float3 normal : NORMAL;
};
struct CommonVSOut
{
	float4 position  : SV_Position;
	float3 normal : NORMAL0;
	float3 worldPosition : POSITION1;
	float2 textureCoordinate : TEXCOORD0;
	float4 Pos2DAsSeenByLight : TEXCOORD1;
	float clipDistance : TEXCOORD2;
	float depth : TEXCOORD3;
	float fog	: COLOR0;
};
struct CommonPSOut
{
	float4 color : COLOR0;
	float4 depth : COLOR1;
};
CommonPSOut PSReturn(float4 color, CommonVSOut vsout) {
	CommonPSOut output = (CommonPSOut)0;
	output.color = color;
	output.depth.rgb = max(0, (vsout.position.w - 1000) / 10000);
	output.depth.a = 1;
	return output;
}
float4 VSCalcPos2DAsSeenByLight(float4 worldPosition){
	return mul(worldPosition, lightViewProjectionMatrix);
}
float3 VSCalculateLight(float3 normal, float3 worldPosition){
	float3 lightDirection = worldPosition - lightPosition;
		lightDirection.y *= -1;
	return (.2+.8*clamp(dot(normalize(lightDirection), normal),0,1));
}
void DoClip(CommonVSOut vsout){
	if(Clip) { clip(vsout.clipDistance); }
	if(vsout.fog >=.99f){ clip(-1); }
}
float4 CommonVS(CommonVSInput vin, out CommonVSOut vsout){
	vsout = (CommonVSOut)0;
	float4 HworldPosition = mul(vin.Position, world);
	vsout.worldPosition = HworldPosition.xyz / HworldPosition.w;
	vsout.position =mul(mul(HworldPosition, view), proj);
	vsout.depth = length(vsout.worldPosition-cameraPosition);
	vsout.fog = clamp(1-(fogDistance-fogWidth-vsout.depth)/fogWidth,0,1);
	vsout.clipDistance = dot(vsout.worldPosition, ClipPlane);
	vsout.Pos2DAsSeenByLight = VSCalcPos2DAsSeenByLight(HworldPosition);
	vsout.textureCoordinate = vin.TextureCoordinate;
	vsout.normal = mul(vin.normal, (float3x3)world);
	return HworldPosition;
}
