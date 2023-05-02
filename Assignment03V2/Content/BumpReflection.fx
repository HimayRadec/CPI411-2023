float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;
float3 LightPosition;

float4 DiffuseColor;
float DiffuseIntensity;

float4 SpecularColor;
float SpecularColorIntensity;
float Shininess;


float NormalMapRepeatU;
float NormalMapRepeatV;
int SelfShadow;
float BumpHeight;
int NormalizeTangentFrame;
int NormalizeNormalMap;
int MipMap;

texture normapMap;
samplerCUBE SkyBoxSampler = sampler_state
{
    texture = <environmentMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

sampler NormalMapSamplerLinear = sampler_state
{
    Texture = <normalMap>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler NormalMapSamplerNone = sampler_state
{
    Texture = <normalMap>;
    MinFilter = none;
    MagFilter = none;
    MipFilter = none;
    AddressU = Wrap;
    AddressV = Wrap;
};

// expand (0-1) D3DCOLORtoUBYTE4 (-1,1)
float3 Expand(float3 v)
{
    return (v - 0.5) * 2;
}
// inverse of Expand 
float3 InverseExpand(float3 v)
{
    return (v + 1) / 2;
}

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float3 Tangent : TANGENT0;
    float3 Binormal : BINORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float3 Tangent : TANGENT0;
    float3 Binormal : BINORMAL0;
    float2 TexCoord : TEXCOORD0;
    float3 Position3D : TEXCOORD4; // ???????????????
    
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    
    output.Position = mul(viewPosition, Projection);
    output.Normal = mul(input.Normal, WorldInverseTranspose).xyz;
    output.Tangent = mul(input.Tangent, WorldInverseTranspose).xyz;
    output.Binormal = mul(input.Binormal, WorldInverseTranspose).xyz;
    
    output.TexCoord = input.TexCoord;
    output.TexCoord.xy *= float2(NormalMapRepeatU, NormalMapRepeatV);
    output.Position3D = worldPosition.xyz; // ???????????????
    
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 L = normalize(LightPosition - input.Position3D); // ?????????????
    float3 V = normalize(CameraPosition - input.Position3D); // ??????????????
    float3 N = normalize(input.Normal);
    float3 T = normalize(input.Tangent);
    float3 B = normalize(input.Binormal);
    float3 H = normalize(L + V);
    float3 I = normalize(input.Position3D - CameraPosition);
    
    float3 normalTex = tex2D(NormalMapSamplerLinear, input.TexCoord).xyz; // input.NormalTexCoord ??????
    normalTex = 2 * (normalTex - 0.5);
    normalTex.x *= (1 + 0.2 * (BumpHeight - 5));
    normalTex.y *= (1 + 0.2 * (BumpHeight - 5));
    normalTex.z *= (1 + 0.2 * (5 - BumpHeight));
    
    float3 bumpNormal = mul(normalTex, float3x3(T, B, N));
    float3 R = reflect(I, bumpNormal); // Could be T instead of I ????
    float4 reflectedColor = texCUBE(SkyBoxSampler, R);
    return reflectedColor;

}
technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}
