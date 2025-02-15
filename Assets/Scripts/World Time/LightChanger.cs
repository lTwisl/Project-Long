using System;
using UnityEngine;

public class LightChanger : MonoBehaviour
{
    [Header("Настройки солнца")]
    [SerializeField, Tooltip("Скорость вращения солнца (градусов в минуту)")]
    private float rotationSpeed = 0.25f; // 15 градусов в час = 0.25 градусов в минуту

    private Transform sunTransform; // Трансформ объекта солнца

    private void Awake()
    {
        // Получаем компонент Transform объекта, к которому прикреплен этот скрипт
        sunTransform = transform;
    }

    private void Start()
    {
        WorldTime.Instance.WaitTime(TimeSpan.FromDays(5));
    }

    private void Update()
    {
        // Получаем текущее время из WorldTime
        TimeSpan currentTime = WorldTime.Instance.CurrentTime;

        // Вычисляем угол поворота солнца
        float sunRotationAngle = CalculateSunRotation(currentTime);

        // Применяем поворот к солнцу
        sunTransform.rotation = Quaternion.Euler(sunRotationAngle, 0, 0);
    }

    /// <summary>
    /// Вычисляет угол поворота солнца на основе текущего времени.
    /// </summary>
    /// <param name="currentTime">Текущее время.</param>
    /// <returns>Угол поворота солнца по оси X.</returns>
    private float CalculateSunRotation(TimeSpan currentTime)
    {
        // Вычисляем общее количество минут с начала суток
        float totalMinutes = (float)currentTime.TotalMinutes;

        // Вычисляем угол поворота: 0.25 градусов в минуту * общее количество минут
        float rotationAngle = totalMinutes * rotationSpeed;

        // Ограничиваем угол в диапазоне от 0 до 360 градусов
        rotationAngle %= 360f;

        return rotationAngle;
    }
}