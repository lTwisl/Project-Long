using System;
using UnityEngine;

public class DynamicLightingAngle : MonoBehaviour
{
    private const float DegreesPerSecond = 0.25f / 60f; // �������� � �������

    private void Update()
    {        
        //if (WorldTime.Instance == null) return;

        // �������� ������� ����� �� WorldTime
        TimeSpan currentTime = GameTime.Time;

        // ��������� ���� �������� ������
        float sunRotationAngle = CalculateSunRotation(currentTime);

        // ��������� ������� � ������
        transform.rotation = Quaternion.Euler(sunRotationAngle, 0, 0);
    }

    /// <summary>
    /// ��������� ���� �������� ������ �� ������ �������� �������.
    /// </summary>
    /// <param name="currentTime">������� �����.</param>
    /// <returns>���� �������� ������ �� ��� X.</returns>
    private float CalculateSunRotation(TimeSpan currentTime)
    {
        // ��������� ����� ���������� ����� � ������ �����
        float totalSeconds = (float)currentTime.TotalSeconds % 86400; // ������������ 24 ������ (86400 ������)

        // ��������� ���� ��������: 0.25 �������� � ������ * ����� ���������� �����
        float rotationAngle = totalSeconds * DegreesPerSecond - 90f;
        return rotationAngle;
    }
}