#include "Common.fxh"

CommonVSOut Transform(CommonVSInput input){
	CommonVSOut Out = (CommonVSOut)0;
	float4 worldPosition = CommonVS((CommonVSInput)input, (CommonVSOut)Out);
	return Out;
}
CommonPSOut ApplyTexture(CommonVSOut vsout)
{
	if(Clip) { clip(vsout.clipDistance); }
	if(vsout.fog >=.99f){ clip(-1); }
	float4 color = tex2D(customTexture, vsout.textureCoordinate).rgba;
	clip(color.a <= 0 ? -1:1);
	if(!aboveWater){
		color.b+=.1f;
	}
	color = PSLighting(color, vsout);
	color.a *= 1-vsout.fog;
	return PSReturn(color, vsout);
}
technique TextureDraw
{
	pass P0
	{
		vertexShader = compile vs_4_0 Transform();
		pixelShader  = compile ps_4_0 ApplyTexture();
	}
}