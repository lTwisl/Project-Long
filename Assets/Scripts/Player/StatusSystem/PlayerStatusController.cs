using System;
using UnityEngine;
using Zenject;


[RequireComponent(typeof(PlayerMovement))]
public class PlayerStatusController : MonoBehaviour
{
    [SerializeField] private float _damageHunger = 0.1f;
    [SerializeField] private float _damageThirst = 0.2f;
    [SerializeField] private float _damageCold = 0.4f;

    [Inject] private PlayerParameters _playerParameters;
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();

        _playerParameters.Hunger.OnReachZero += HungerReachZero;
        _playerParameters.Hunger.OnRecoverFromZero += HungerRecoverFromZero;

        _playerParameters.Thirst.OnReachZero += ThirstReachZero;
        _playerParameters.Thirst.OnRecoverFromZero += ThirstRecoverFromZero;

        _playerParameters.Cold.OnReachZero += ColdReachZero;
        _playerParameters.Cold.OnRecoverFromZero += ColdRecoverFromZero;


        _playerParameters.Fatigue.OnReachZero += FatigueReachZero;
        _playerParameters.Fatigue.OnRecoverFromZero += FatigueRecoverFromZero;

        _playerMovement.OnChangedMoveMode += UpdateStaminaChangeRate;
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
        _playerParameters.Hunger.OnReachZero -= HungerReachZero;
        _playerParameters.Hunger.OnRecoverFromZero -= HungerRecoverFromZero;

        _playerParameters.Thirst.OnReachZero -= ThirstReachZero;
        _playerParameters.Thirst.OnRecoverFromZero -= ThirstRecoverFromZero;

        _playerParameters.Cold.OnReachZero -= ColdReachZero;
        _playerParameters.Cold.OnRecoverFromZero -= ColdRecoverFromZero;


        _playerParameters.Fatigue.OnReachZero -= FatigueReachZero;
        _playerParameters.Fatigue.OnRecoverFromZero -= FatigueRecoverFromZero;

        _playerMovement.OnChangedMoveMode -= UpdateStaminaChangeRate;
    }

    private void HungerRecoverFromZero() => _playerParameters.Health.SetChangeRate(0.0f);
    private void HungerReachZero() => _playerParameters.Health.SetChangeRate(-_damageHunger);

    private void ThirstRecoverFromZero() => _playerParameters.Health.SetChangeRate(0.0f);
    private void ThirstReachZero() => _playerParameters.Health.SetChangeRate(-_damageThirst);

    private void ColdRecoverFromZero() => _playerParameters.Health.SetChangeRate(0.0f);
    private void ColdReachZero() => _playerParameters.Health.SetChangeRate(-_damageCold);

    private void FatigueRecoverFromZero() => _playerParameters.LoadCapacity = _playerParameters.MaxLoadCapacity;
    private void FatigueReachZero() => _playerParameters.LoadCapacity = _playerParameters.MaxLoadCapacity / 2f;

    private void UpdateStaminaChangeRate(PlayerMovement.PlayerMoveMode mode)
    {
        _playerParameters.Stamina.SetChangeRateByMoveMode(mode);
        _playerParameters.Fatigue.SetChangeRateByMoveMode(mode);
    }

    [ContextMenu("RestoreFatigue")]
    public void RestoreFatigue()
    {
        _playerParameters.Fatigue.Reset();
    }
}
