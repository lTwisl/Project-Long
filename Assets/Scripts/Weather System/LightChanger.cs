using System;
using UnityEngine;

public class LightChanger : MonoBehaviour
{
    private float rotationSpeed = 0.25f; // 15 �������� � ��� = 0.25 �������� � ������

    private Transform sunTransform; // ��������� ������� ������

    private void Awake()
    {
        // �������� ��������� Transform �������, � �������� ���������� ���� ������
        sunTransform = transform;
    }

    private void Start()
    {
        // ������������� �� ������� ���������� ��������
        WorldTime.Instance.OnWaitingEnd += TakeWaitTime;

        // ������ �������������: ���� 1 ��� � 21 ������
        //WorldTime.Instance.WaitTargetTime(new TimeSpan(0, 1, 21, 0));
        //WorldTime.Instance.WaitTheTime(new TimeSpan(0, 1, 0, 0));
    }

    private void TakeWaitTime(TimeSpan takingTime)
    {
        Debug.Log($"����� �������� ���������: {takingTime.ToString(@"dd\.hh\:mm")}");
    }

    private void Update()
    {
        // �������� ������� ����� �� WorldTime
        TimeSpan currentTime = WorldTime.Instance.CurrentTime;

        // ��������� ���� �������� ������
        float sunRotationAngle = CalculateSunRotation(currentTime);

        // ��������� ������� � ������
        sunTransform.rotation = Quaternion.Euler(sunRotationAngle, 0, 0);
    }

    /// <summary>
    /// ��������� ���� �������� ������ �� ������ �������� �������.
    /// </summary>
    /// <param name="currentTime">������� �����.</param>
    /// <returns>���� �������� ������ �� ��� X.</returns>
    private float CalculateSunRotation(TimeSpan currentTime)
    {
        // ��������� ����� ���������� ����� � ������ �����
        float totalMinutes = (float)currentTime.TotalMinutes % 1440; // ������������ 24 ������

        // ��������� ���� ��������: 0.25 �������� � ������ * ����� ���������� �����
        float rotationAngle = totalMinutes * rotationSpeed;

        // ������������ ����, ����� ������ ��������� �� ������� � �������� �� ������
        // � ������� (00:00) ���� = -90 �������� (������ �� ����������)
        // � ������� (12:00) ���� = 90 �������� (������ � ������)
        rotationAngle -= 90f;

        return rotationAngle;
    }

    private void OnDestroy()
    {
        // ������������ �� ������� ��� ����������� �������
        if (WorldTime.Instance != null)
        {
            WorldTime.Instance.OnWaitingEnd -= TakeWaitTime;
        }
    }
}