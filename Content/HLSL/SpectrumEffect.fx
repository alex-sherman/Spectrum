#include "Common.fxh"


CommonVSOut CommonTransform(
	CommonVSInput input,
	float4x4 instanceTransform
	)
{
	CommonVSOut Out = (CommonVSOut)0;
	float4 worldPosition = mul(input.Position, instanceTransform);
	float4 viewPosition = mul(worldPosition, view);
	Out.position = mul(viewPosition, proj);
	Out.textureCoordinate = input.TextureCoordinate;
	Out.clipDistance = dot(worldPosition, ClipPlane);
	float3 lightDirection = lightPosition - worldPosition;
	//lightDirection.y *= -1;
	Out.light = clamp(dot(normalize(lightDirection), normalize(mul(input.normal, world))),0,1);
	Out.fog = clamp(1-(fogDistance-fogWidth-length(worldPosition-cameraPosition))/fogWidth,0,1);
	return Out;
}

CommonVSOut Transform(CommonVSInput input){
	CommonVSOut Out = (CommonVSOut)0;
	float4 worldPosition = CommonVS((CommonVSInput)input, (CommonVSOut)Out);
	return Out;
}
CommonVSOut InstanceTransform(CommonVSInput input, float4x4 instanceTranform : BLENDWEIGHT){
	return CommonTransform(input, mul(transpose(instanceTranform),world));
}
CommonPSOut ApplyTexture(CommonVSOut vsout)
{
	if(Clip) { clip(vsout.clipDistance); }
	if(vsout.fog >=.99f){ clip(-1); }
	float4 color = tex2D(customTexture, vsout.textureCoordinate).rgba;
	clip(color.a < 1 ? -1:1);
	if (lightingEnabled)
		color.rgb *= specularLightColor.rgb*vsout.light+ambientLightColor.rgb;
	if(!aboveWater){
		color.b+=.1f;
	}
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
technique InstanceDraw
{
	pass P0
	{
		vertexShader = compile vs_4_0 InstanceTransform();
		pixelShader  = compile ps_4_0 ApplyTexture();
	}
}