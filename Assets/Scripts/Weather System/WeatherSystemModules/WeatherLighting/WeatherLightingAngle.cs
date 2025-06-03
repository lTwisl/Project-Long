using System;
using UnityEngine;

public class WeatherLightingAngle : MonoBehaviour
{
    private const float SECONDS_IN_DAY = 86400f;
    private const float DEGREES_PER_SECOND = 360f / SECONDS_IN_DAY;

    private void Awake()
    {
        GameTime.OnTimeChanged += UpdateLightAngle; 
    }

    private void UpdateLightAngle()
    {
        transform.rotation = Quaternion.Euler(CalculateTheRotationXAngle(GameTime.Time), transform.eulerAngles.y, transform.eulerAngles.z);
    }

    private float CalculateTheRotationXAngle(TimeSpan currentTime)
    {
        // Конвертируем время в секунды, а затем нормализуем в пределах 24 часов
        float totalSeconds = (float)currentTime.TotalSeconds % SECONDS_IN_DAY;

        return totalSeconds * DEGREES_PER_SECOND - 90f;
    }

    private void OnDestroy()
    {
        GameTime.OnTimeChanged -= UpdateLightAngle;
    }
}