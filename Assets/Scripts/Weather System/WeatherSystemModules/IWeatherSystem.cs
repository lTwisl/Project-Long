public interface IWeatherSystem
{
    /// <summary>
    /// Флаг валидности системы
    /// </summary>
    public bool IsSystemValid { get; }

    /// <summary>
    /// Инициализация и детальная самопроверка системы на валидность
    /// </summary>
    public void InitializeAndValidateSystem();

    /// <summary>
    /// Обновить требуемые параметры системы по погодным профилям
    /// </summary>
    public void UpdateSystemParameters(WeatherProfile currentWeatherProfile, WeatherProfile nextWeatherProfile, float t);
}
