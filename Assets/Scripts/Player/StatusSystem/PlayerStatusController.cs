using System;
using System.Linq;
using UnityEngine;
using Zenject;


[RequireComponent(typeof(PlayerMovement))]
public class PlayerStatusController : MonoBehaviour
{
    [SerializeField] private float _damageHunger = 0.1f;
    [SerializeField] private float _damageThirst = 0.2f;
    [SerializeField] private float _damageCold = 0.4f;
    [SerializeField] private float _damageEnergy = 0.4f;

    [Inject] private PlayerParameters _playerParameters;
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _playerParameters.AllReset();

        _playerMovement = GetComponent<PlayerMovement>();

        Subscribe(_playerParameters.Hunger, HungerReachZero, HungerRecoverFromZero);
        Subscribe(_playerParameters.Thirst, ThirstReachZero, ThirstRecoverFromZero);
        Subscribe(_playerParameters.Heat, ColdReachZero, ColdRecoverFromZero);
        Subscribe(_playerParameters.Energy, EnergyReachZero, EnergyRecoverFromZero);
        _playerParameters.Energy.OnValueChanged += EnergyChanged;

        _playerMovement.OnChangedMoveMode += ChangedMoveMode;
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
        Unsubscribe(_playerParameters.Hunger, HungerReachZero, HungerRecoverFromZero);
        Unsubscribe(_playerParameters.Thirst, ThirstReachZero, ThirstRecoverFromZero);
        Unsubscribe(_playerParameters.Heat, ColdReachZero, ColdRecoverFromZero);
        Unsubscribe(_playerParameters.Energy, EnergyReachZero, EnergyRecoverFromZero);
        _playerParameters.Energy.OnValueChanged -= EnergyChanged;

        _playerMovement.OnChangedMoveMode -= ChangedMoveMode;
    }

    private void HungerRecoverFromZero() => _playerParameters.Health.AddChangeRate(_damageHunger);
    private void HungerReachZero() => _playerParameters.Health.AddChangeRate(-_damageHunger);

    private void ThirstRecoverFromZero() => _playerParameters.Health.AddChangeRate(_damageThirst);
    private void ThirstReachZero() => _playerParameters.Health.AddChangeRate(-_damageThirst);

    private void ColdRecoverFromZero() => _playerParameters.Health.AddChangeRate(_damageCold);
    private void ColdReachZero() => _playerParameters.Health.AddChangeRate(-_damageCold);

    private void EnergyRecoverFromZero() => _playerParameters.Health.AddChangeRate(_damageEnergy);
    private void EnergyReachZero() => _playerParameters.Health.AddChangeRate(-_damageEnergy);
    private void EnergyChanged(float value)
    {
        if (value > 0.5f * _playerParameters.Energy.Max)
            return;

        float value01 = Utility.MapRange(value, 0, 0.5f * _playerParameters.Energy.Max, 1, 0, true);

        _playerParameters.OffsetMaxLoadCapacity = Mathf.Lerp(0, 15, value01);
    }

    private void ChangedMoveMode(PlayerMovement.PlayerMoveMode mode)
    {
        _playerParameters.Stamina.SetChangeRateByMoveMode(mode);
        _playerParameters.Hunger.SetChangeRateByMoveMode(mode);
        _playerParameters.Thirst.SetChangeRateByMoveMode(mode);
        _playerParameters.Energy.SetChangeRateByMoveMode(mode);
    }

    private void Subscribe(StatusParameter parameter, Action ReachZero, Action RecoverFromZero) 
    {
        parameter.OnReachZero += ReachZero;
        parameter.OnRecoverFromZero += RecoverFromZero;
    }

    private void Unsubscribe(StatusParameter parameter, Action ReachZero, Action RecoverFromZero)
    {
        parameter.OnReachZero -= ReachZero;
        parameter.OnRecoverFromZero -= RecoverFromZero;
    }

    private void OnGUI()
    {
        if (_playerParameters is null)
            return;

        GUILayout.Label($"\n\nЗдоровье: {_playerParameters.Health.Current:f1}");
        GUILayout.Label($"Выносливость: {_playerParameters.Stamina.Current:f1}");
        GUILayout.Label($"Сытость: {_playerParameters.Hunger.Current:f1}");
        GUILayout.Label($"Жажда: {_playerParameters.Thirst.Current:f1}");
        GUILayout.Label($"Бодрость: {_playerParameters.Energy.Current:f1}");
        GUILayout.Label($"Тепло: {_playerParameters.Heat.Current:f1}");
        GUILayout.Label($"Заражённость: {_playerParameters.Toxisity.Current:f1}");
    }
}
