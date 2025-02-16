using System;
using UnityEngine;

public class LightChanger : MonoBehaviour
{
    private float rotationSpeed = 0.25f; // 15 градусов в час = 0.25 градусов в минуту

    private Transform sunTransform; // Трансформ объекта солнца

    private void Awake()
    {
        // Получаем компонент Transform объекта, к которому прикреплен этот скрипт
        sunTransform = transform;
    }

    private void Start()
    {
        // Подписываемся на событие завершения ожидания
        WorldTime.Instance.OnWaitingEnd += TakeWaitTime;

        // Пример использования: ждем 1 час и 21 минуту
        //WorldTime.Instance.WaitTargetTime(new TimeSpan(0, 1, 21, 0));
        //WorldTime.Instance.WaitTheTime(new TimeSpan(0, 1, 0, 0));
    }

    private void TakeWaitTime(TimeSpan takingTime)
    {
        Debug.Log($"Время ожидания завершено: {takingTime.ToString(@"dd\.hh\:mm")}");
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
        float totalMinutes = (float)currentTime.TotalMinutes % 1440; // Ограничиваем 24 часами

        // Вычисляем угол поворота: 0.25 градусов в минуту * общее количество минут
        float rotationAngle = totalMinutes * rotationSpeed;

        // Корректируем угол, чтобы солнце восходило на востоке и заходило на западе
        // В полночь (00:00) угол = -90 градусов (солнце за горизонтом)
        // В полдень (12:00) угол = 90 градусов (солнце в зените)
        rotationAngle -= 90f;

        return rotationAngle;
    }

    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        if (WorldTime.Instance != null)
        {
            WorldTime.Instance.OnWaitingEnd -= TakeWaitTime;
        }
    }
}