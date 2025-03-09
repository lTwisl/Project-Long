using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class WindParticles : MonoBehaviour
{
    [Header("��������� ������")]
    [SerializeField, Tooltip("������ ������ ��� �������")] private float spawnHeightAbovePlayer = 10.0f;

    [Header("��������� ��������������")] 
    [SerializeField, Tooltip("���� ��������������")] private float turbulenceStrength = 1.0f;

    [Header("������ �� ������")]
    [SerializeField, Tooltip("������ ������")] private Transform player;

    private ParticleSystem particleSystem;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule;
    private ParticleSystem.NoiseModule noiseModule;

    private void Start()
    {
        // ������������� ParticleSystem
        particleSystem = GetComponent<ParticleSystem>();
        emissionModule = particleSystem.emission;
        mainModule = particleSystem.main;
        velocityOverLifetimeModule = particleSystem.velocityOverLifetime;
        noiseModule = particleSystem.noise;
    }

    private void Update()
    {
        if (WindSystem.Instance == null || player == null) return;

        // �������� ������ �����
        Vector3 windVector = WindSystem.Instance.GetWindGlobalVector();

        // ��������� ��������� ������ � ����������� �� �����
        UpdateParticles(windVector);

        // ������������� ������� ������ ��� �������
        PositionParticlesAbovePlayer();
    }

    /// <summary>
    /// ��������� ��������� ������ � ����������� �� �����.
    /// </summary>
    private void UpdateParticles(Vector3 windVector)
    {
        // ������������� ��������� �������� ������ � ����������� �����
        mainModule.startSpeed = 1 + windVector.magnitude;

        // ������������ ����������� ������ � ����������� �� �����
        velocityOverLifetimeModule.x = windVector.x;
        velocityOverLifetimeModule.y = -1.75f;
        velocityOverLifetimeModule.z = windVector.z;

        // ��������� �������������� � ����������� �� ���� �����
        noiseModule.strength = turbulenceStrength * windVector.magnitude;
    }

    /// <summary>
    /// ������������� ������� ������ ��� �������.
    /// </summary>
    private void PositionParticlesAbovePlayer()
    {
        Vector3 playerPosition = player.position;
        transform.position = new Vector3(playerPosition.x, playerPosition.y + spawnHeightAbovePlayer, playerPosition.z);
    }
}