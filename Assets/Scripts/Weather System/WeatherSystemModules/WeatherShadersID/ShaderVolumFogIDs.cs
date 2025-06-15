using UnityEngine;

public class ShaderVolumFogIDs
{
    // Colors
    public static readonly int ColorZenith = Shader.PropertyToID("_Color_Zenith");
    public static readonly int ColorHorizon = Shader.PropertyToID("_Color_Horizon");
    public static readonly int ColorHaze = Shader.PropertyToID("_Color_Haze");
    public static readonly int ColorGround = Shader.PropertyToID("_Color_Ground");

    // Colors Blending
    public static readonly int SoftnessZenithHorizon = Shader.PropertyToID("_Softness_Zenith_Horizon");
    public static readonly int HeightHaze = Shader.PropertyToID("_Height_Haze");
    public static readonly int SoftnessHaze = Shader.PropertyToID("_Softness_Haze");
    public static readonly int SoftnessGround = Shader.PropertyToID("_Softness_Ground");

    // Scattering
    public static readonly int SunPhaseIn = Shader.PropertyToID("_Sun_Phase_In");
    public static readonly int SunPhaseOut = Shader.PropertyToID("_Sun_Phase_Out");
    public static readonly int MoonPhase = Shader.PropertyToID("_Moon_Phase");
    public static readonly int HeightAtmosphere = Shader.PropertyToID("_Height_Atmosphere");

    // Stars
    public static readonly int ColorStars = Shader.PropertyToID("_Color_Stars");
    public static readonly int HorizonOffset = Shader.PropertyToID("_Horizon_Offset");
    public static readonly int ScaleFlickNoise = Shader.PropertyToID("_Scale_Flick_Noise");
    public static readonly int SpeedFlickNoise = Shader.PropertyToID("_Speed_Flick_Noise");

    // Moon & Sun
    public static readonly int MoonColor = Shader.PropertyToID("_Moon_Color");
    public static readonly int SunDirection = Shader.PropertyToID("_Sun_Direction");

    /// <summary>
    /// Проверка валидности всех параметров материала
    /// </summary>
    public static bool IsPropertiesValid(Material material)
    {
        // 1. Проверяем ссылку на материал:
        if (!material) return false;

        // 2. Проверяем свойства материала:
        return
            material.HasProperty(ColorZenith) &&
            material.HasProperty(ColorHorizon) &&
            material.HasProperty(ColorHaze) &&
            material.HasProperty(ColorGround) &&

            material.HasProperty(SoftnessZenithHorizon) &&
            material.HasProperty(HeightHaze) &&
            material.HasProperty(SoftnessHaze) &&
            material.HasProperty(SoftnessGround) &&

            material.HasProperty(SunPhaseIn) &&
            material.HasProperty(SunPhaseOut) &&
            material.HasProperty(MoonPhase) &&
            material.HasProperty(HeightAtmosphere) &&

            material.HasProperty(ColorStars) &&
            material.HasProperty(HorizonOffset) &&
            material.HasProperty(ScaleFlickNoise) &&
            material.HasProperty(SpeedFlickNoise) &&

            material.HasProperty(MoonColor) &&
            material.HasProperty(SunDirection);
    }
}