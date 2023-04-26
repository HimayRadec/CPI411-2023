// Lab02
float3 offset;

// Lab03
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 AmbientColor;
float AmbientIntensity;

float4 DiffuseColor;
float DiffuseIntensity;

// Lab04
float Shininess;
float4 SpecularColor;
float SpecularIntensity = 1;

float3 CameraPosition;
float3 LightPosition;


struct VertexInput
{
	float4 Position : POSITION;
	float4 Normal: NORMAL;
};

struct VertexShaderOutput
{
	float4 Position : POSITION;
	float4 Color : COLOR;
	float4 Normal : TEXCOORD0;
	float4 WorldPosition: TEXCOORD1;

}; 

// PHONG PIXEL SHADER
VertexShaderOutput PhongVertexShaderFunction(VertexInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	output.Color = 0;

	return output;
}
float4 PhongPixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	float3 N = normalize(input.Normal.xyz);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 L = normalize(LightPosition);
	float3 R = reflect(-L, N);

	float4 ambient = AmbientColor * AmbientIntensity;
	float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
	float4 specular = pow(max(0, dot(V, R)), Shininess) * SpecularColor * SpecularIntensity;
	float4 color = saturate(ambient + diffuse + specular);
	
	
	color.a = 1;
	return color;
}
technique Phong
{
	pass pass1
	{
		VertexShader = compile vs_4_0 PhongVertexShaderFunction();
		PixelShader = compile ps_4_0 PhongPixelShaderFunction();
	}
}; 

// RED
struct RVS_INPUT
{
    float4 position : POSITION;
    float4 color : COLOR;
};

struct RVS_OUTPUT
{
    float4 pos : SV_POSITION;
    float4 color : COLOR0;
};

RVS_OUTPUT RVSMain(RVS_INPUT input)
{
    RVS_OUTPUT output;
    output.pos = input.position;
    output.color = input.color;
    return output;
}

float4 RPSMain(RVS_OUTPUT input) : SV_Target
{
    return float4(input.color.r, 0, 0, 1); // output only the red component of the color
}

technique RValue
{
    pass pass1
    {
        VertexShader = compile vs_4_0 RVSMain();
        PixelShader = compile ps_4_0 RPSMain();
    }
}

// GREEN
struct GVS_INPUT
{
    float4 position : POSITION;
    float4 color : COLOR;
};

struct GVS_OUTPUT
{
    float4 pos : SV_POSITION;
    float4 color : COLOR0;
};

GVS_OUTPUT GVSMain(GVS_INPUT input)
{
    GVS_OUTPUT output;
    output.pos = input.position;
    output.color = input.color;
    return output;
}

float4 GPSMain(GVS_OUTPUT input) : SV_Target
{
    return float4(0, input.color.g, 0, 1); // output only the red component of the color
}

technique GValue
{
    pass pass1
    {
        VertexShader = compile vs_4_0 GVSMain();
        PixelShader = compile ps_4_0 GPSMain();
    }
}

// BLUE 

RVS_OUTPUT BVSMain(RVS_INPUT input)
{
    RVS_OUTPUT output;
    output.pos = input.position;
    output.color = input.color;
    return output;
}

float4 BPSMain(RVS_OUTPUT input) : SV_Target
{
    return float4(0, 0, input.color.b, 1); // output only the red component of the color
}

technique RValue
{
    pass pass1
    {
        VertexShader = compile vs_4_0 BVSMain();
        PixelShader = compile ps_4_0 BPSMain();
    }
}
