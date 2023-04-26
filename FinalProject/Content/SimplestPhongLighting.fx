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
	return color.r;
}
technique Phong
{
	pass pass1
	{
		VertexShader = compile vs_4_0 PhongVertexShaderFunction();
		PixelShader = compile ps_4_0 PhongPixelShaderFunction();
	}
};

// See Red
struct VS_INPUT
{
    float4 position : POSITION;
    //float4 Normal : NORMAL;
    float4 color : COLOR;
};

struct VS_OUTPUT
{
    float4 pos : SV_POSITION;
    float4 color : COLOR0;
    //float4 Normal : TEXCOORD0;
    //float4 WorldPosition : TEXCOORD1;
};

VS_OUTPUT RVSMain(VS_INPUT input)
{
    VS_OUTPUT output;
    output.pos = input.position;
    output.color = input.color;
    return output;
}

float4 RPSMain(VS_OUTPUT input) : SV_Target
{
    return float4(input.color.r, 0, 0, 1); // output only the red component of the color
}

technique R
{
    pass pass1
    {
        VertexShader = compile vs_4_0 RVSMain();
        PixelShader = compile ps_4_0 RPSMain();
    }
};

// See Green
VS_OUTPUT GVSMain(VS_INPUT input)
{
    VS_OUTPUT output;
    output.pos = input.position;
    output.color = input.color;
    return output;
}

float4 GPSMain(VS_OUTPUT input) : SV_Target
{
    return float4(0, input.color.g, 0, 1); // output only the red component of the color
}

technique G
{
    pass pass1
    {
        VertexShader = compile vs_4_0 GVSMain();
        PixelShader = compile ps_4_0 GPSMain();
    }
};

// See Blue
VS_OUTPUT BVSMain(VS_INPUT input)
{
    VS_OUTPUT output;
    output.pos = input.position;
    output.color = input.color;
    return output;
}

float4 BPSMain(VS_OUTPUT input) : SV_Target
{
    return float4(0, 0, input.color.b, 1); // output only the red component of the color
}

technique B
{
    pass pass1
    {
        VertexShader = compile vs_4_0 BVSMain();
        PixelShader = compile ps_4_0 BPSMain();
    }
};

