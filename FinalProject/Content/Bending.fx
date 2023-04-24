matrix WorldViewProjection;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	// pass in worldPos?
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
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
// DETAIL BENDING
// Leave bending is done by deforming the edges using the vertex red value for edge stiffness
// done along world space using the winds xy direction

// per leaf is done by deforming along the z axis using the blue value 

// to give each leaf its own phase variation we use the g value

// Phases (object, vertex, branch)


// MAIN BENDING
// main vegetation bends by displacing vertices' xy positions along the wind direction, 
// using normalized height to scale the amount of deformation

// computing the vertex's distance to the mesh center and using this distance for rescaling the new displaced normalized position ????

VertexShaderOutput MyVertexShader(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	
	// DETAILED BENDING
    float fObjPhase = dot(worldPos.xyz, 1);
    fBranchPhase += fObjPhase; // what is fBranchPhase (main vegetation)?
	//  x is used for edges; y is used for branches
    float fVtxPhase = dot(vPos.xyz, fDetailPhase + fBranchPhase); // what is fDetailPhase
	
    float2 vWavesIn = fTime + float2(fVtxPhase, fBranchPhase); // 1.975, 0.793, 0.375, 0.193 are good frequencies
    float4 vWaves = (frac(vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193)) * 2.0 - 1.0) * fSpeed * fDetailFreq;
    vWaves = SmoothTriangleWave(vWaves);
    float2 vWavesSum = vWaves.xz + vWaves.yw; // Edge (xy) and branch bending (z)
    vPos.xyz += vWavesSum.xxy * float3(fEdgeAtten * fDetailAmp * vNormal.xy, fBranchAtten * fBranchAmp);
	
	
	
	// MAIN BENDING
    float fBF = vPos.z * fBendScale; // Smooth bending factor and increase its nearby height limit.
    fBF += 1.0;
    fBF *= fBF;
    fBF = fBF * fBF - fBF; // Displace position
    float3 vNewPos = vPos;
    vNewPos.xy += vWind.xy * fBF; // Rescale 
	vPos.xyz = normalize(vNewPos.xyz)* fLength;
	

	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = input.Color;

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