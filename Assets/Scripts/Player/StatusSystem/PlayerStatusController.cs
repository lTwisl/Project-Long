using System;
using System.Linq;
using UnityEngine;
using Zenject;


[RequireComponent(typeof(PlayerMovement))]
public class PlayerStatusController : MonoBehaviour
{
    [Inject] private PlayerParameters _playerParameters;
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _playerParameters.AllReset();

        _playerMovement = GetComponent<PlayerMovement>();

        AutoSubscribe();
    }

    private void FixedUpdate()
    {
        foreach (var parameter in _playerParameters.AllParameters)
        {
            parameter.UpdateParameter(Time.fixedDeltaTime *  WorldTime.Instance.TimeScale / 60f);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeAll();
    }

    private void EnergyChanged(float value)
    {
        if (value > 0.5f * _playerParameters.Energy.Max)
            return;

        float value01 = Utility.MapRange(value, 0, 0.5f * _playerParameters.Energy.Max, 1, 0, true);

        _playerParameters.OffsetMaxLoadCapacity = Mathf.Lerp(0, 15, value01);
    }

    private void ChangedMoveMode(PlayerMovement.PlayerMoveMode mode)
    {
        foreach (var param in _playerParameters.AllParameters.OfType<MovementStatusParameter>())
        {
            param.SetChangeRateByMoveMode(mode);
        }
    }

    private void AutoSubscribe()
    {
        foreach (var param in _playerParameters.AllParameters.OfType<StatusParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.OnReachZero += () => _playerParameters.Health.AddChangeRate(param.DecreasedHealthRate);
            param.OnRecoverFromZero += () => _playerParameters.Health.AddChangeRate(-param.DecreasedHealthRate);
        }

        _playerParameters.Energy.OnValueChanged += EnergyChanged;
        _playerMovement.OnChangedMoveMode += ChangedMoveMode;
    }

    private void UnsubscribeAll()
    {
        foreach (var param in _playerParameters.AllParameters.OfType<StatusParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.UnsubscribeAll();
        }

        _playerMovement.OnChangedMoveMode -= ChangedMoveMode;
    }

    private void OnGUI()
    {
        if (_playerParameters == null)
            return;

        GUILayout.Label($"\n\nЗдоровье: {_playerParameters.Health.Current:f1}");
        GUILayout.Label($"Выносливость: {_playerParameters.Stamina.Current:f1}");
        GUILayout.Label($"Сытость: {_playerParameters.FoodBalance.Current:f1}");
        GUILayout.Label($"Жажда: {_playerParameters.WaterBalance.Current:f1}");
        GUILayout.Label($"Бодрость: {_playerParameters.Energy.Current:f1}");
        GUILayout.Label($"Тепло: {_playerParameters.Heat.Current:f1}");
        GUILayout.Label($"Заражённость: {_playerParameters.Toxicity.Current:f1}");
    }
}
