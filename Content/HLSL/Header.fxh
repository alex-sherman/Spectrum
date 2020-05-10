float4x4 view;
float4x4 world;
float4x4 proj;
float4 ClipPlane;
float3 lightPosition;
float3 cameraPosition;
float fogDistance = 6400;
float fogWidth = 100;
bool Clip = false;
float4x4 ShadowViewProjection;
uniform extern bool UseShadowMap;
uniform extern float ShadowThreshold = 0.0001;
uniform extern texture Texture;
uniform extern float2 DiffuseTextureOffset = 0;
uniform bool TextureMagFilter = true;
uniform bool DiffuseWrap = true;
uniform bool UseTexture = false;
uniform extern texture NormalMap;
uniform bool UseNormalMap = false;
uniform extern texture Transparency;
uniform bool UseTransparency = false;
uniform float4 diffuseColor = float4(1, 1, 1, 1);
uniform float4 materialDiffuse = float4(1, 0, 1, 1);
uniform float3 ambientLightColor = float3(0.2,.2,.2);
uniform float3 diffuseLightColor = float3(0.8,0.8,0.8);
uniform float3 specularLightColor = float3(1,1,1);
bool lightingEnabled = true;
Texture2D<float> ShadowMapTexture;

struct CommonVSInput
{
    float4 Position : POSITION0;
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
	float depth : COLOR1;
};

float4 CommonVS(CommonVSInput vin, float4x4 world, out CommonVSOut vsout){
	vsout = (CommonVSOut)0;
	float4 HworldPosition = mul(vin.Position, world);
	HworldPosition.w = 1;
	vsout.worldPosition = HworldPosition.xyz;
	vsout.position = mul(mul(HworldPosition, view), proj);
	vsout.depth = length(vsout.worldPosition-cameraPosition);
	vsout.fog = clamp(1-(fogDistance-fogWidth-vsout.depth)/fogWidth,0,1);
	vsout.clipDistance = dot(vsout.worldPosition, ClipPlane);
	vsout.Pos2DAsSeenByLight = mul(HworldPosition, ShadowViewProjection);
	vsout.textureCoordinate = vin.TextureCoordinate + DiffuseTextureOffset;
	vsout.normal = normalize(mul(vin.normal, world));
	vsout.tangent = vin.tangent == 0 ? 0 : normalize(mul(vin.tangent, (float3x3)world));
	vsout.binormal = cross(vsout.tangent, vsout.normal);
	return HworldPosition;
}
float4 CommonVS(CommonVSInput vin, out CommonVSOut vsout) {
	return CommonVS(vin, world, vsout);
}
CommonVSOut Transform(CommonVSInput input)
{
	CommonVSOut Out = (CommonVSOut) 0;
	float4 worldPosition = CommonVS((CommonVSInput) input, (CommonVSOut) Out);
	return Out;
}
CommonVSOut InstanceTransform(CommonVSInput input, float4x4 instanceWorld : POSITION1)
{
	CommonVSOut Out = (CommonVSOut) 0;
    float4 worldPosition = CommonVS(input, mul(world, transpose(instanceWorld)), Out);
	Out.clipDistance = length(worldPosition.xyz - cameraPosition) < 120 ? 1 : -1;
	return Out;
}