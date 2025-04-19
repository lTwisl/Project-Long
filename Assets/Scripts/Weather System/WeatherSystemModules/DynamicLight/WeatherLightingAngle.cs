using System;
using UnityEngine;

public class WeatherLightingAngle : MonoBehaviour
{
    private const float DegreesPerSecond = 0.25f / 60f; // �������� � �������

    private void Awake()
    {
        GameTime.OnTimeChanged += UpdateLightAngle; 
    }

    private void UpdateLightAngle()
    {        
        transform.rotation = Quaternion.Euler(CalculateSunRotation(GameTime.Time), 0, 0);
    }

    /// <summary>
    /// ��������� ���� �������� ������ �� ������ �������� �������.
    /// </summary>
    /// <returns>���� �������� ������ �� ��� X.</returns>
    private float CalculateSunRotation(TimeSpan currentTime)
    {
        // ��������� ����� ���������� ����� � ������ �����
        float totalSeconds = (float)currentTime.TotalSeconds % 86400; // ������������ 24 ������ (86400 ������)

        // ��������� ���� ��������
        float rotationAngle = totalSeconds * DegreesPerSecond - 90f;
        return rotationAngle;
    }

    private void OnDestroy()
    {
        GameTime.OnTimeChanged -= UpdateLightAngle;
    }
}