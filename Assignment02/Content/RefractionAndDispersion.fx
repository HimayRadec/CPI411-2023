float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;

texture decalMap;
texture environmentMap;

float refractivity = 1.0f;
float3 etaRatio = float3(0.5f, 0.5f, 0.5f);

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

struct RefractionDispersionVertexShaderInput
{
    float4 Position : POSITION0;
    float2 texCoord : TEXCOORD0;
    float3 normal : NORMAL0;
};
struct RefractionDispersionVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 texCoord : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
};
RefractionDispersionVertexShaderOutput RefractionDispersionVertexShaderFunction(RefractionDispersionVertexShaderInput input)
{
    RefractionDispersionVertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    
    output.Position = mul(viewPosition, Projection);
    output.texCoord = input.texCoord;
    output.WorldPosition = worldPosition;
    output.Normal = normalize(mul(input.normal, WorldInverseTranspose));
    
    return output;
}
float4 RefractionDispersionPixelShaderFunction(RefractionDispersionVertexShaderOutput input) : COLOR0
{
    float3 I = normalize(input.WorldPosition - CameraPosition);
    float3 R = refract(I, input.Normal, etaRatio.x);
    float3 G = refract(I, input.Normal, etaRatio.y);
    float3 B = refract(I, input.Normal, etaRatio.x);
    
    float4 refractedColor;
    refractedColor.r = texCUBE(SkyBoxSampler, R).r;
    refractedColor.g = texCUBE(SkyBoxSampler, R).g;
    refractedColor.b = texCUBE(SkyBoxSampler, R).b;
    refractedColor.a = 1;
    
    float4 decalColor = tex2D(tsampler1, input.texCoord);
    return lerp(decalColor, refractedColor, refractivity);
}
technique refraction
{
    pass pass1
    {
        VertexShader = compile vs_4_0 RefractionDispersionVertexShaderFunction();
        PixelShader = compile ps_4_0 RefractionDispersionPixelShaderFunction();
    }
};