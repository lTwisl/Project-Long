// VolumetricFogHelpers.hlsl
#ifndef VOLUMETRIC_FOG_HELPERS_HLSL
#define VOLUMETRIC_FOG_HELPERS_HLSL

float HenyeyGreenstein(float cosTheta, float g)
{
    return (1 - g * g) / (4 * PI * pow(1 + g * g - 2 * g * cosTheta, 1.5));
}

float GetHeightDensity(float3 worldPos, float baseHeight, float falloff)
{
    float height = worldPos.y - baseHeight;
    return exp(-falloff * max(0, height));
}

#endif