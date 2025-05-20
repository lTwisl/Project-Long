public interface IWeatherSystem
{
    /// <summary>
    /// Система валидна?
    /// </summary>
    public bool IsSystemValid { get; set; }

    /// <summary>
    /// Проверить валидность системы
    /// </summary>
    public void ValidateSystem();

    /// <summary>
    /// Обновить требуемые параметры системы
    /// </summary>
    public void UpdateSystem(WeatherProfile currentProfile, WeatherProfile newProfile, float t);
}
