#ifndef CVGLighting
#define CVGLighting

float3 normalFromColor(float4 colorVal){
    #if defined(UNITY_NO_DXT5mn)
        return colorVal * 2 - 1;
    #else 
        // R => x => A
        // G => y
        // B =? z => ignored
        float3 normalVal;
        normalVal = float3(colorVal.a * 2.0 - 1,
                            colorVal.g * 2.0 - 1.0,
                            0.0 );
        normalVal.z = sqrt(1.0 - dot(normalVal, normalVal));
        return normalVal;
    #endif
}

float3 WorldNormalFromNormalMap(sampler2D normalMap, float2 normalTexCoord,float3 tangentWorld, float3 binormalWorld, float3 normalWorld){
    // Color at Pixel which we read from Tangent space normal map
    float4 colorAtPixel = tex2D(normalMap, normalTexCoord);
    // Normal value converted from Color value
    fixed3 normalAtPixed = normalFromColor(colorAtPixel);
    // Compose TBN matrix
    float3x3 TBNWorld = float3x3(tangentWorld, binormalWorld, normalWorld);
    //float3x3 TBNWorld = float3x3(i.tangentWorld.xyz, float3(1, 1, 1), float3(1, 1, 1));
    float3 worldNormalAtPixel = normalize(mul(normalAtPixed, TBNWorld));
    return worldNormalAtPixel;
}

float3 DiffuseLambert(float3 normalVal, float3 lightDir,float3 lightColor, float diffuseFactor, float attenuation){
    return lightColor * diffuseFactor * attenuation * max(0, dot(normalVal, lightDir));
}

float3 SpecularBlinnPhong(float3 normalDir, float3 lightDir, float3 worldSpaceViewDir, float3 specularColor,float specularFactor, float attenuation, float specularPower){
    float3 halfwayDir = normalize(lightDir + worldSpaceViewDir);
    return specularColor * specularFactor * attenuation * pow(max(0, dot(normalDir, halfwayDir)), specularPower);
}

#endif