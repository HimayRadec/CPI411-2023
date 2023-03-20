float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;
texture decalMap;
texture environmentMap;

sampler tsampler1 = sampler_state {
	texture = <decalMap>;
	/*magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;*/
};
samplerCUBE SkyBoxSampler = sampler_state
{
	texture = <environmentMap>;
	/*magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;*/
};

struct VertexShaderInput
{
	float4 Position: POSITION0;
	float2 texCoord: TEXCOORD0;
	float4 normal: NORMAL0;
};
struct VertexShaderOutput
{
	float4 Position: POSITION0;
	float2 texCoord: TEXCOORD0;
	float3 R: TEXCOORD1; // *** Reflectoin Vector
};

VertexShaderOutput ReflectionVertexShaderFunction(VertexShaderInput input) {
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 projPosition = mul(viewPosition, Projection);
	output.Position = projPosition;

	float3 N = normalize( mul(input.normal, WorldInverseTranspose).xyz );
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.R = reflect(I, N);
	output.texCoord = input.texCoord;
	
	return output;
}

float4 ReflectionPixelShaderFunction(VertexShaderOutput input): COLOR0
{
	float4 reflectedColor = texCUBE(SkyBoxSampler, input.R);
	float4 decalColor = tex2D(tsampler1, input.texCoord);
	return lerp(decalColor, reflectedColor, 0.9);
	//return reflectedColor;
}

technique Reflection {
	pass Pass1 {
		VertexShader = compile vs_4_0 ReflectionVertexShaderFunction();
		PixelShader = compile ps_4_0 ReflectionPixelShaderFunction();
	}
}