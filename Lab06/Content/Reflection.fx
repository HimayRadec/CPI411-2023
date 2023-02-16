float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;
texture decalMap;
texture environmentMap;

sampler tsampler1 = sampler_state {
	texture = <decalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

samplerCUBE SkyBoxSampler = sampler_state
{
	texture = <environmentMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};


struct VertexShaderInput // edit
{
	float4 Position : POSITION0;
	float4 TexCoord : TEXCOORD;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput // edit
{
	float4 Position : POSITION0;
	float3 TexCoord : TEXCOORD;
	float3 Reflection : TEXCOORD1;

};

VertexShaderOutput ReflectionVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, View);
	output.Position = mul(viewPos, Projection);
	output.TexCoord = input.TexCoord;

	float3 N = mul(input.Normal, WorldInverseTranspose).xyz;
	float3 I = normalize(worldPos.xyz - CameraPosition);
	output.R = reflect(I, N)

	return output;
}

float4 ReflectionPixelShader(VertexShaderOutput input) : COLOR0
{

	return texCUBE(SkyBoxSampler, input.R);
}

technique Reflection
{
	pass pass1
	{
		VertexShader = compile vs_4_0 ReflectionVertexShader();
		PixelShader = compile ps_4_0 ReflectionPixelShader();
	}
};