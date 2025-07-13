using UnityEngine;

public interface IWeatherSystem
{
    /// <summary>
    /// ���� ���������� �������
    /// </summary>
    public bool IsSystemValid { get; set; }

    /// <summary>
    /// ���������������� � �������� ��������� �������
    /// </summary>
    public abstract void InitializeAndValidateSystem();

    /// <summary>
    /// �������� ��������� ������� �� �������� ��������
    /// </summary>
    public abstract void UpdateSystemParameters(WeatherProfile currentWeatherProfile, WeatherProfile nextWeatherProfile, float t);
}