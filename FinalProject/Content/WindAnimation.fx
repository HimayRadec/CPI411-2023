float4x4 World;
float4x4 View;
float4x4 Projection;

float windActive;

// See the following links:
// http://http.developer.nvidia.com/GPUGems3/gpugems3_ch16.html 

texture Texture : register(t0);
sampler TheSampler : register(s0) = sampler_state
{
    Texture = <Texture>;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
    float3 Normal : TEXCOORD1;
};

float4 SmoothCurve(float4 x)
{
    return x * x * (3.0 - 2.0 * x);
}

float4 TriangleWave(float4 x)
{
    return abs(frac(x + 0.5) * 2.0 - 1.0);
}

float4 SmoothTriangleWave(float4 x)
{
    return SmoothCurve(TriangleWave(x));
}

// The suggested frequencies  
#define SIDE_TO_SIDE_FREQ1 1.975
#define SIDE_TO_SIDE_FREQ2 0.793
#define UP_AND_DOWN_FREQ1 0.375
#define UP_AND_DOWN_FREQ2 0.193

void ApplyDetailBending(
	inout float3 vPos, // The final world position of the vertex being modified
	float3 vNormal, // The world normal for this vertex
	float3 objectPosition, // The world position of the plant instance (same for all vertices)
	float fDetailPhase, // Optional phase for side-to-side. This is used to vary the phase for side-to-side motion
	float fBranchPhase, // The green vertex channel per Crytek's convention
	float fTime, // Ever-increasing time value (e.g. seconds ellapsed)
	float fEdgeAtten, // "Leaf stiffness", red vertex channel per Crytek's convention
	float fBranchAtten, // "Overall stiffness", *inverse* of blue channel per Crytek's convention
	float fBranchAmp, // Controls how much up and down
	float fSpeed, // Controls how quickly the leaf oscillates
	float fDetailFreq, // Same thing as fSpeed (they could really be combined, but I suspect
							// this could be used to let you additionally control the speed per vertex).
	float fDetailAmp)		// Controls how much back and forth
{
	// Phases (object, vertex, branch) 
    float fObjPhase = dot(objectPosition.xyz, 1);

	// fBranchPhase is always zero, but if you want you could somehow supply a different phase for each branch.
    fBranchPhase += fObjPhase;

	// Detail phase is controlled by the GREEN vertex color. 
    float fVtxPhase = dot(vPos.xyz, fDetailPhase + fBranchPhase);

    float2 vWavesIn = fTime + float2(fVtxPhase, fBranchPhase);
    float4 vWaves = (frac(vWavesIn.xxyy * float4(SIDE_TO_SIDE_FREQ1, SIDE_TO_SIDE_FREQ2, UP_AND_DOWN_FREQ1, UP_AND_DOWN_FREQ2)) * 2.0 - 1.0) * fSpeed * fDetailFreq;
    vWaves = SmoothTriangleWave(vWaves);
    float2 vWavesSum = vWaves.xz + vWaves.yw;

	// -fBranchAtten is how restricted this vertex of the leaf/branch is. e.g. close to the stem
	//  it should be 0 (maximum stiffness). At the far outer edge it might be 1.
	//  In this sample, this is controlled by the blue vertex color.
	// -fEdgeAtten controls movement in the plane of the leaf/branch. It is controlled by the
	//  red vertex color in this sample. It is supposed to represent "leaf stiffness". Generally, it
	//  should be 0 in the middle of the leaf (maximum stiffness), and 1 on the outer edges. 
    vPos.xzy += vWavesSum.xxy * float3(fEdgeAtten * fDetailAmp * vNormal.xy, fBranchAtten * fBranchAmp);
}

