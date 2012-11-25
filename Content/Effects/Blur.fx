
float4x4 World;
float4x4 View;
float4x4 Projection;

Texture Diffuse;
float2 BlurRange;
float2 Resolution;

float offsets[] = { -3, -2, -1, 0, 1, 2, 3 };
float weights[] = { 0.09f, 0.11f, 0.18f, 0.24f, 0.18f, 0.11f, 0.09f };

sampler DiffuseSampler = sampler_state
{
	texture = <Diffuse>;
	minfilter = LINEAR;
	magfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};


struct VertexShaderInput
{
    float4 Position : POSITION;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
	float2 UV : TEXCOORD0;
};

VertexShaderOutput Surface_VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;

    return output;
}

float4 Surface_PixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color = float4(0, 0, 0, 1);

	for (int i = 0; i < 7; ++i)
	{
		float2 uv = input.UV + offsets[i]*BlurRange + 0.5f/Resolution;
		float4 sample = tex2D(DiffuseSampler, uv);
		color += sample * weights[i];
	}
	
	//float4 diffuse = tex2D(DiffuseSampler, input.UV + 0.5f/Resolution);
	return color;
}

technique Planet
{
    pass Pass1
	{
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;		
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;

        VertexShader = compile vs_2_0 Surface_VertexShader();
        PixelShader = compile ps_2_0 Surface_PixelShader();
    }
}
