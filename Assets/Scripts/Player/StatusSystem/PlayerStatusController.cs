using System;
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
        Subscribe(_playerParameters.Energy, FatigueReachZero, FatigueRecoverFromZero);

        _playerMovement.OnChangedMoveMode += ChangedMoveMode;
    }

    private void Update()
    {
        foreach (var parameter in _playerParameters.AllParameters)
        {
            parameter.UpdateParameter(Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        Unsubscribe(_playerParameters.Hunger, HungerReachZero, HungerRecoverFromZero);
        Unsubscribe(_playerParameters.Thirst, ThirstReachZero, ThirstRecoverFromZero);
        Unsubscribe(_playerParameters.Heat, ColdReachZero, ColdRecoverFromZero);
        Unsubscribe(_playerParameters.Energy, FatigueReachZero, FatigueRecoverFromZero);

        _playerMovement.OnChangedMoveMode -= ChangedMoveMode;
    }

    private void HungerRecoverFromZero() => _playerParameters.Health.AddChangeRate(_damageHunger);
    private void HungerReachZero() => _playerParameters.Health.AddChangeRate(-_damageHunger);

    private void ThirstRecoverFromZero() => _playerParameters.Health.AddChangeRate(_damageThirst);
    private void ThirstReachZero() => _playerParameters.Health.AddChangeRate(-_damageThirst);

    private void ColdRecoverFromZero() => _playerParameters.Health.AddChangeRate(_damageCold);
    private void ColdReachZero() => _playerParameters.Health.AddChangeRate(-_damageCold);

    private void FatigueRecoverFromZero()
    {
        _playerParameters.MaxLoadCapacity *= 2f;
        _playerParameters.Health.AddChangeRate(_damageEnergy);
    }
    private void FatigueReachZero()
    {
        _playerParameters.MaxLoadCapacity /= 2f;
        _playerParameters.Health.AddChangeRate(-_damageEnergy);
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


}
