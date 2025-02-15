using System;
using UnityEngine;

public class LightChanger : MonoBehaviour
{
    [Header("��������� ������")]
    [SerializeField, Tooltip("�������� �������� ������ (�������� � ������)")]
    private float rotationSpeed = 0.25f; // 15 �������� � ��� = 0.25 �������� � ������

    private Transform sunTransform; // ��������� ������� ������

    private void Awake()
    {
        // �������� ��������� Transform �������, � �������� ���������� ���� ������
        sunTransform = transform;
    }

    private void Start()
    {
        WorldTime.Instance.WaitTime(TimeSpan.FromDays(5));
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
        float totalMinutes = (float)currentTime.TotalMinutes;

        // ��������� ���� ��������: 0.25 �������� � ������ * ����� ���������� �����
        float rotationAngle = totalMinutes * rotationSpeed;

        // ������������ ���� � ��������� �� 0 �� 360 ��������
        rotationAngle %= 360f;

        return rotationAngle;
    }
}