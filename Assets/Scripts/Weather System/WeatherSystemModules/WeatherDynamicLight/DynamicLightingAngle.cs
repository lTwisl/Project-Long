using System;
using UnityEngine;

/// <summary>
///  ласс предназначенный дл€ управлени€ изменением положени€ солнца на небе в зависимости от времени суток
/// </summary>
public class DynamicLightingAngle : MonoBehaviour
{
    private const float DegreesPerSecond = 0.25f / 60f; // √радусов в секунду

    private void Update()
    {        
        if (WorldTime.Instance == null) return;

        // ѕолучаем текущее врем€ из WorldTime
        TimeSpan currentTime = WorldTime.Instance.CurrentTime;

        // ¬ычисл€ем угол поворота солнца
        float sunRotationAngle = CalculateSunRotation(currentTime);

        // ѕримен€ем поворот к солнцу
        transform.rotation = Quaternion.Euler(sunRotationAngle, 0, 0);
    }

    /// <summary>
    /// ¬ычисл€ет угол поворота солнца на основе текущего времени.
    /// </summary>
    /// <param name="currentTime">“екущее врем€.</param>
    /// <returns>”гол поворота солнца по оси X.</returns>
    private float CalculateSunRotation(TimeSpan currentTime)
    {
        // ¬ычисл€ем общее количество минут с начала суток
        float totalSeconds = (float)currentTime.TotalSeconds % 86400; // ќграничиваем 24 часами (86400 секунд)

        // ¬ычисл€ем угол поворота: 0.25 градусов в минуту * общее количество минут
        float rotationAngle = totalSeconds * DegreesPerSecond - 90f;
        return rotationAngle;
    }
}