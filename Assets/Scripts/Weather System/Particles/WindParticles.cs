using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class WindParticles : MonoBehaviour
{
    [Header("Настройки спавна")]
    [SerializeField, Tooltip("Высота спавна над игроком")] private float spawnHeightAbovePlayer = 10.0f;

    [Header("Настройки турбулентности")] 
    [SerializeField, Tooltip("Сила турбулентности")] private float turbulenceStrength = 1.0f;

    [Header("Ссылка на игрока")]
    [SerializeField, Tooltip("Объект игрока")] private Transform player;

    private ParticleSystem particleSystem;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule;
    private ParticleSystem.NoiseModule noiseModule;

    private void Start()
    {
        // Инициализация ParticleSystem
        particleSystem = GetComponent<ParticleSystem>();
        emissionModule = particleSystem.emission;
        mainModule = particleSystem.main;
        velocityOverLifetimeModule = particleSystem.velocityOverLifetime;
        noiseModule = particleSystem.noise;
    }

    private void Update()
    {
        if (WindSystem.Instance == null || player == null) return;

        // Получаем вектор ветра
        Vector3 windVector = WindSystem.Instance.GetWindGlobalVector();

        // Обновляем параметры частиц в зависимости от ветра
        UpdateParticles(windVector);

        // Позиционируем систему частиц над игроком
        PositionParticlesAbovePlayer();
    }

    /// <summary>
    /// Обновляет параметры частиц в зависимости от ветра.
    /// </summary>
    private void UpdateParticles(Vector3 windVector)
    {
        // Устанавливаем начальную скорость частиц в направлении ветра
        mainModule.startSpeed = 1 + windVector.magnitude;

        // Корректируем направление частиц в зависимости от ветра
        velocityOverLifetimeModule.x = windVector.x;
        velocityOverLifetimeModule.y = -1.75f;
        velocityOverLifetimeModule.z = windVector.z;

        // Обновляем турбулентность в зависимости от силы ветра
        noiseModule.strength = turbulenceStrength * windVector.magnitude;
    }

    /// <summary>
    /// Позиционирует систему частиц над игроком.
    /// </summary>
    private void PositionParticlesAbovePlayer()
    {
        Vector3 playerPosition = player.position;
        transform.position = new Vector3(playerPosition.x, playerPosition.y + spawnHeightAbovePlayer, playerPosition.z);
    }
}