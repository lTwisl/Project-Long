using UnityEngine;

public class ShaderFogIDs
{
    // Colors
    public static readonly int FogColor = Shader.PropertyToID("_Fog_Color");
    public static readonly int LightInfluence = Shader.PropertyToID("_Light_Influence");
    public static readonly int FogMinIntensity = Shader.PropertyToID("_Fog_Min_Intensity");
    public static readonly int FogTransparency = Shader.PropertyToID("_Fog_Transparency");

    // Distance Mask
    public static readonly int FogDistance = Shader.PropertyToID("_Fog_Distance");
    public static readonly int FogDistanceSoftness = Shader.PropertyToID("_Fog_Distance_Softness");
    public static readonly int FogOffset = Shader.PropertyToID("_Fog_Offset");

    // Height Mask
    public static readonly int FogLowerHeight = Shader.PropertyToID("_Fog_Lower_Height");
    public static readonly int FogUpperHeight = Shader.PropertyToID("_Fog_Upper_Height");
    public static readonly int FogSoftnessHeight = Shader.PropertyToID("_Fog_Softness_Height");

    /// <summary>
    /// Проверка валидности всех параметров материала
    /// </summary>
    public static bool IsMaterialHasShaderProperties(Material material)
    {
        // 1. Проверяем ссылку на материал:
        if (!material) return false;

        // 2. Проверяем свойства материала:
        return material.HasProperty(FogColor) &&
                material.HasProperty(LightInfluence) &&
                material.HasProperty(FogMinIntensity) &&
                material.HasProperty(FogTransparency) &&

                material.HasProperty(FogDistance) &&
                material.HasProperty(FogDistanceSoftness) &&
                material.HasProperty(FogOffset) &&

                material.HasProperty(FogLowerHeight) &&
                material.HasProperty(FogUpperHeight) &&
                material.HasProperty(FogSoftnessHeight);
    }
}