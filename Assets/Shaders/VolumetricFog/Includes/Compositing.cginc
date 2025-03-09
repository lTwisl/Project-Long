#ifndef COMPOSITING_CGINC
#define COMPOSITING_CGINC

// Включаем необходимые библиотеки URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

// Функция для получения глубины из текстуры глубины
float GetHalfDepth(float2 uv, Texture2D depthTexture)
{
    // Используем жестко заданный семплер
    float d = SAMPLE_TEXTURE2D_X(depthTexture, sampler_PointClamp, uv).r;
    return LinearEyeDepth(d, _ZBufferParams);
}

// Функция для получения линейной глубины
float GetLinearDepth(float2 uv)
{
    // Используем встроенную функцию URP для получения глубины
    float d = SampleSceneDepth(uv);
    return LinearEyeDepth(d, _ZBufferParams);
}

// Функция для преобразования глубины в линейное пространство (float4)
float4 LinearEyeDepth(float4 d)
{
    float4 linearEyeDepth = 0;
    
    linearEyeDepth.x = LinearEyeDepth(d.x, _ZBufferParams);
    linearEyeDepth.y = LinearEyeDepth(d.y, _ZBufferParams);
    linearEyeDepth.z = LinearEyeDepth(d.z, _ZBufferParams);
    linearEyeDepth.w = LinearEyeDepth(d.w, _ZBufferParams);
    
    return linearEyeDepth;
}

// Функция для обновления ближайшего сэмпла
void UpdateNearestSample(inout float minDistance, inout float2 nearestUV, float z, float2 uv, float zFull)
{
    float distance = abs(z - zFull);
    
    if (distance < minDistance)
    {
        minDistance = distance;
        nearestUV = uv;
    }
}

// Фильтр ближайшей глубины
void NearestDepthFilter_float(float2 uv, float2 texelSize, Texture2D fogTexture, Texture2D depthTexture, out float4 output)
{
    float zFull = GetLinearDepth(uv);
    const float depthThreshold = 0.5;
    
    float minDistance = 1e9;
    float2 nearestUV = uv;
    bool withinThreshold = true;

    // Поиск ближайшего сэмпла
    [unroll]
    for (int y = 0; y <= 1; ++y)
    {
        for (int x = 0; x <= 1; ++x)
        {
            float2 currentUV = uv + (float2(x, y) * texelSize) - (0.5 * texelSize);
            
            float z = GetHalfDepth(currentUV, depthTexture);
            float distance = abs(z - zFull);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestUV = currentUV;
            }

            if (distance >= depthThreshold)
            {
                withinThreshold = false;
            }
        }
    }
    
    float4 fogSample = 0.0;

    if (withinThreshold)
    {
        // Билинейная фильтрация для внутренних пикселей
        fogSample = SAMPLE_TEXTURE2D_X(fogTexture, sampler_LinearClamp, uv);
    }
    else
    {
        // Точечная фильтрация для краев
        fogSample = SAMPLE_TEXTURE2D_X(fogTexture, sampler_PointClamp, nearestUV);
    }

    // Гарантируем корректные значения альфа-канала
    output = fogSample;
}

#endif