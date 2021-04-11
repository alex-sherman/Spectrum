#include "Header.fxh"

sampler customTexture = sampler_state
{
    Texture = <Texture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};
sampler customTextureWrap = sampler_state
{
    Texture = <Texture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
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

float4 PSLighting(float4 color, CommonVSOut vsout)
{
    float4 output = color;
    if (lightingEnabled)
    {
        output.rgb = (float3) 0;
        float3 normal;
        if (UseNormalMap)
        {
            float3 normalTex = 2 * tex2D(normalMap, vsout.textureCoordinate).rgb - 1;
            normal = vsout.normal * normalTex.b;
            normal += vsout.tangent * normalTex.r;
            normal += vsout.binormal * normalTex.g;
        }
        else
        {
            normal = vsout.normal;
        }
        float diffuseMagnitude = max(0, dot(normalize(normal), normalize(lightPosition - vsout.worldPosition)));
        if (UseShadowMap)
        {
            vsout.Pos2DAsSeenByLight /= vsout.Pos2DAsSeenByLight.w;
            float2 shadowCoord = vsout.Pos2DAsSeenByLight.xy;
            shadowCoord.y *= -1;
            shadowCoord = (shadowCoord + 1) / 2;
            if (shadowCoord.x >= 0 && shadowCoord.x <= 1 && shadowCoord.y >= 0 && shadowCoord.y <= 1)
            {
                float shadowDepth = 1 - ShadowMapTexture.Sample(shadowMapSampler, shadowCoord);
                float depth = vsout.Pos2DAsSeenByLight.z;
                if (shadowDepth - depth < -ShadowThreshold)
                {
                    diffuseMagnitude = 0;
                }
            }
        }
        output.rgb += color.rgb * min(1, (diffuseMagnitude * diffuseLightColor * (1 - ambientLightColor) + ambientLightColor));
    }
    return output;
}
CommonPSOut PSReturn(float4 color, CommonVSOut vsout)
{
    CommonPSOut output = (CommonPSOut) 0;
    output.color = color;
    output.position.xyz = vsout.worldPosition;
    output.position.a = 1;
    output.normal.xyz = vsout.normal;
    output.normal.a = 1;
    return output;
}
float3 VSCalculateLight(float3 normal, float3 worldPosition)
{
    float3 lightDirection = worldPosition - lightPosition;
    lightDirection.y *= -1;
    return (.2 + .8 * clamp(dot(normalize(lightDirection), normal), 0, 1));
}
void DoClip(CommonVSOut vsout)
{
    if (Clip)
    {
        clip(vsout.clipDistance);
    }
    if (vsout.fog >= .99f)
    {
        clip(-1);
    }
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
        float4 textureColor = TextureMagFilter ?
            (DiffuseWrap ? tex2D(customTextureWrap, vsout.textureCoordinate) : tex2D(customTexture, vsout.textureCoordinate))
            : tex2D(customTextureNoFilter, vsout.textureCoordinate);
        if (UseTransparency)
        {
            color.rgb = textureColor.rgb;
            color.a = 1 - tex2D(transparencySampler, vsout.textureCoordinate).r;
            color.rgb *= color.a;
        }
        else
            color = textureColor;
        color *= materialDiffuse;
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

