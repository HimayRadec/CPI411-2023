float4x4 World;
float4x4 View;
float4x4 Projection;
// This is the wind direction and magnitude on the horizontal plane.
float2 WindSpeed;

// This needs to keep increasing. The functions we use to simulate sine/cos
// waves don't have a distinct period, so we can't loop time back to a certain
// point to avoid floating point precision issues at large values. One could take
// advantage of lulls in the wind to flip this back to zero, if desired.
float Time;

// This describes how much the overall plant bends due to the wind.
float BendScale = 0.01;

// This describes how much the overall leaf/branch oscillates in the up-and-down direction.
float BranchAmplitude = 0.05;

// This describes how much the overall leaf/branch oscillates side-to-side.
float DetailAmplitude = 0.05;

bool InvertNormal = false;

#define SIDE_TO_SIDE_FREQ1 1.975
#define SIDE_TO_SIDE_FREQ2 0.793
#define UP_AND_DOWN_FREQ1 0.375
#define UP_AND_DOWN_FREQ2 0.193



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

// Functions Used for Wave Generation
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
	// fObjPhase: This ensures phase is different for different plant instances, but it should be
	// the same value for all vertices of the same plant.
    float fObjPhase = dot(objectPosition.xyz, 1);

	// In this sample fBranchPhase is always zero, but if you want you could somehow supply a
	// different phase for each branch.
    fBranchPhase += fObjPhase;

	// Detail phase is (in this sample) controlled by the GREEN vertex color. In your modelling program,
	// assign the same "random" phase color to each vertex in a single leaf/branch so that the whole leaf/branch
	// moves together.
    float fVtxPhase = dot(vPos.xyz, fDetailPhase + fBranchPhase);

    float2 vWavesIn = fTime + float2(fVtxPhase, fBranchPhase);
    float4 vWaves = (frac(vWavesIn.xxyy *
					   float4(SIDE_TO_SIDE_FREQ1, SIDE_TO_SIDE_FREQ2, UP_AND_DOWN_FREQ1, UP_AND_DOWN_FREQ2)) *
					   2.0 - 1.0) * fSpeed * fDetailFreq;
    vWaves = SmoothTriangleWave(vWaves);
    float2 vWavesSum = vWaves.xz + vWaves.yw;

	// -fBranchAtten is how restricted this vertex of the leaf/branch is. e.g. close to the stem
	//  it should be 0 (maximum stiffness). At the far outer edge it might be 1.
	//  In this sample, this is controlled by the blue vertex color.
	// -fEdgeAtten controls movement in the plane of the leaf/branch. It is controlled by the
	//  red vertex color in this sample. It is supposed to represent "leaf stiffness". Generally, it
	//  should be 0 in the middle of the leaf (maximum stiffness), and 1 on the outer edges.
	// -Note that this is different from the Crytek code, in that we use vPos.xzy instead of vPos.xyz,
	//  because I treat y as the up-and-down direction.
    vPos.xzy += vWavesSum.xxy * float3(fEdgeAtten * fDetailAmp *
							vNormal.xy, fBranchAtten * fBranchAmp);
}

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

VertexShaderOutput MyVertexShader(VertexShaderInput input)
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
    output.Normal = normalize(mul(input.Normal, World));

    float windStrength = length(WindSpeed);

    ApplyDetailBending(
		vPos,
		output.Normal,
		objectPosition,
		0, // Leaf phase - not used in this scenario, but would allow for variation in side-to-side motion
		input.Color.g, // Branch phase - should be the same for all verts in a leaf/branch.
		Time,
		input.Color.r, // edge attenuation, leaf stiffness
		1 - input.Color.b, // branch attenuation. High values close to stem, low values furthest from stem.
							// For some reason, Crysis uses solid blue for non-moving, and black for most movement.
							// So we invert the blue value here.
		BranchAmplitude * windStrength, // branch amplitude. Play with this until it looks good.
		2, // Speed. Play with this until it looks good.
		1, // Detail frequency. Keep this at 1 unless you want to have different per-leaf frequency
		DetailAmplitude * windStrength // Detail amplitude. Play with this until it looks good.
		);

    float4 viewPosition = mul(float4(vPos, worldPosition.w), View);
    output.Position = mul(viewPosition, Projection);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    if (InvertNormal)
    {
        output.Normal = -output.Normal;
    }
    return output;
}

float4 MyPixelShader(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

technique BasicColorDrawing
{
	pass P0
	{
        VertexShader = compile vs_4_0 MyVertexShader();
        PixelShader = compile ps_4_0 MyPixelShader();
    }
};