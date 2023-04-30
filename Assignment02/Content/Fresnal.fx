float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;

texture decalMap;
texture environmentMap;

float refractivity;
float reflectivity;
float3 etaRatio = float3(0.5f, 0.5f, 0.5f);
float3 fresnalTerm = float3(0.0f, 5.0f, 3.0f);

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

struct FresnalVertexShaderInput
{
    float4 Position : POSITION0;
    float2 texCoord : TEXCOORD0;
    float3 normal : NORMAL0;
};
struct FresnalVertexShaderOutput
{
    float4 Position : POSITION0;
    float2 texCoord : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
};
FresnalVertexShaderOutput FresnalVertexShaderFunction(FresnalVertexShaderInput input)
{
    FresnalVertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View); 
    
    output.Position = mul(viewPosition, Projection);
    output.texCoord = input.texCoord;
    output.WorldPosition = worldPosition;
    output.Normal = normalize(mul(input.normal, WorldInverseTranspose));
    
    return output;
}
float4 FresnalPixelShaderFunction(FresnalVertexShaderOutput input) : COLOR0
{
    float3 I = normalize(input.WorldPosition - CameraPosition);
    float3 R = refract(I, input.Normal, etaRatio.x);
    float3 G = refract(I, input.Normal, etaRatio.y);
    float3 B = refract(I, input.Normal, etaRatio.x);
    
    float3 Reflect = reflect(I, input.Normal);
    float coef = fresnalTerm.x + fresnalTerm.y * pow(1 + dot(I, input.Normal), fresnalTerm.z);
    float4 decalColor = tex2D(tsampler1, input.texCoord);
    float4 reflectedColor = texCUBE(SkyBoxSampler, Reflect);
    reflectedColor = lerp(decalColor, reflectedColor, reflectivity);
    float4 refractedColor;
    refractedColor.r = texCUBE(SkyBoxSampler, R).r;
    refractedColor.g = texCUBE(SkyBoxSampler, R).g;
    refractedColor.b = texCUBE(SkyBoxSampler, R).b;
    refractedColor.a = 1;
    
    return lerp(refractedColor, reflectedColor, coef);
}
technique refraction
{
    pass pass1
    {
        VertexShader = compile vs_4_0 FresnalVertexShaderFunction();
        PixelShader = compile ps_4_0 FresnalPixelShaderFunction();
    }
};