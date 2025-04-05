using System;
using UnityEngine;

public class DynamicLightingAngle : MonoBehaviour
{
    private const float DegreesPerSecond = 0.25f / 60f; // Градусов в секунду

    private void Update()
    {        
        //if (WorldTime.Instance == null) return;

        // Получаем текущее время из WorldTime
        TimeSpan currentTime = GameTime.Time;

        // Вычисляем угол поворота солнца
        float sunRotationAngle = CalculateSunRotation(currentTime);

        // Применяем поворот к солнцу
        transform.rotation = Quaternion.Euler(sunRotationAngle, 0, 0);
    }

    /// <summary>
    /// Вычисляет угол поворота солнца на основе текущего времени.
    /// </summary>
    /// <param name="currentTime">Текущее время.</param>
    /// <returns>Угол поворота солнца по оси X.</returns>
    private float CalculateSunRotation(TimeSpan currentTime)
    {
        // Вычисляем общее количество минут с начала суток
        float totalSeconds = (float)currentTime.TotalSeconds % 86400; // Ограничиваем 24 часами (86400 секунд)

        // Вычисляем угол поворота: 0.25 градусов в минуту * общее количество минут
        float rotationAngle = totalSeconds * DegreesPerSecond - 90f;
        return rotationAngle;
    }
}