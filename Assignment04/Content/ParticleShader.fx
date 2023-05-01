float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 InverseCamera;
texture2D Texture;

sampler ParticleSampler : register(s0) = sampler_state
{
    Texture = <Texture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 ParticlePosition : POSITION1;
    float4 ParticleParameter : POSITION2;
};
struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput ParticleVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, InverseCamera);
    worldPosition.xyz = worldPosition.xyz * sqrt(input.ParticleParameter.x / input.ParticleParameter.y);
    worldPosition += input.ParticlePosition;
    
    output.Position = mul(mul(mul(worldPosition, World), View), Projection);
    output.TexCoord = input.TexCoord;
    output.Color = 1 - input.ParticleParameter.x / input.ParticleParameter.y;
    
    return output;
}

float4 ParticlePixelShader(VertexShaderOutput input) : COLOR 
{
    float4 color = tex2D(ParticleSampler, input.TexCoord);
    color *= input.Color;
    return color;
}

technique Technique1
{
    pass particle
    {
        VertexShader = compile vs_4_0 ParticleVertexShader();
        PixelShader = compile ps_4_0 ParticlePixelShader();

    }
}
        