using System;
using UnityEngine;
using Zenject;


public class PlayerStatusController : MonoBehaviour
{
    [SerializeField] private float _damageHunger = 0.1f;
    [SerializeField] private float _damageThirst = 0.2f;
    [SerializeField] private float _damageCold = 0.4f;

    [Inject] private PlayerParameters _playerParameters;
    [Inject] private PlayerMovement _playerMovement;

    private void Awake()
    {
        _playerParameters.Hunger.OnReachZero += HungerReachZero;
        _playerParameters.Hunger.OnRecoverFromZero += HungerRecoverFromZero;

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

        _playerParameters.Fatigue.OnReachZero -= FatigueReachZero;
        _playerParameters.Fatigue.OnRecoverFromZero -= FatigueRecoverFromZero;

        _playerMovement.OnChangedMoveMode -= UpdateStaminaChangeRate;
    }

    private void HungerRecoverFromZero() => _playerParameters.Health.ChangeRate = 0.0f;
    private void HungerReachZero() => _playerParameters.Health.ChangeRate = -_damageHunger;

    private void FatigueRecoverFromZero() => _playerParameters.LoadCapacity = _playerParameters.MaxLoadCapacity;
    private void FatigueReachZero() => _playerParameters.LoadCapacity = _playerParameters.MaxLoadCapacity / 2f;

    private void UpdateStaminaChangeRate(PlayerMovement.PlayerMoveMode mode)
    {
        switch (mode)
        {
            case PlayerMovement.PlayerMoveMode.Sprint:
                _playerParameters.Stamina.ChangeRate = -3.0f;
                _playerParameters.Fatigue.ChangeRate = -0.5f;
                break;

            default:
                _playerParameters.Stamina.ChangeRate = 0.0f;
                _playerParameters.Fatigue.ChangeRate = -0.1f;
                break;
        }
    }

    [ContextMenu("RestoreFatigue")]
    public void RestoreFatigue()
    {
        _playerParameters.Fatigue.Reset();
    }
}
