using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;
using Zenject;


[RequireComponent(typeof(PlayerMovement))]
public class PlayerStatusController : MonoBehaviour
{
    [Inject] private World _world;

    private PlayerMovement _playerMovement;
    private PlayerParameters _playerParameters;


    [Inject]
    private void Construct(PlayerParameters playerParameters)
    {
        _playerParameters = playerParameters;
    }


    private void Awake()
    {
        _playerParameters.AllReset();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        AutoSubscribe();
    }


    private void FixedUpdate()
    {
        _playerParameters.Heat.ChangeRate = _world.TotalTemperature;

        foreach (var parameter in _playerParameters.AllParameters)
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
        if (value > 0.5f * _playerParameters.Energy.Max)
            return;

        float value01 = Utility.MapRange(value, 0, 0.5f * _playerParameters.Energy.Max, 1, 0, true);
        _playerParameters.Capacity.OffsetMax = 15 * value01;
    }


    /// <summary>
    /// ���������� ��������� ������ ��������. ��������� ��������� ��������
    /// </summary>
    private void ChangedMoveMode(PlayerMovement.PlayerMoveMode mode)
    {
        foreach (var param in _playerParameters.AllParameters.OfType<MovementStatusParameter>())
        {
            param.SetChangeRateByMoveMode(mode);
        }
    }


    /// <summary>
    /// �������������� �������� �� ������� ����������
    /// </summary>
    private void AutoSubscribe()
    {
        foreach (var param in _playerParameters.AllParameters.OfType<StatusParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.OnReachZero += () => _playerParameters.Health.ChangeRate += param.DecreasedHealthRate;
            param.OnRecoverFromZero += () => _playerParameters.Health.ChangeRate -= param.DecreasedHealthRate;
        }

        _playerParameters.Capacity.OnValueChanged += UpdateStaminaChangeRateRatioByCapacity;
        _playerParameters.Energy.OnValueChanged += EnergyChanged;
        _playerMovement.OnChangedMoveMode += ChangedMoveMode;
    }


    /// <summary>
    /// ������� �� ���� ������� ��� �������������� ������ ������
    /// </summary>
    private void UnsubscribeAll()
    {
        foreach (var param in _playerParameters.AllParameters.OfType<StatusParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.UnsubscribeAll();
        }

        _playerParameters.Capacity.OnValueChanged -= UpdateStaminaChangeRateRatioByCapacity;
        _playerParameters.Energy.OnValueChanged -= EnergyChanged;
        _playerMovement.OnChangedMoveMode -= ChangedMoveMode;
    }


    /// <summary>
    /// �������� �������� � ���������� ���������
    /// </summary>
    public void ModifyParameter(ParameterType parameter, float value)
    {
        _playerParameters.ModifyParameter(parameter, value);
    }


    /// <summary>
    /// �������� ����������� ��������� ������������ � ����������� �� ���� ���������
    /// </summary>
    private void UpdateStaminaChangeRateRatioByCapacity(float capcsity)
    {
        _playerParameters.Stamina.ChangeRateRatioByCapacity = Utility.MapRange(capcsity, 30, 60, 1, 0, true);
    }


#if UNITY_EDITOR
    private void OnGUI()
    {
        if (_playerParameters == null)
            return;

        GUILayout.Label($"\n\n��������: {_playerParameters.Health.Current:f1}");
        GUILayout.Label($"������������: {_playerParameters.Stamina.Current:f1}");
        GUILayout.Label($"�������: {_playerParameters.FoodBalance.Current:f1}");
        GUILayout.Label($"�����: {_playerParameters.WaterBalance.Current:f1}");
        GUILayout.Label($"��������: {_playerParameters.Energy.Current:f1}");
        GUILayout.Label($"�����: {_playerParameters.Heat.Current:f1}");
        GUILayout.Label($"�����������: {_playerParameters.Toxicity.Current:f1}");
    }
#endif
}
