using UnityEngine;

public interface IWeatherSystem
{
    /// <summary>
    /// Флаг валидности системы
    /// </summary>
    public bool IsSystemValid { get; set; }

    /// <summary>
    /// Инициализировать и детально проверить систему
    /// </summary>
    public abstract void InitializeAndValidateSystem();

    /// <summary>
    /// Обновить параметры системы по погодным профилям
    /// </summary>
    public abstract void UpdateSystemParameters(WeatherProfile currentWeatherProfile, WeatherProfile nextWeatherProfile, float t);
}