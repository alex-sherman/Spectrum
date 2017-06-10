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
uniform extern texture Texture;
uniform bool TextureMagFilter = true;
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

sampler customTexture = sampler_state
{
	Texture = <Texture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};
sampler customTextureNoFilter = sampler_state
{
	Texture = <Texture>;
	magfilter = POINT;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = clamp;
	AddressV = clamp;
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
	Texture = <ShadowMapTexture>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = clamp;
	AddressV = clamp;
};

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
		float diffuseMagnitude = (dot(normalize(normal), normalize(lightPosition - vsout.worldPosition)) + 1) / 2;
		if(UseShadowMap) {
			vsout.Pos2DAsSeenByLight /= vsout.Pos2DAsSeenByLight.w;
			float2 shadowCoord = vsout.Pos2DAsSeenByLight.xy;
			shadowCoord.y *= -1;
			shadowCoord = (shadowCoord + 1) / 2;
            if (shadowCoord.x >= 0 && shadowCoord.x <= 1 && shadowCoord.y >= 0 && shadowCoord.y <= 1)
            {
                float shadowDepth = 1 - ShadowMapTexture.Sample(shadowMapSampler, shadowCoord);
                float depth = vsout.Pos2DAsSeenByLight.z;
                if (shadowDepth - depth < -0.0001)
                {
                    diffuseMagnitude *= 0.5f;
                }
            }
        }
		output.rgb += color.rgb * min(1, (diffuseMagnitude * diffuseLightColor + ambientLightColor));
	}
	return output;
}
CommonPSOut PSReturn(float4 color, CommonVSOut vsout) {
	CommonPSOut output = (CommonPSOut)0;
	output.color = color;
	output.depth.rgb = vsout.position.z / vsout.position.w;
	output.depth.a = 1;
	return output;
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
	HworldPosition.w = 1;
	vsout.worldPosition = HworldPosition.xyz / HworldPosition.w;
	vsout.position =mul(mul(HworldPosition, view), proj);
	vsout.depth = length(vsout.worldPosition-cameraPosition);
	vsout.fog = clamp(1-(fogDistance-fogWidth-vsout.depth)/fogWidth,0,1);
	vsout.clipDistance = dot(vsout.worldPosition, ClipPlane);
	vsout.Pos2DAsSeenByLight = mul(HworldPosition, ShadowViewProjection);
	vsout.textureCoordinate = vin.TextureCoordinate;
	vsout.normal = normalize(mul(vin.normal, world));
	vsout.tangent = vin.tangent == 0 ? 0 : normalize(mul(vin.tangent, (float3x3)world));
	vsout.binormal = cross(vsout.tangent, vsout.normal);
	return HworldPosition;
}
float4 CommonVS(CommonVSInput vin, out CommonVSOut vsout) {
	return CommonVS(vin, world, vsout);
}
CommonPSOut ApplyTexture(CommonVSOut vsout)
{
	if (Clip)
	{
		clip(vsout.clipDistance);
	}
	if (vsout.fog >= .99f)
	{
		clip(-1);
	}
	float4 color;
	if (UseTexture)
	{
		float4 textureColor = TextureMagFilter ? tex2D(customTexture, vsout.textureCoordinate) : tex2D(customTextureNoFilter, vsout.textureCoordinate);
		if (UseTransparency)
		{
			color.rgb = textureColor.rgb;
			color.a = 1 - tex2D(transparencySampler, vsout.textureCoordinate).r;
			color.rgb *= color.a;
		}
		else
            color = textureColor * materialDiffuse;
    }
	else
	{
		color = diffuseColor * materialDiffuse;
	}
	clip(color.a <= 0 ? -1 : 1);
	color = PSLighting(color, vsout);
	color.a *= 1 - vsout.fog;
	return PSReturn(color, vsout);
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
    float4 worldPosition = CommonVS(input, mul(transpose(instanceWorld), world), Out);
	Out.clipDistance = length(worldPosition.xyz - cameraPosition) < 120 ? 1 : -1;
	return Out;
}

