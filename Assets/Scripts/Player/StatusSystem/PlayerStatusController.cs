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

        GameTime.OnTimeChanged += HanldeChangedTime;
    }

    private void OnDisable()
    {
        UnsubscribeAll();

        GameTime.OnTimeChanged -= HanldeChangedTime;
    }

    private void HanldeChangedTime()
    {
        PlayerParameters.Heat.ChangeRate = _world.TotalTemperature;
        PlayerParameters.Toxicity.ChangeRate = _world.TotalToxicity;

        foreach (var parameter in PlayerParameters.AllParameters)
        {
            parameter.UpdateParameter(GameTime.DeltaTime / 60f);
        }
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
        foreach (var param in PlayerParameters.AllParameters.OfType<BaseStatusParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.OnReachZero += () => PlayerParameters.Health.ChangeRate += param.DecreasedHealthRate;
            param.OnRecoverFromZero += () => PlayerParameters.Health.ChangeRate -= param.DecreasedHealthRate;
        }

        PlayerParameters.Capacity.OnValueChanged += UpdateStaminaChangeRateRatioByCapacity;
        PlayerParameters.Capacity.OnValueChanged += UpdateFoodBalanceChangeRateRatioByCapacity;
        PlayerParameters.Capacity.OnValueChanged += UpdateWaterBalanceChangeRateRatioByCapacity;
        PlayerParameters.Energy.OnValueChanged += EnergyChanged;
        _playerMovement.OnChangedMoveMode += ChangedMoveMode;
    }


    /// <summary>
    /// ������� �� ���� ������� ��� �������������� ������ ������
    /// </summary>
    private void UnsubscribeAll()
    {
        foreach (var param in PlayerParameters.AllParameters.OfType<BaseStatusParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.UnsubscribeAll();
        }

        PlayerParameters.Capacity.OnValueChanged -= UpdateStaminaChangeRateRatioByCapacity;
        PlayerParameters.Capacity.OnValueChanged -= UpdateFoodBalanceChangeRateRatioByCapacity;
        PlayerParameters.Capacity.OnValueChanged -= UpdateWaterBalanceChangeRateRatioByCapacity;
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
    private void UpdateStaminaChangeRateRatioByCapacity(float capacity)
    {
        PlayerParameters.Stamina.ChangeRateRatioByCapacity = Utility.MapRange(capacity, 
            PlayerParameters.Capacity.GetRangeLoadCapacity(WeightRange.Critical), 
            PlayerParameters.Capacity.GetRangeLoadCapacity(WeightRange.Ultimate), 
            1, 3, true);
        // 1 - �������� ��������/���������� �����������
        // 3 - �������� �������� � 3 ���� ������, �������� ���������� 0.
    }

    /// <summary>
    /// �������� ����������� ��������� ��� � ����������� �� ���� ���������
    /// </summary>
    private void UpdateFoodBalanceChangeRateRatioByCapacity(float capacity)
    {
        PlayerParameters.FoodBalance.ChangeRateRatioByCapacity = Utility.MapRange(capacity,
            PlayerParameters.Capacity.GetRangeLoadCapacity(WeightRange.Critical),
            PlayerParameters.Capacity.GetRangeLoadCapacity(WeightRange.Ultimate),
            1, 2, true);
        // 1 - �������� ��������/���������� �����������
        // 2 - �������� �������� � 2 ���� ������, �������� ���������� 0.
    }

    /// <summary>
    /// �������� ����������� ��������� ���� � ����������� �� ���� ���������
    /// </summary>
    private void UpdateWaterBalanceChangeRateRatioByCapacity(float capacity)
    {
        PlayerParameters.WaterBalance.ChangeRateRatioByCapacity = Utility.MapRange(capacity,
            PlayerParameters.Capacity.GetRangeLoadCapacity(WeightRange.Critical),
            PlayerParameters.Capacity.GetRangeLoadCapacity(WeightRange.Ultimate),
            1, 2, true);
        // 1 - �������� ��������/���������� �����������
        // 2 - �������� �������� � 2 ���� ������, �������� ���������� 0.
    }


#if UNITY_EDITOR
    private void OnGUI()
    {
        if (PlayerParameters == null)
            return;

        GUI.color = Color.red;
        GUILayout.Label($"<size=34>\n\n��������: {PlayerParameters.Health.Current:f1}</size>");
        GUILayout.Label($"<size=34>������������: {PlayerParameters.Stamina.Current:f1}</size>");
        GUILayout.Label($"<size=34>�������: {PlayerParameters.FoodBalance.Current:f1}</size>");
        GUILayout.Label($"<size=34>�����: {PlayerParameters.WaterBalance.Current:f1}</size>");
        GUILayout.Label($"<size=34>��������: {PlayerParameters.Energy.Current:f1}</size>");
        GUILayout.Label($"<size=34>�����: {PlayerParameters.Heat.Current:f1}</size>");
        GUILayout.Label($"<size=34>�����������: {PlayerParameters.Toxicity.Current:f1}</size>");
    }
#endif
}
