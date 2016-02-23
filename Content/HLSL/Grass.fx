#include "Common.fxh"

CommonVSOut Transform(CommonVSInput input, float4x4 instanceWorld : TANGENT) {
	CommonVSOut Out = (CommonVSOut)0;
	CommonVSInput tInput = (CommonVSInput)input;
	tInput.Position = mul(tInput.Position, transpose(instanceWorld));
	float4 worldPosition = CommonVS((CommonVSInput)tInput, (CommonVSOut)Out);
	Out.clipDistance = length(worldPosition - cameraPosition) < 120 ? 1 : -1;
	
	return Out;
}
CommonPSOut ApplyTexture(CommonVSOut vsout)
{
	clip(vsout.clipDistance);
	float4 color = tex2D(customTexture, vsout.textureCoordinate).rgba;
	color.a *= 1 - vsout.fog;
	clip(color.a <= .3 ? -1 : 1);
	if (!aboveWater) {
		color.b += .1f;
	}
	if (lightingEnabled) {
		color.rgb *= clamp(dot(normalize(vsout.normal), normalize(lightPosition - vsout.worldPosition)), 0.2, 1);
	}
	return PSReturn(color, vsout);
}
technique TextureDraw
{
	pass P0
	{
		vertexShader = compile vs_4_0 Transform();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
}