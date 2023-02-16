float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
Texture SkyBoxTexture;

samplerCUBE SkyBoxSampler = sampler_state
{
	texture = <SkyBoxTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;

	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput {
	float4 Position : POSITION0;
};

struct VertexShaderOutput {
	float4 Position : POSITION0;
	float3 TextureCoordinate : COORDINATE0;
};

VertexShaderOutput MyVertexShader(VertexShaderOutput input)
{
	VertexShaderOutput output;
	float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, View);
	output.Position = mul(viewPos, Projection);
	output.TextureCoordinate = worldPos.xyz - CameraPosition;

	return output;
}

float4 MyPixelShader(VertexShaderOutput input) : COLOR
{
	return texCUBE(SkyBoxSampler, normalize(input.TextureCoordinate));
}

technique MyTechnique
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShader();
	}
};