#include "Common.fxh"

float4x4 worlds[16];

CommonVSOut Transform(CommonVSInput input, int i) {
	CommonVSOut Out = (CommonVSOut)0;
	CommonVSInput tInput = (CommonVSInput)input;
	tInput.Position = mul(tInput.Position, worlds[i]);
	float4 worldPosition = CommonVS((CommonVSInput)tInput, (CommonVSOut)Out);
	Out.clipDistance = length(worldPosition - cameraPosition) < 60 ? 1 : -1;
	
	return Out;
}

CommonVSOut Transform0(CommonVSInput input) { return Transform(input, 0); }
CommonVSOut Transform1(CommonVSInput input) { return Transform(input, 1); }
CommonVSOut Transform2(CommonVSInput input) { return Transform(input, 2); }
CommonVSOut Transform3(CommonVSInput input) { return Transform(input, 3); }
CommonVSOut Transform4(CommonVSInput input) { return Transform(input, 4); }
CommonVSOut Transform5(CommonVSInput input) { return Transform(input, 5); }
CommonVSOut Transform6(CommonVSInput input) { return Transform(input, 6); }
CommonVSOut Transform7(CommonVSInput input) { return Transform(input, 7); }
CommonVSOut Transform8(CommonVSInput input) { return Transform(input, 8); }
CommonVSOut Transform9(CommonVSInput input) { return Transform(input, 9); }
CommonVSOut Transform10(CommonVSInput input) { return Transform(input, 10); }
CommonVSOut Transform11(CommonVSInput input) { return Transform(input, 11); }
CommonVSOut Transform12(CommonVSInput input) { return Transform(input, 12); }
CommonVSOut Transform13(CommonVSInput input) { return Transform(input, 13); }
CommonVSOut Transform14(CommonVSInput input) { return Transform(input, 14); }
CommonVSOut Transform15(CommonVSInput input) { return Transform(input, 15); }
CommonPSOut ApplyTexture(CommonVSOut vsout)
{
	clip(vsout.clipDistance);
	float4 color = tex2D(customTexture, vsout.textureCoordinate).rgba;
	color.a *= 1 - vsout.fog;
	clip(color.a <= 0 ? -1 : 1);
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
		vertexShader = compile vs_4_0 Transform0();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P1
	{
		vertexShader = compile vs_4_0 Transform1();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P2
	{
		vertexShader = compile vs_4_0 Transform2();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P3
	{
		vertexShader = compile vs_4_0 Transform3();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P4
	{
		vertexShader = compile vs_4_0 Transform4();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P5
	{
		vertexShader = compile vs_4_0 Transform5();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P6
	{
		vertexShader = compile vs_4_0 Transform6();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P7
	{
		vertexShader = compile vs_4_0 Transform7();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P8
	{
		vertexShader = compile vs_4_0 Transform8();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P9
	{
		vertexShader = compile vs_4_0 Transform9();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P10
	{
		vertexShader = compile vs_4_0 Transform10();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P11
	{
		vertexShader = compile vs_4_0 Transform11();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P12
	{
		vertexShader = compile vs_4_0 Transform12();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P13
	{
		vertexShader = compile vs_4_0 Transform13();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P14
	{
		vertexShader = compile vs_4_0 Transform14();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
	pass P15
	{
		vertexShader = compile vs_4_0 Transform15();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
}