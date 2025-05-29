public interface IWeatherSystem
{
    /// <summary>
    /// ���� ���������� �������
    /// </summary>
    public bool IsSystemValid { get; set; }

    /// <summary>
    /// ��������� ������������ ������� �� ����������
    /// </summary>
    public void ValidateSystem();

    /// <summary>
    /// �������� ��������� ��������� ������� �� �������� ��������
    /// </summary>
    public void UpdateSystem(WeatherProfile currentWeatherProfile, WeatherProfile nextWeatherProfile, float t);
}
