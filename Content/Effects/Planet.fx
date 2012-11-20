
float4x4 World;
float4x4 View;
float4x4 Projection;
Texture Diffuse;
Texture Clouds;
Texture CloudsAlpha;
float Ambient = 0.0f;
float4 Glow;
float PlanetOpacity = 1.0f;
float PlanetGrayScale = 0;


sampler DiffuseSampler = sampler_state
{
	texture = <Diffuse>;
	minfilter = LINEAR;
	magfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};
sampler CloudsSampler = sampler_state
{
	texture = <Clouds>;
	minfilter = LINEAR;
	magfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};
sampler CloudsAlphaSampler = sampler_state
{
	texture = <CloudsAlpha>;
	minfilter = LINEAR;
	magfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};


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

	float4 position = input.Position * float4(1.05, 1.05, 1.05, 1.0);
    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Normal = mul(input.Normal, World);
	output.UV = input.UV;

    // TODO: add your vertex shader code here.

    return output;
}

float4 Glow_PixelShader(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.

	return Glow;
}

VertexShaderOutput Surface_VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Normal = mul(input.Normal, World);
	output.UV = input.UV;

    // TODO: add your vertex shader code here.

    return output;
}

float4 Surface_PixelShader(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.

	float4 diffuse = tex2D(DiffuseSampler, input.UV);

	diffuse.rgb = PlanetGrayScale * dot(diffuse.rgb, float3(0.3, 0.59, 0.11));

	float4 color = diffuse*(1.0f + Ambient);
	color.w = PlanetOpacity;
	return color;
}

VertexShaderOutput Clouds_VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 position = input.Position * float4(1.02, 1.02, 1.02, 1.0);
    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Normal = mul(input.Normal, World);
	output.UV = input.UV;

    // TODO: add your vertex shader code here.

    return output;
}

float4 Clouds_PixelShader(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.

	float4 clouds = tex2D(CloudsSampler, input.UV);
	float cloudsAlpha = 1.0 - tex2D(CloudsAlphaSampler, input.UV).x;

	float4 color = clouds;
	color.w = clouds.w * cloudsAlpha * PlanetOpacity;
    return color;
}

technique Planet
{
    pass Pass1
    {
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		
		
		StencilEnable = true;
		StencilMask = 0xFF;
		StencilWriteMask = 0xFF;
		StencilFail = Keep;
		StencilZFail = Keep;
		StencilPass = IncrSat;
		StencilFunc = Always;

        VertexShader = compile vs_2_0 Surface_VertexShader();
        PixelShader = compile ps_2_0 Surface_PixelShader();
    }
	pass Glow
	{
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;

		
		StencilEnable = true;
		StencilMask = 0xFF;
		StencilWriteMask = 0xFF;
		StencilFail = IncrSat;
		StencilZFail = Keep;
		StencilPass = Keep;
		StencilFunc = Equal;

        VertexShader = compile vs_2_0 Glow_VertexShader();
        PixelShader = compile ps_2_0 Glow_PixelShader();
	}
    pass Pass2
    {
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = One;

		StencilEnable = false;

        VertexShader = compile vs_2_0 Clouds_VertexShader();
        PixelShader = compile ps_2_0 Clouds_PixelShader();
    }
}
