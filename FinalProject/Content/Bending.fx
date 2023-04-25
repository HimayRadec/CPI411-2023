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

// Phases (object, vertex, branch)    
float fObjPhase = dot(worldPos.xyz, 1); 
fBranchPhase += fObjPhase; 
float fVtxPhase = dot(vPos.xyz, fDetailPhase + fBranchPhase); // x is used for edges; 
y is used for branches    
float2 vWavesIn = fTime + float2(fVtxPhase, fBranchPhase ); // 1.975, 0.793, 0.375, 0.193 are good frequencies    
float4 vWaves = (frac( vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193) ) * 2.0 - 1.0 ) * fSpeed * fDetailFreq; 
vWaves = SmoothTriangleWave( vWaves ); float2 vWavesSum = vWaves.xz + vWaves.yw; // Edge (xy) and branch bending (z) 
vPos.xyz += vWavesSum.xxy * float3(fEdgeAtten * fDetailAmp * vNormal.xy, fBranchAtten * fBranchAmp);

// Bend factor - Wind variation is done on the CPU.    
float fBF = vPos.z * fBendScale; // Smooth bending factor and increase its nearby height limit. 
fBF += 1.0; fBF *= fBF; fBF = fBF * fBF - fBF; // Displace position    
float3 vNewPos = vPos; vNewPos.xy += vWind.xy * fBF; // Rescale 
vPos.xyz = normalize(vNewPos.xyz)* fLength; 

half3 LeafShadingBack(half3 vEye, half3 vLight, half3 vNormal, half3 cDiffBackK, half fBackViewDep)
{
    half fEdotL = saturate(dot(vEye.xyz, -vLight.xyz));
    half fPowEdotL = fEdotL * fEdotL;
    fPowEdotL *= fPowEdotL; // Back diffuse shading, wrapped slightly    
    half fLdotNBack = saturate(dot(-vNormal.xyz, vLight.xyz)*0.6+0.4);   // Allow artists to tweak view dependency.    
    half3 vBackShading = lerp(fPowEdotL, fLdotNBack, fBackViewDep);   // Apply material back diffuse color.    
    return vBackShading * cDiffBackK.xyz; 
}

void LeafShadingFront(half3 vEye,half3 vLight,half3 vNormal,half3 cDifK,half4 cSpecK,out half3 outDif,out half3 outSpec) 
{   half fLdotN = saturate(dot(vNormal.xyz, vLight.xyz));   
    outDif = fLdotN * cDifK.xyz;   
    outSpec = Phong(vEye, vLight, cSpecK.w) * cSpecK.xyz; 
} 

void frag_custom_per_light(inout fragPass pPass, inout fragLightPass pLight)
{
    half3 cDiffuse = 0, cSpecular = 0;
    LeafShadingFront(pPass.vReflVec, pLight.vLight, pPass.vNormal.xyz, pLight.cDiffuse.xyz, pLight.cSpecular, cDiffuse, cSpecular); // Shadows * light falloff * light projected texture    
    half3 cK = pLight.fOcclShadow * pLight.fFallOff * pLight.cFilter;   // Accumulate results.   
    pPass.cDiffuseAcc += cDiffuse * cK;   
    pPass.cSpecularAcc += cSpecular * cK;   
    pPass.pCustom.fOcclShadowAcc += pLight.fOcclShadow; 
    } 
void frag_custom_ambient(inout fragPass pPass, inout half3 cAmbient) 
    {   // Hemisphere lighting approximation   
    cAmbient.xyz = lerp(cAmbient*0.5f, cAmbient,saturate(pPass.vNormal.z*0.5f+0.5f));   
    pPass.cAmbientAcc.xyz = cAmbient; 
    } 
void frag_custom_end(inout fragPass pPass, inout half3 cFinal) 
    {   if( pPass.nlightCount && pPass.pCustom.bLeaves ) 
        {     // Normalize shadow accumulation.    
            half fOccFactor = pPass.pCustom.fOcclShadowAcc/pPass.nlightCount;     // Apply subsurface map.     
            pPass.pCustom.cShadingBack.xyz *= pPass.pCustom.cBackDiffuseMap;     // Apply shadows and light projected texture.     
            pPass.pCustom.cShadingBack.xyz *= fOccFactor * pPass.pCustom.cFilterColor; 
        } // Apply diffuse texture and material diffuse color to    // ambient/diffuse/sss terms. 
        cFinal.xyz = (pPass.cAmbientAcc.xyz + pPass.cDiffuseAcc.xyz + pPass.pCustom.cShadingBack.xyz) * pPass.cDiffuseMap.xyz * MatDifColor.xyz; // Apply gloss map and material specular color, add to result. 
        cFinal.xyz += pPass.cSpecularAcc.xyz * pPass.cGlossMap.xyz * MatSpecColor.xyz; // Apply prebaked ambient occlusion term. 
        cFinal.xyz *= pPass.pCustom.fAmbientOcclusion; 
    } 