float NormalMapRepeatU;
float NormalMapRepeatV;
int SelfShadow;
float BumpHeight;
int NormalizeTangentFrame;
int NormalizeNormalMap;
int MipMap;
texture normalMap;


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

// expand (0-1)  (-1,1)
float3 Expand(float3 v)
{
    return (v - 0.5) * 2;
}
// inverse of Expand 
float3 InverseExpand(float3 v)
{
    return (v + 1) / 2;
}

float4 PixelShaderFunction(float4 input : TEXCOORD0) : COLOR0
{
    float2 texCoord = input.xy;
    texCoord.x *= NormalMapRepeatU;
    texCoord.y *= NormalMapRepeatV;
    float3 n = tex2D(NormalMapSamplerLinear, texCoord).rgb;
    if (MipMap == 1) n = tex2D(NormalMapSamplerNone, texCoord).rgb;
    
    n = Expand(n);
    n.x *= (1 + 0.2 * (BumpHeight - 5));
    n.y *= (1 + 0.2 * (BumpHeight - 5));
    n.z *= (1 + 0.2 * (5 - BumpHeight));
    if (NormalizeNormalMap > 0) n = normalize(n);
    return float4(InverseExpand(n), 1.0); 

}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_4_0 PixelShaderFunction();

    }
}