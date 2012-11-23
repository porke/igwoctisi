float4x4 World;
float4x4 View;
float4x4 Projection;
float4 Glow;
float PlanetOpacity = 1.0f;	// 0.0 - 1.0
float PlanetGrayScale = 0;	// 0 or 1


struct VertexShaderInput
{
    float4 Position : POSITION;
	float3 Normal : NORMAL;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
	float3 Normal : TEXCOORD1;
	float2 UV : TEXCOORD0;

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput Glow_VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 position = input.Position;// * float4(1.05, 1.05, 1.05, 1.0);
    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Normal = mul(input.Normal, World);
	output.UV = input.UV;
	
    return output;
}

float4 Glow_PixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color = Glow;

	if (PlanetGrayScale > 0)
		color.rgb = PlanetGrayScale * dot(color.rgb, float3(0.3, 0.59, 0.11));

	color.w = sqrt(PlanetOpacity);

	return color;
}


technique Planet
{
	pass Glow
	{
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		
		StencilEnable = true;
		StencilMask = 0xFF;
		StencilWriteMask = 0xFF;
		StencilFail = Keep;
		StencilZFail = Keep;
		StencilPass = IncrSat;
		StencilFunc = Always;

        VertexShader = compile vs_2_0 Glow_VertexShader();
        PixelShader = compile ps_2_0 Glow_PixelShader();
	}
}
