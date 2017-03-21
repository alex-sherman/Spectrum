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
uniform bool UseTexture = false;
uniform extern texture NormalMap;
uniform bool UseNormalMap = false;
uniform extern texture Transparency;
uniform bool UseTransparency = false;
uniform float4 diffuseColor = float4(1, 0, 1, 1);
uniform float3 ambientLightColor = float3(0.3,.3,.3);
uniform float3 diffuseLightColor = float3(1,1,1);
uniform float3 specularLightColor = float3(1,1,1);
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
sampler normalMap = sampler_state
{
	Texture = <NormalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};
sampler transparencySampler = sampler_state
{
	Texture = <Transparency>;
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
	float3 tangent : TANGENT;
};
struct CommonVSOut
{
	float4 position  : SV_Position;
	float3 normal : NORMAL0;
	float3 tangent : TANGENT0;
	float3 binormal : TANGENT1;
	float3 worldPosition : POSITION1;
	float2 textureCoordinate : TEXCOORD0;
	float4 Pos2DAsSeenByLight : TEXCOORD1;
	float clipDistance : TEXCOORD2;
	float depth : TEXCOORD3;
	float4 color : COLOR0;
	float fog	: COLOR1;
};
struct CommonPSOut
{
	float4 color : COLOR0;
	float4 depth : COLOR1;
};
float4 PSLighting(float4 color, CommonVSOut vsout) {
	float4 output = color;
	if (lightingEnabled) {
		output.rgb = (float3)0;
		float3 normal;
		if(UseNormalMap) {
			float3 normalTex = 2 * tex2D(normalMap, vsout.textureCoordinate).rgb - 1;
			normal = vsout.normal * normalTex.b;
			normal += vsout.tangent * normalTex.r;
			normal += vsout.binormal * normalTex.g;
		}
		else {
			normal = vsout.normal;
		}
		float3 light = (dot(normalize(normal), normalize(lightPosition - vsout.worldPosition)) + 1) / 2;
		output.rgb += color.rgb * light;
	}
	return output;
}
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
float4 CommonVS(CommonVSInput vin, float4x4 world, out CommonVSOut vsout){
	vsout = (CommonVSOut)0;
	float4 HworldPosition = mul(vin.Position, world);
	vsout.worldPosition = HworldPosition.xyz / HworldPosition.w;
	vsout.position =mul(mul(HworldPosition, view), proj);
	vsout.depth = length(vsout.worldPosition-cameraPosition);
	vsout.fog = clamp(1-(fogDistance-fogWidth-vsout.depth)/fogWidth,0,1);
	vsout.clipDistance = dot(vsout.worldPosition, ClipPlane);
	vsout.Pos2DAsSeenByLight = VSCalcPos2DAsSeenByLight(HworldPosition);
	vsout.textureCoordinate = vin.TextureCoordinate;
	vsout.normal = normalize(mul(vin.normal, world));
	vsout.tangent = vin.tangent == 0 ? 0 : normalize(mul(vin.tangent, (float3x3)world));
	vsout.binormal = cross(vsout.tangent, vsout.normal);
	return HworldPosition;
}
float4 CommonVS(CommonVSInput vin, out CommonVSOut vsout) {
	return CommonVS(vin, world, vsout);
}
