public interface IWeatherSystem
{
    /// <summary>
    /// Флаг валидности системы
    /// </summary>
    public bool IsSystemValid { get; set; }

    /// <summary>
    /// Детальная самопроверка системы на валидность
    /// </summary>
    public void ValidateSystem();

    /// <summary>
    /// Обновить требуемые параметры системы по погодным профилям
    /// </summary>
    public void UpdateSystem(WeatherProfile currentWeatherProfile, WeatherProfile nextWeatherProfile, float t);
}
