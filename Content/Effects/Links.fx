float4x4 World;
float4x4 View;
float4x4 Projection;
float Ambient = 0.0f;


struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Color = input.Color;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = input.Color*(1.0f + Ambient);
	color.w = 0.1f;
    return color;
}

technique Links
{
    pass Pass1
    {
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;

		/*StencilEnable = true;
		StencilMask = 0xFF;
		StencilWriteMask = 0xFF;
		StencilFail = Keep;
		StencilZFail = Keep;
		StencilPass = Keep;
		StencilFunc = Equal;*/

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
