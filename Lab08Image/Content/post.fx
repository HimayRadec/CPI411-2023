

float4x4 MatrixTransform;
texture2D modelTexture;
float imageWidth;
float imageHeight;

sampler TextureSampler: register(s0) = sampler_state {
	texture = <modelTexture>;
	magfilter = LINEAR; // None, POINT, LINEAR, Anisotropic
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Clamp; // Clamp, Mirror, MirrorOnce, Wrap, Border
	AddressV = Clamp;
};
sampler FilterSampler: register(s1) = sampler_state {
	texture = <filterTexture>;
	magfilter = LINEAR; // None, POINT, LINEAR, Anisotropic
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Clamp; // Clamp, Mirror, MirrorOnce, Wrap, Border
	AddressV = Clamp;
};

struct VS_OUTPUT {
	float4 Pos: POSITION;
	float2 UV0: TEXCOORD0;
	float4 UV1: TEXCOORD1;
};

VS_OUTPUT vtxSh(float4 inPos: POSITION, float2 inTex : TEXCOORD0) {
	VS_OUTPUT Out;
	Out.Pos = mul(inPos, MatrixTransform);
	Out.UV0 = inTex;
	Out.UV1 = float4(2 / imageWidth, 0, 0, 2 / imageHeight);

	return Out;
}

float4 pxlSh(VS_OUTPUT In) : COLOR{
	float4 tex = tex2D(TextureSampler, In.UV0);

	// Effect 1
	// tex.rgb = ceil(tex * 8) / 8;

	//Effect 2
	tex.r = tex2D(FilterSampler, float2(tex.r, 0)).r;
	tex.g = tex2D(FilterSampler, float2(tex.g, 0)).g;
	tex.b = tex2D(FilterSampler, float2(tex.b, 0)).b;
	return tex;
}

technique MyShader {
	pass Pass1
	{
		vertexShader = compile vs_4_0 vtxSh();
		pixelShader = compile ps_4_0 pxlSh();
	}
};