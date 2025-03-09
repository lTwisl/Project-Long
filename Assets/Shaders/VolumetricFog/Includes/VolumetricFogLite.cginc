#ifndef VOLUMETRIC_FOG_LITE_CGINC
#define VOLUMETRIC_FOG_LITE_CGINC

// ����������� ������� ��������� URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

// ========================================================
// ������� ���� ��������� ����-����������
// cosTheta - ������� ���� ����� ������������ ����� � ����������
// g2 - ��������� �������� �����������
// gPow2 - ������� ��������� �����������
// ========================================================
float HenyeyGreenstein(float cosTheta, float g2, float gPow2)
{
    float denominator = (1.0 + gPow2) - (g2 * cosTheta);
    return (1.0 - gPow2) / (4.0 * PI * pow(denominator, 1.5));
}

// ========================================================
// ������ ��������� �� ��������� ��������� �����
// ========================================================
float CalculateMainLightScattering(float cosTheta, float costThetaPow2, float g2, float gPow2)
{
    return HenyeyGreenstein(cosTheta, g2, gPow2);
}

// ========================================================
// ��������� ���� ��� ��������� (��������: Jorge Jimenez)
// ========================================================
float ignoise(float2 position, int frame)
{
    position += (float(frame) * 5.588238f);
    return frac(52.9829189f * frac(0.06711056f * float(position.x) + 0.00583715f * float(position.y)));
}

#define MAX_RAYMARCH_STEPS 128

// ========================================================
// �������� ������� ������
// ========================================================
void VolumetricFog_float(
    // ��������� �����
    float4 colour,
    
    // ������� � �������� ����������
    float3 position,
    float2 screenPosition,
    
    // ��������� ��������
    uint raymarchSteps,
    float raymarchNoise,
    float raymarchDistance,
    float raymarchDistanceBias,
    float3 raymarchDirection,
    float raymarchMaxDepth,
    
    // ��������� ��������� �����
    float mainLightStrength,
    float mainLightAnisotropy,
    float mainLightAnisotropyBlend,
    float mainLightShadowStrength,
    
    out float4 output)
{
#ifdef SHADERGRAPH_PREVIEW
    discard; // �������� ��� ������
#endif

    // ========================================================
    // ������������� ����������
    // ========================================================
    
    // ��������� ���� ��� ���������
    float random = frac((sin(dot(screenPosition, float2(12.9898, 78.233))) * 43758.55) + _Time.y);
    float interleavedGradientNoise = ignoise(screenPosition, random * 9999.0);
    
    // ������������� ��������� ��������
    raymarchDistance = min(raymarchDistance, raymarchMaxDepth + (raymarchDistance * raymarchDistanceBias));
    raymarchDistance = lerp(raymarchDistance, raymarchDistance * interleavedGradientNoise, raymarchNoise);
    
    // ������ ���� ��������
    float rayStep = raymarchDistance / raymarchSteps;
    float3 rayDirectionStep = raymarchDirection * rayStep;
    
    // ������������� ����������
    float density = 0.0;
    float depth = 0.0;
    float mainLightShading = 0.0;
    float raymarchStepsMinusOne = raymarchSteps - 1.0;

    // ========================================================
    // ���������� ������ ��������� �����
    // ========================================================
#ifdef _MAIN_LIGHT_ENABLED
    Light mainLight = GetMainLight();
    
    // ���������� ���������� ��� ���� ���������
    float g = mainLightAnisotropy;
    float g2 = g * 2.0;
    float gSquared = g * g;
    
    // ������� ���� ����� ������������ ����� � ��������
    float cosTheta = dot(mainLight.direction, raymarchDirection);
    float cosThetaSquared = cosTheta * cosTheta;
#endif

    // ========================================================
    // �������� ���� ��������
    // ========================================================
    for (int i = 0; i < MAX_RAYMARCH_STEPS; i++)
    {
        // ���������� ��� ���������� ������ �����
        if (i >= raymarchSteps)
            break;
        
        // ������������� ���������� ���� ��� ������� ��������� � raymarchMaxDepth
        if (i == raymarchSteps - 1)
        {
            rayStep = raymarchMaxDepth - depth;
            rayDirectionStep = raymarchDirection * rayStep;
        }
        
        // ����������� ����� ����� ����
        position += rayDirectionStep;
        depth += rayStep;
        
        // ========================================================
        // ������ ����� � ���������
        // ========================================================
        float mainLightShadow = 1.0;
        float mainLightScattering = 1.0;
#define _MAIN_LIGHT_SHADOWS_ENABLED
#ifdef _MAIN_LIGHT_ENABLED
#if defined(_MAIN_LIGHT_SHADOWS)
        // ����: �������������� ��������� � �������
        float4 shadowCoord = TransformWorldToShadowCoord(position);
        mainLightShadow = MainLightRealtimeShadow(shadowCoord);
        mainLightShadow = lerp(1.0, mainLightShadow, mainLightShadowStrength);
#endif
        
        // ������ ������������� ���������
        mainLightScattering = CalculateMainLightScattering(cosTheta, cosThetaSquared, g2, gSquared);
        mainLightScattering = lerp(1.0, mainLightScattering, mainLightAnisotropyBlend);
#endif
        
        // ���������� ������ ����� � ���������
        mainLightShading += mainLightShadow * mainLightScattering;
        density++;
        
        // ========================================================
        // ��������� �������������� ���������� �����
        // ========================================================
#ifdef _ADDITIONAL_LIGHTS
        int additionalLightsCount = GetAdditionalLightsCount();
        for (int j = 0; j < additionalLightsCount; ++j)
        {
            Light additionalLight = GetAdditionalLight(j, position);
            
            // ������ ����� � ����������
            float additionalShadow = additionalLight.shadowAttenuation;
            additionalShadow = lerp(1.0, additionalShadow, mainLightShadowStrength);
            
            // ������ ���������� ���������
            float additionalCosTheta = dot(additionalLight.direction, raymarchDirection);
            float additionalCosThetaSquared = additionalCosTheta * additionalCosTheta;
            float scattering = CalculateMainLightScattering(additionalCosTheta, additionalCosThetaSquared, g2, gSquared);
            scattering = lerp(1.0, scattering, mainLightAnisotropyBlend);
            
            // ������������ ������ �����
            float attenuation = additionalLight.distanceAttenuation * additionalShadow;
            colour.rgb += scattering * attenuation * additionalLight.color / raymarchSteps;
        }
#endif

        // �������� �� ���������� ������������ �������
        if (depth > raymarchMaxDepth)
            break;
    }

    // ========================================================
    // ������������� �����������
    // ========================================================
    
    // ������������ ���������
    density /= raymarchSteps;
    colour.a *= density;

#ifdef _MAIN_LIGHT_ENABLED
    // ����������� ������ ��������� �����
    mainLightShading /= raymarchSteps;
    mainLightShading *= mainLightStrength;
    
    // ���������� � ������ ������
    float3 mainLightColour = mainLight.color * mainLightShading;
    colour.rgb += mainLightColour;
#endif

    output = colour;
}

#endif

//#define _MAIN_LIGHT_SHADOWS_ENABLED