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
	mipfilter=LINEAR;
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
	float2 textureCoordinate : TEXCOORD0;
	float4 Pos2DAsSeenByLight : TEXCOORD1;
	float clipDistance : TEXCOORD2;
	float depth : TEXCOORD3;
	float fog	: COLOR0;
	float light : COLOR1;
};
float4 VSCalcPos2DAsSeenByLight(float4 worldPosition){
	return mul(worldPosition, lightViewProjectionMatrix);
}
float3 VSCalculateLight(float3 normal, float4 worldPosition){
	float3 lightDirection = worldPosition - lightPosition;
		lightDirection.y *= -1;
	return (.2+.8*clamp(dot(normalize(lightDirection), normal),0,1));
}
float3 PSCalculateLight(CommonVSOut vsout){
	float3 lightEffect = vsout.light;
	if(lightingEnabled)
		lightEffect = specularLightColor.rgb*lightEffect+ambientLightColor.rgb;
	if(shadowMapEnabled){
		float2 ProjectedTexCoords; 
		ProjectedTexCoords.x = vsout.Pos2DAsSeenByLight.x/vsout.Pos2DAsSeenByLight.w/2.0f +0.5f;
		ProjectedTexCoords.y = -vsout.Pos2DAsSeenByLight.y/vsout.Pos2DAsSeenByLight.w/2.0f +0.5f;
		if(ProjectedTexCoords.x <1 && ProjectedTexCoords.x>0 && ProjectedTexCoords.y<1 && ProjectedTexCoords.y>0){
			float3 shadow = tex2D(shadowMapSampler, ProjectedTexCoords);
				if(600.0/shadow.r+1000<vsout.Pos2DAsSeenByLight.z-10){
					lightEffect = ambientLightColor.rgb;
				}
				else
				{
					lightEffect = specularLightColor.rgb*vsout.light+ambientLightColor.rgb;
				}
		}
	}
	return lightEffect;
}
void DoClip(CommonVSOut vsout){
	if(Clip) { clip(vsout.clipDistance); }
	if(vsout.fog >=.99f){ clip(-1); }
}
float4 CommonVS(CommonVSInput vin, out CommonVSOut vsout){
	vsout = (CommonVSOut)0;
	float4 worldPosition = mul(vin.Position, world);
	vsout.position =mul(mul(worldPosition, view), proj);
	vsout.depth = length(worldPosition-cameraPosition);
	vsout.fog = clamp(1-(fogDistance-fogWidth-vsout.depth)/fogWidth,0,1);
	vsout.clipDistance = dot(worldPosition, ClipPlane);
	vsout.light = VSCalculateLight(mul(vin.normal,world), worldPosition);
	vsout.Pos2DAsSeenByLight = VSCalcPos2DAsSeenByLight(worldPosition);
	vsout.textureCoordinate = vin.TextureCoordinate;
	return worldPosition;
}