float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;
float3 LightPosition;

float4 AmbientColor;
float AmbientIntensity;
float4 DiffuseColor;
float DiffuseIntensity;
float4 SpecularColor;
float SpecularIntensity;
float Shininess;

texture normalMap;

sampler tsampler1 = sampler_state {
	texture = <normalMap>;
	magfilter = LINEAR; // None, POINT, LINEAR, Anisotropic
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap; // Clamp, Mirror, MirrorOnce, Wrap, Border
	AddressV = Wrap;
};

struct VertexInput
{
	float4 Position: POSITION;
	float2 TexCoord: TEXCOORD;
	float4 Normal: NORMAL;
	float4 Tangent: TANGENT0;
	float4 Binormal: BINORMAL0;
};

struct VertexShaderOutput
{
	float4 Position: POSITION;
	float3 Normal: TEXCOORD0;
	float3 Tangent: TEXCOORD1;
	float3 Binormal: TEXCOORD2;
	float2 TexCoord: TEXCOORD3;
	float3 Position3D: TEXCOORD4; // World Position

};

VertexShaderOutput BumpMapVertexShader(VertexInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.Position3D = worldPosition.xyz;
	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
	output.Tangent = normalize(mul(input.Tangent, WorldInverseTranspose).xyz);
	output.Binormal = normalize(mul(input.Binormal, WorldInverseTranspose).xyz);
	output.TexCoord = input.TexCoord;

	return output;
}

float4 BumpMapPixelShader(VertexShaderOutput input) : COLOR0
{
	float3 N = input.Normal;
	float3 V = normalize(CameraPosition - input.Position3D.xyz);
	float3 L = normalize(LightPosition - input.Position3D.xyz);
	float3 R = reflect(-L, N);

	float3 T = input.Tangent;
	float3 B = input.Binormal;
	float3 H = normalize(L + V);

	float3 normalTex = (tex2D(tsampler1, input.TexCoord).xyz - float3(0.5, 0.5, 0.5)) * 2.0;

	float3 bumpNormal = N + normalTex.x* T + normalTex.y * B;

	float4 ambient = AmbientColor * AmbientIntensity;
	float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(bumpNormal, L));
	float4 specular = pow(max(0, dot(H, bumpNormal)), Shininess) * SpecularColor * SpecularIntensity;
	float4 color = saturate(ambient + diffuse + specular);

	return color;
}

technique BumpMap
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 BumpMapVertexShader();
		PixelShader = compile ps_4_0 BumpMapPixelShader();
	}
};