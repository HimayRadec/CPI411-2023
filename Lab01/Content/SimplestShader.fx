texture MyTexture;

sampler mySampler = sampler_state {
	Texture = < MyTexture>;
};

struct VertexPositionTexture {
	float4 Position: POSITION;
	float2 TextureCoordinate : TEXCOORD;
};

VertexPositionTexture MyVertexShader(VertexPositionTexture input) {
	return input;
}

float4 MyPixelShader(VertexPositionTexture input) : COLOR
{
	return tex2D(mySampler, input.TextureCoordinate);
}

technique MyTechnique {
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShader();
	}
}