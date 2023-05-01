float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;


float NormalMapRepeatU;
float NormalMapRepeatV;
int SelfShadow;
float BumpHeight;
int NormalizeTangentFrame;
int NormalizeNormalMap;
int MipMap;

texture normapMap;

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
    
    return output;
}

float4 PixelShaderFuntion(VertexShaderOutput input) : COLOR0
{
    float2 texCoord = input.TexCoord;
    
    float3 normalTex = tex2D(NormalMapSamplerLinear, texCoord).rgb;
    if (MipMap == 0) normalTex = tex2D(NormalMapSamplerNone, texCoord).rgb;
    
    normalTex = Expand(normalTex);
    normalTex.x *= (1 + 0.2 * (BumpHeight - 5));
    normalTex.y *= (1 + 0.2 * (BumpHeight - 5));
    normalTex.z *= (1 + 0.2 * (5 - BumpHeight));
    
    float3x3 TangentToWorld;
    TangentToWorld[0] = (input.Tangent);
    TangentToWorld[1] = (input.Binormal);
    TangentToWorld[2] = (input.Normal);
    float3 bumpNormal = mul(normalTex, TangentToWorld);
    
    if (NormalizeNormalMap > 0) bumpNormal = normalize(bumpNormal);
    return float4(bumpNormal, 1.0);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();

    }
}