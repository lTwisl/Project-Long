using System;
using UnityEngine;

public class WeatherLightingAngle : MonoBehaviour
{
    [Tooltip("Географическая широта места (в градусах)")]
    [SerializeField, Range(-90, 90)] private float _latitude = 45f;

    [Tooltip("Угол поворота сцены относительно направления на восток (в градусах)")]
    [SerializeField, Range(0, 360)] private float _angleToEast = 45f;

    private const float AXIAL_TILT = 23.5f;
    private const float SECONDS_IN_DAY = 86400f;
    private const int FIXED_DAY_OF_YEAR = 66; // День года (7 марта)

    private void OnEnable()
    {
        GameTime.OnTimeChanged += UpdateSunPosition;
    }

    private void OnDisable()
    {
        GameTime.OnTimeChanged -= UpdateSunPosition;
    }

    private void UpdateSunPosition()
    {
        transform.eulerAngles = CalculateSunEulerAngles(GameTime.Time);
        Shader.SetGlobalVector("_Sun_Direction", transform.forward);
    }

    private Vector3 CalculateSunEulerAngles(TimeSpan currentTime)
    {
        // 1. Расчет временных параметров:
        float totalSeconds = (float)(currentTime.TotalSeconds + 12 * 3600) % SECONDS_IN_DAY;
        float hourAngle = (totalSeconds / SECONDS_IN_DAY * 360f - 180f); // Часовой угол в градусах

        // 2. Расчет склонения солнца (по упрощенной синуисудальной зависимости):
        float solarDeclination = AXIAL_TILT * Mathf.Sin(Mathf.Deg2Rad * (360f * (FIXED_DAY_OF_YEAR - 81) / 365f));

        // 3. Расчет высоты солнца над горизонтом:
        float elevation = CalculateElevation(hourAngle, solarDeclination);

        // 4. Расчет азимута солнца:
        float azimuth = CalculateAzimuth(hourAngle, solarDeclination, elevation);

        // 5. Применяем смещение направления на восток
        azimuth = (azimuth + _angleToEast) % 360f;

        return new Vector3(-elevation, azimuth, 0f);
    }

    private float CalculateElevation(float hourAngle, float declination)
    {
        float latRad = Mathf.Deg2Rad * _latitude;
        float decRad = Mathf.Deg2Rad * declination;
        float haRad = Mathf.Deg2Rad * hourAngle;

        // Формула высоты солнца: sin(h) = sin(φ)*sin(δ) + cos(φ)*cos(δ)*cos(H)
        float sinElevation = Mathf.Sin(latRad) * Mathf.Sin(decRad) + Mathf.Cos(latRad) * Mathf.Cos(decRad) * Mathf.Cos(haRad);

        return Mathf.Rad2Deg * Mathf.Asin(sinElevation);
    }

    private float CalculateAzimuth(float hourAngle, float declination, float elevation)
    {
        float latRad = Mathf.Deg2Rad * _latitude;
        float decRad = Mathf.Deg2Rad * declination;
        float haRad = Mathf.Deg2Rad * hourAngle;
        float elRad = Mathf.Deg2Rad * elevation;

        // Защита от деления на ноль при зените
        if (Mathf.Approximately(elRad, Mathf.PI / 2))
        {
            return 180f;
        }

        float sinAzimuth = -Mathf.Sin(haRad) * Mathf.Cos(decRad) / Mathf.Cos(elRad);
        float cosAzimuth = (Mathf.Sin(decRad) - Mathf.Sin(latRad) * Mathf.Sin(elRad))
                         / (Mathf.Cos(latRad) * Mathf.Cos(elRad));

        float azimuthRad = Mathf.Atan2(sinAzimuth, cosAzimuth);
        float azimuth = Mathf.Rad2Deg * azimuthRad;

        // Нормализация в диапазон [0, 360]
        return (azimuth + 360f) % 360f;
    }

#if UNITY_EDITOR
    [Header("- - Параметры времени (Editor Only):")]
    [SerializeField, Range(0, 23)] private int _hour = 8;
    [SerializeField, Range(0, 59)] private int _minute = 0;

    private void OnValidate()
    {
        if (transform.hideFlags != HideFlags.NotEditable) transform.hideFlags = HideFlags.NotEditable;
        //if (transform.hideFlags != HideFlags.None) transform.hideFlags = HideFlags.None;

        transform.rotation = Quaternion.Euler(CalculateSunEulerAngles(new TimeSpan(_hour, _minute, 0)));
        Shader.SetGlobalVector("_Sun_Direction", transform.forward);
    }
#endif
}