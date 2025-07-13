using UnityEngine;

public class ShaderSkyboxIDs
{
    // Colors
    public static readonly int ColorZenith = Shader.PropertyToID("_Color_Zenith");
    public static readonly int ColorHorizon = Shader.PropertyToID("_Color_Horizon");
    public static readonly int ColorHaze = Shader.PropertyToID("_Color_Haze");
    public static readonly int ColorGround = Shader.PropertyToID("_Color_Ground");
    public static readonly int ColorNight = Shader.PropertyToID("_Color_Night");

    // Colors Blending
    public static readonly int SoftnessZenithHorizon = Shader.PropertyToID("_Softness_Zenith_Horizon");
    public static readonly int HeightHaze = Shader.PropertyToID("_Height_Haze");
    public static readonly int SoftnessHaze = Shader.PropertyToID("_Softness_Haze");
    public static readonly int SoftnessGround = Shader.PropertyToID("_Softness_Ground");

    // Time of Day Correction
    public static readonly int SunsetRange = Shader.PropertyToID("_Sunset_Range");

    // Stars
    public static readonly int ColorStars = Shader.PropertyToID("_Color_Stars");
    public static readonly int ColorMoon = Shader.PropertyToID("_Color_Moon");
    public static readonly int SpeedFlickingStars = Shader.PropertyToID("_Speed_Flicking_Stars");
    public static readonly int HorizonOffsetStars = Shader.PropertyToID("_Horizon_Offset_Stars");

    public static bool IsMaterialHasShaderProperties(Material material)
    {
        // 1. Проверяем ссылку на материал:
        if (!material) return false;

        // 2. Проверяем свойства материала:
        return material.HasProperty(ColorZenith) &&
                material.HasProperty(ColorHorizon) &&
                material.HasProperty(ColorHaze) &&
                material.HasProperty(ColorGround) &&
                material.HasProperty(ColorNight) &&

                material.HasProperty(SoftnessZenithHorizon) &&
                material.HasProperty(HeightHaze) &&
                material.HasProperty(SoftnessHaze) &&
                material.HasProperty(SoftnessGround) &&

                material.HasProperty(SunsetRange) &&

                material.HasProperty(ColorStars) &&
                material.HasProperty(ColorMoon) &&
                material.HasProperty(SpeedFlickingStars) &&
                material.HasProperty(HorizonOffsetStars);
    }
}