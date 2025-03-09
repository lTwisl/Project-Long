#ifndef VOLUMETRIC_FOG_LITE_CGINC
#define VOLUMETRIC_FOG_LITE_CGINC

// Подключение базовых библиотек URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

// ========================================================
// Функция фазы рассеяния Хени-Гринстейна
// cosTheta - косинус угла между направлением света и наблюдения
// g2 - удвоенный параметр анизотропии
// gPow2 - квадрат параметра анизотропии
// ========================================================
float HenyeyGreenstein(float cosTheta, float g2, float gPow2)
{
    float denominator = (1.0 + gPow2) - (g2 * cosTheta);
    return (1.0 - gPow2) / (4.0 * PI * pow(denominator, 1.5));
}

// ========================================================
// Расчет рассеяния от основного источника света
// ========================================================
float CalculateMainLightScattering(float cosTheta, float costThetaPow2, float g2, float gPow2)
{
    return HenyeyGreenstein(cosTheta, g2, gPow2);
}

// ========================================================
// Генерация шума для дизеринга (источник: Jorge Jimenez)
// ========================================================
float ignoise(float2 position, int frame)
{
    position += (float(frame) * 5.588238f);
    return frac(52.9829189f * frac(0.06711056f * float(position.x) + 0.00583715f * float(position.y)));
}

#define MAX_RAYMARCH_STEPS 128

// ========================================================
// ОСНОВНАЯ ФУНКЦИЯ ТУМАНА
// ========================================================
void VolumetricFog_float(
    // Параметры цвета
    float4 colour,
    
    // Позиция и экранные координаты
    float3 position,
    float2 screenPosition,
    
    // Параметры реймарча
    uint raymarchSteps,
    float raymarchNoise,
    float raymarchDistance,
    float raymarchDistanceBias,
    float3 raymarchDirection,
    float raymarchMaxDepth,
    
    // Настройки основного света
    float mainLightStrength,
    float mainLightAnisotropy,
    float mainLightAnisotropyBlend,
    float mainLightShadowStrength,
    
    out float4 output)
{
#ifdef SHADERGRAPH_PREVIEW
    discard; // Заглушка для превью
#endif

    // ========================================================
    // ИНИЦИАЛИЗАЦИЯ ПАРАМЕТРОВ
    // ========================================================
    
    // Генерация шума для дизеринга
    float random = frac((sin(dot(screenPosition, float2(12.9898, 78.233))) * 43758.55) + _Time.y);
    float interleavedGradientNoise = ignoise(screenPosition, random * 9999.0);
    
    // Корректировка дистанции реймарча
    raymarchDistance = min(raymarchDistance, raymarchMaxDepth + (raymarchDistance * raymarchDistanceBias));
    raymarchDistance = lerp(raymarchDistance, raymarchDistance * interleavedGradientNoise, raymarchNoise);
    
    // Расчет шага реймарча
    float rayStep = raymarchDistance / raymarchSteps;
    float3 rayDirectionStep = raymarchDirection * rayStep;
    
    // Инициализация переменных
    float density = 0.0;
    float depth = 0.0;
    float mainLightShading = 0.0;
    float raymarchStepsMinusOne = raymarchSteps - 1.0;

    // ========================================================
    // ПОДГОТОВКА ДАННЫХ ОСНОВНОГО СВЕТА
    // ========================================================
#ifdef _MAIN_LIGHT_ENABLED
    Light mainLight = GetMainLight();
    
    // Предрасчет параметров для фазы рассеяния
    float g = mainLightAnisotropy;
    float g2 = g * 2.0;
    float gSquared = g * g;
    
    // Косинус угла между направлением света и реймарча
    float cosTheta = dot(mainLight.direction, raymarchDirection);
    float cosThetaSquared = cosTheta * cosTheta;
#endif

    // ========================================================
    // ОСНОВНОЙ ЦИКЛ РЕЙМАРЧА
    // ========================================================
    for (int i = 0; i < MAX_RAYMARCH_STEPS; i++)
    {
        // Прерывание при достижении лимита шагов
        if (i >= raymarchSteps)
            break;
        
        // Корректировка последнего шага для точного попадания в raymarchMaxDepth
        if (i == raymarchSteps - 1)
        {
            rayStep = raymarchMaxDepth - depth;
            rayDirectionStep = raymarchDirection * rayStep;
        }
        
        // Перемещение точки вдоль луча
        position += rayDirectionStep;
        depth += rayStep;
        
        // ========================================================
        // РАСЧЕТ ТЕНЕЙ И РАССЕЯНИЯ
        // ========================================================
        float mainLightShadow = 1.0;
        float mainLightScattering = 1.0;
#define _MAIN_LIGHT_SHADOWS_ENABLED
#ifdef _MAIN_LIGHT_ENABLED
#if defined(_MAIN_LIGHT_SHADOWS)
        // Тени: преобразование координат и выборка
        float4 shadowCoord = TransformWorldToShadowCoord(position);
        mainLightShadow = MainLightRealtimeShadow(shadowCoord);
        mainLightShadow = lerp(1.0, mainLightShadow, mainLightShadowStrength);
#endif
        
        // Расчет анизотропного рассеяния
        mainLightScattering = CalculateMainLightScattering(cosTheta, cosThetaSquared, g2, gSquared);
        mainLightScattering = lerp(1.0, mainLightScattering, mainLightAnisotropyBlend);
#endif
        
        // Накопление вклада света и плотности
        mainLightShading += mainLightShadow * mainLightScattering;
        density++;
        
        // ========================================================
        // ОБРАБОТКА ДОПОЛНИТЕЛЬНЫХ ИСТОЧНИКОВ СВЕТА
        // ========================================================
#ifdef _ADDITIONAL_LIGHTS
        int additionalLightsCount = GetAdditionalLightsCount();
        for (int j = 0; j < additionalLightsCount; ++j)
        {
            Light additionalLight = GetAdditionalLight(j, position);
            
            // Расчет теней и аттенюации
            float additionalShadow = additionalLight.shadowAttenuation;
            additionalShadow = lerp(1.0, additionalShadow, mainLightShadowStrength);
            
            // Расчет параметров рассеяния
            float additionalCosTheta = dot(additionalLight.direction, raymarchDirection);
            float additionalCosThetaSquared = additionalCosTheta * additionalCosTheta;
            float scattering = CalculateMainLightScattering(additionalCosTheta, additionalCosThetaSquared, g2, gSquared);
            scattering = lerp(1.0, scattering, mainLightAnisotropyBlend);
            
            // Суммирование вклада света
            float attenuation = additionalLight.distanceAttenuation * additionalShadow;
            colour.rgb += scattering * attenuation * additionalLight.color / raymarchSteps;
        }
#endif

        // Проверка на превышение максимальной глубины
        if (depth > raymarchMaxDepth)
            break;
    }

    // ========================================================
    // ПОСТОБРАБОТКА РЕЗУЛЬТАТОВ
    // ========================================================
    
    // Нормализация плотности
    density /= raymarchSteps;
    colour.a *= density;

#ifdef _MAIN_LIGHT_ENABLED
    // Финализация вклада основного света
    mainLightShading /= raymarchSteps;
    mainLightShading *= mainLightStrength;
    
    // Смешивание с цветом тумана
    float3 mainLightColour = mainLight.color * mainLightShading;
    colour.rgb += mainLightColour;
#endif

    output = colour;
}

#endif

//#define _MAIN_LIGHT_SHADOWS_ENABLED