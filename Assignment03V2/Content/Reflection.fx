float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;

texture decalMap;
texture environmentMap;

float reflectivity;


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

struct ReflectionVertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};
struct ReflectionVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL0;
    float3 WorldPosition : TEXCOORD1;
};
ReflectionVertexShaderOutput ReflectionVertexShaderFunction(ReflectionVertexShaderInput input)
{
    ReflectionVertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.TexCoord = input.TexCoord;
    output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
    output.WorldPosition = worldPosition.xyz;
    
    
    // float3 N = mul(input.normal, worldPosition);
    // float3 I = normalize(worldPosition.xyz - CameraPosition);
    // output.R = reflect(I, normalize(N)); 

    return output;
}
float4 ReflectionPixelShaderFunction(ReflectionVertexShaderOutput input) : COLOR0
{
    float3 I = normalize(input.WorldPosition - CameraPosition);
    float3 R = reflect(I, normalize(input.Normal));
    
    float4 reflectedColor = texCUBE(SkyBoxSampler, R);
    float4 decalColor = tex2D(tsampler1, input.TexCoord);
    return lerp(decalColor, reflectedColor, reflectivity);
}
technique reflection
{
    pass pass1
    {
        VertexShader = compile vs_4_0 ReflectionVertexShaderFunction();
        PixelShader = compile ps_4_0 ReflectionPixelShaderFunction();
    }
};