texture MyTexture;

float3 offset;
float4x4 World;
float4x4 View;
float4x4 Projection;

sampler mySampler = sampler_state {
	Texture = < MyTexture>;
};

struct VertexPositionTexture {
	float4 position: POSITION;
	float2 textureCoordinate : TEXCOORD;
};

VertexPositionTexture MyVertexShader(VertexPositionTexture input) {
	//input.position.xyz += offset;
	VertexPositionTexture output;
	float4 worldPos = mul(input.position, World);
	float4 viewPos = mul(worldPos, View);
	output.position = mul(viewPos, Projection);
	output.textureCoordinate = input.textureCoordinate;

	return output;
}

float4 MyPixelShader(VertexPositionTexture input) : COLOR
{
	return tex2D(mySampler, input.textureCoordinate);
}

technique MyTechnique {
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShader();
	}
}