// This bends the entire plant in the direction of the wind.
// vPos:		The world position of the plant *relative* to the base of the plant.
//				(That means we assume the base is at (0, 0, 0). Ensure this before calling this function).
// vWind:		The current direction and strength of the wind.
// fBendScale:	How much this plant is affected by the wind.
void ApplyMainBending(inout float3 vPos, float2 vWind, float fBendScale)
{
	// Calculate the length from the ground, since we'll need it.
    float fLength = length(vPos);
	// Bend factor - Wind variation is done on the CPU.  
    float fBF = vPos.y * fBendScale;
	// Smooth bending factor and increase its nearby height limit.  
    fBF += 1.0;
    fBF *= fBF;
    fBF = fBF * fBF - fBF;
	// Displace position  
    float3 vNewPos = vPos;
    vNewPos.xz += vWind.xy * fBF;
	// Rescale - this keeps the plant parts from "stretching" by shortening the y (height) while
	// they move about the xz.
    vPos.xyz = normalize(vNewPos.xyz) * fLength;
}

// This is the wind direction and magnitude on the horizontal plane.
float2 WindSpeed;

// This needs to keep increasing.  
float Time;

// This describes how much the overall plant bends due to the wind.
float BendScale = 0.01;

// This describes how much the overall leaf/branch oscillates in the up-and-down direction.
float BranchAmplitude = 0.05;

// This describes how much the overall leaf/branch oscillates side-to-side.
float DetailAmplitude = 0.05;

bool InvertNormal = false;

VertexShaderOutput WindAnimationVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float3 vPos = worldPosition.xyz;

	// Grab the object position from the translation part of the world matrix.
	// We need the object position because we want to temporarily translate the vertex
	// back to the position it would be if the plant were at (0, 0, 0).
	// This is necessary for the main bending to work.
    float3 objectPosition = float3(World._m30, World._m31, World._m32);
    vPos -= objectPosition; // Reset the vertex to base-zero
    ApplyMainBending(vPos, WindSpeed, BendScale);
    vPos += objectPosition; // Restore it.

    float windStrength = length(WindSpeed);

    ApplyDetailBending(
		vPos,
		input.Normal,
		objectPosition,
		0, // Leaf phase - not used in this scenario, but would allow for variation in side-to-side motion
		input.Color.g, // Branch phase - should be the same for all verts in a leaf/branch.
		Time,
		input.Color.r, // edge attenuation, leaf stiffness
		input.Color.b, // branch attenuation. low values close to stem, high values furthest from stem.
		BranchAmplitude * windStrength, // branch amplitude. Play with this until it looks good.
		2, // Speed. Play with this until it looks good.
		1, // Detail frequency. Keep this at 1 unless you want to have different per-leaf frequency
		DetailAmplitude * windStrength // Detail amplitude. Play with this until it looks good.
		);
		
    float4 viewPosition = mul(float4(vPos, worldPosition.w), View);
    output.Position = mul(viewPosition, Projection);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    output.Normal = normalize(mul(input.Normal, World));
    if (InvertNormal)
    {
        output.Normal = -output.Normal;
    }
    return output;
}

#define FAKE_LIGHT float3(0.5744, 0.5744, 0.5744)

bool displayRed, displayGreen, displayBlue, displayAlpha;
float red, green, blue, alpha;

float4 WindAnimationPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 value = float4(input.Color.rgb, 1.0);

	// Lighting
    float brightNess = dot(input.Normal, float3(0.5744, 0.5744, 0.5744));
    brightNess = brightNess * 0.4 + 0.6;
    value.rgb *= brightNess;

    clip(value.a - 0.5); // Alpha test for leaf outlines.

    red = displayRed ? value.r : 0;
    green = displayGreen ? value.g : 0;
    blue = displayBlue ? value.b : 0;
    alpha = displayAlpha ? value.a : 0;

    
    // Display RGBA values as color
    value.rgba = float4(red, green, blue, alpha);
    return value;
}


technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 WindAnimationVertexShaderFunction();
        PixelShader = compile ps_4_0 WindAnimationPixelShaderFunction();
    }
}
