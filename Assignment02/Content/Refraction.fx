float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;

texture decalMap;
texture environmentMap;


float refractivity;
float3 etaRatio;

sampler tsampler1 = sampler_state
{
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

struct RefractionVertexShaderInput
{
    float4 Position : POSITION0;
    float2 texCoord : TEXCOORD0;
    float3 normal : NORMAL0;
};
struct RefractionVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 texCoord : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
};
RefractionVertexShaderOutput RefractionVertexShaderFunction(RefractionVertexShaderInput input)
{
    RefractionVertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    
    output.Position = mul(viewPosition, Projection);
    output.texCoord = input.texCoord;
    output.WorldPosition = worldPosition;
    output.Normal = normalize(mul(input.normal, WorldInverseTranspose));
    
    return output;
}
float4 RefractionPixelShaderFunction(RefractionVertexShaderOutput input) : COLOR0
{
    float3 I = normalize(input.WorldPosition - CameraPosition);
    float3 R = refract(I, input.Normal, etaRatio.r);
    
    float4 refractedColor = texCUBE(SkyBoxSampler, R);
    float4 decalColor = tex2D(tsampler1, input.texCoord);
    return lerp(decalColor, refractedColor, refractivity);
}
technique refraction
{
    pass pass1
    {
        VertexShader = compile vs_4_0 RefractionVertexShaderFunction();
        PixelShader = compile ps_4_0 RefractionPixelShaderFunction();
    }
};