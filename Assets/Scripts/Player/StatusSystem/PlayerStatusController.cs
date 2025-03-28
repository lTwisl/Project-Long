using System.Linq;
using UnityEngine;
using Zenject;


[RequireComponent(typeof(PlayerMovement))]
public class PlayerStatusController : MonoBehaviour
{
    public PlayerParameters PlayerParameters { get; private set; }
    private World _world;

    private PlayerMovement _playerMovement;

    [Inject]
    private void Construct(PlayerParameters playerParameters, World world)
    {
        PlayerParameters = playerParameters;
        _world = world;
    }


    private void Awake()
    {
        PlayerParameters.AllReset();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        AutoSubscribe();
    }


    private void FixedUpdate()
    {
        PlayerParameters.Heat.ChangeRate = _world.TotalTemperature;
        PlayerParameters.Toxicity.ChangeRate = _world.TotalToxicity;

        foreach (var parameter in PlayerParameters.AllParameters)
        {
            parameter.UpdateParameter(WorldTime.Instance.FixedDeltaTime / 60f);
        }
    }

    private void OnDisable()
    {
        UnsubscribeAll();
    }


    /// <summary>
    /// ���������� ��������� �������. ���������� ������������ �����������
    /// </summary>
    private void EnergyChanged(float value)
    {
        if (value > 0.5f * PlayerParameters.Energy.Max)
            return;

        float value01 = Utility.MapRange(value, 0, 0.5f * PlayerParameters.Energy.Max, 1, 0, true);
        PlayerParameters.Capacity.OffsetMax = 15 * value01;
    }


    /// <summary>
    /// ���������� ��������� ������ ��������. ��������� ��������� ��������
    /// </summary>
    private void ChangedMoveMode(PlayerMovement.PlayerMoveMode mode)
    {
        foreach (var param in PlayerParameters.AllParameters.OfType<MovementStatusParameter>())
        {
            param.SetChangeRateByMoveMode(mode);
        }
    }


    /// <summary>
    /// �������������� �������� �� ������� ����������
    /// </summary>
    private void AutoSubscribe()
    {
        foreach (var param in PlayerParameters.AllParameters.OfType<StatusParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.OnReachZero += () => PlayerParameters.Health.ChangeRate += param.DecreasedHealthRate;
            param.OnRecoverFromZero += () => PlayerParameters.Health.ChangeRate -= param.DecreasedHealthRate;
        }

        PlayerParameters.Capacity.OnValueChanged += UpdateStaminaChangeRateRatioByCapacity;
        PlayerParameters.Energy.OnValueChanged += EnergyChanged;
        _playerMovement.OnChangedMoveMode += ChangedMoveMode;
    }


    /// <summary>
    /// ������� �� ���� ������� ��� �������������� ������ ������
    /// </summary>
    private void UnsubscribeAll()
    {
        foreach (var param in PlayerParameters.AllParameters.OfType<StatusParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.UnsubscribeAll();
        }

        PlayerParameters.Capacity.OnValueChanged -= UpdateStaminaChangeRateRatioByCapacity;
        PlayerParameters.Energy.OnValueChanged -= EnergyChanged;
        _playerMovement.OnChangedMoveMode -= ChangedMoveMode;
    }


    /// <summary>
    /// �������� �������� � ���������� ���������
    /// </summary>
    public void ModifyParameter(ParameterType parameter, float value)
    {
        PlayerParameters.ModifyParameter(parameter, value);
    }


    /// <summary>
    /// �������� ����������� ��������� ������������ � ����������� �� ���� ���������
    /// </summary>
    private void UpdateStaminaChangeRateRatioByCapacity(float capcsity)
    {
        PlayerParameters.Stamina.ChangeRateRatioByCapacity = Utility.MapRange(capcsity, 30, 60, 1, 0, true);
    }


#if UNITY_EDITOR
    private void OnGUI()
    {
        if (PlayerParameters == null)
            return;

        GUILayout.Label($"\n\n��������: {PlayerParameters.Health.Current:f1}");
        GUILayout.Label($"������������: {PlayerParameters.Stamina.Current:f1}");
        GUILayout.Label($"�������: {PlayerParameters.FoodBalance.Current:f1}");
        GUILayout.Label($"�����: {PlayerParameters.WaterBalance.Current:f1}");
        GUILayout.Label($"��������: {PlayerParameters.Energy.Current:f1}");
        GUILayout.Label($"�����: {PlayerParameters.Heat.Current:f1}");
        GUILayout.Label($"�����������: {PlayerParameters.Toxicity.Current:f1}");
    }
#endif
}
