﻿// Lab02
float3 offset;

// Lab03
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float4 AmbientColor;
float AmbientIntensity;
float3 DiffuseLightDirection;
float4 DiffuseColor;
float DiffuseIntensity;



struct VertexInput
{
	float4 Position : POSITION;
	float4 Normal: NORMAL;
};

struct VertexOutput
{
	float4 Position : POSITION;
	float4 Color : COLOR;
};

// ?? Difference between pixel and vertex shader ??
VertexOutput MyVertexShader(VertexInput input)
{
	VertexOutput output;
	float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, View);
	output.Position = mul(viewPos, Projection);

	// ?? What is the mul function ??
	float4 normal = mul(input.Normal, WorldInverseTranspose);
	float lightIntensity = dot(normal.xyz, DiffuseLightDirection);
	output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);

	return output;
}

float4 MyPixelShader(VertexOutput input) : COLOR
{
	return saturate(input.Color + AmbientColor * AmbientIntensity);
}

technique MyTechnique
{
	pass pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShader();
	}
};