public interface IWeatherSystem
{
    /// <summary>
    /// ���� ���������� �������
    /// </summary>
    public bool IsSystemValid { get; }

    /// <summary>
    /// ������������� � ��������� ������������ ������� �� ����������
    /// </summary>
    public void InitializeAndValidateSystem();

    /// <summary>
    /// �������� ��������� ��������� ������� �� �������� ��������
    /// </summary>
    public void UpdateSystemParameters(WeatherProfile currentWeatherProfile, WeatherProfile nextWeatherProfile, float t);
}
