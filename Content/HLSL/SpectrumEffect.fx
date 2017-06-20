#include "Common.fxh"

float4 Depth(CommonVSOut vsout) : COLOR0 {
	return float4(1 - vsout.Pos2DAsSeenByLight.z / vsout.Pos2DAsSeenByLight.w, 0, 0, 1);
}

technique Standard
{
	pass P0
	{
		vertexShader = compile vs_4_0 Transform();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
}
technique StandardInstance
{
	pass P0
	{
		vertexShader = compile vs_4_0 InstanceTransform();
		pixelShader = compile ps_4_0 ApplyTexture();
	}
}
technique ShadowMap
{
    pass P0
    {
        vertexShader = compile vs_4_0 Transform();
        pixelShader = compile ps_4_0 Depth();
    }
}
technique ShadowMapInstance
{
    pass P0
    {
        vertexShader = compile vs_4_0 InstanceTransform();
        pixelShader = compile ps_4_0 Depth();
    }
}
