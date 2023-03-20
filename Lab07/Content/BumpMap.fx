// *** CPI411 Lab#7 (BumpMap) 

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;
float3 LightPosition;

// Light Uniforms
float4 AmbientColor;
float AmbientIntensity;

float4 DiffuseColor;
float DiffuseIntensity;

float4 SpecularColor;
float SpecularIntensity;
float Shininess;

// Bump Mapping Uniforms
float height = 1.0f;
float2 UVScale = float2(1.0f, 1.0f);

texture normalMap;

sampler tsampler1 = sampler_state {
	texture = <normalMap>;
	magfilter = LINEAR; // None, POINT, LINEAR, Anisotropic
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap; // Clamp, Mirror, MirrorOnce, Wrap, Border
	AddressV = Wrap;
};

bool useSelfShadowing = false;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float4 Tangent : TANGENT0;
	float4 Binormal : BINORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 Tangent : TEXCOORD1;
	float3 Binormal : TEXCOORD2;
	float2 TexCoord : TEXCOORD3;
	float3 Position3D : TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	// TODO: add your vertex shader code here.
	/* // to transform from object space to tangent space
	float3x3 objectToTangentSpace;
	objectToTangentSpace[0] = input.Tangent;
	objectToTangentSpace[1] = input.Binormal;
	objectToTangentSpace[2] = input.Normal;
	// [ Tx, Ty, Tz ] [ Objx ] = [ Tanx ]
	// [ Bx, By, Bz ] [ Objy ] = [ Tany ]
	// [ Nx, Ny, Nz ] [ Objz ] = [ Tanz ]

	output.Normal = mul(objectToTangentSpace, input.Normal); // object -> tangent?
	output.Tangent = mul(objectToTangentSpace, input.Tangent);
	output.Binormal = mul(objectToTangentSpace, input.Binormal);
	//*/

	// World Space looks better IMO
	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
	output.Tangent = normalize(mul(input.Tangent, WorldInverseTranspose).xyz);
	output.Binormal = normalize(mul(input.Binormal, WorldInverseTranspose).xyz);

	output.Position3D = worldPosition.xyz;
	output.TexCoord = input.TexCoord;//* UVScale;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

    float3 N = input.Normal;
	float3 V = normalize(CameraPosition - input.Position3D.xyz);
    float3 L = normalize(LightPosition - input.Position3D.xyz);
    float3 R = reflect(-L, N);
	
    float4 color = tex2D(tsampler1, input.TexCoord);
    return color;
	
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}
