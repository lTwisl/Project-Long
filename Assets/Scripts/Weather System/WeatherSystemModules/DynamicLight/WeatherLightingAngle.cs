using System;
using UnityEngine;

public class WeatherLightingAngle : MonoBehaviour
{
    private const float DegreesPerSecond = 0.25f / 60f; // Градусов в секунду

    private void Awake()
    {
        GameTime.OnTimeChanged += UpdateLightAngle; 
    }

    private void UpdateLightAngle()
    {        
        transform.rotation = Quaternion.Euler(CalculateSunRotation(GameTime.Time), 0, 0);
    }

    /// <summary>
    /// Вычисляет угол поворота солнца на основе текущего времени.
    /// </summary>
    /// <returns>Угол поворота солнца по оси X.</returns>
    private float CalculateSunRotation(TimeSpan currentTime)
    {
        // Вычисляем общее количество минут с начала суток
        float totalSeconds = (float)currentTime.TotalSeconds % 86400; // Ограничиваем 24 часами (86400 секунд)

        // Вычисляем угол поворота
        float rotationAngle = totalSeconds * DegreesPerSecond - 90f;
        return rotationAngle;
    }

    private void OnDestroy()
    {
        GameTime.OnTimeChanged -= UpdateLightAngle;
    }
}