// ================== СИСТЕМЫ ВЗАИМОДЕЙСТВИЯ ==================
using FirstPersonMovement;
using System;
using System.Linq;

public class MovementSystem : IDisposable
{
    private PlayerParameters _parameters;
    private PlayerMovement _movement;

    public void Initialize(PlayerParameters parameters, PlayerMovement movement)
    {
        _parameters = parameters;
        _movement = movement;

        _movement.OnJump += ApplyJumpCost;
        _parameters.Stamina.OnRecoverFromZero += EnableMovementAfterStaminaRecovery;
        _parameters.Stamina.OnReachZero += DisableMovementWhenStaminaEmpty;
        _parameters.Capacity.OnValueChanged += UpdateMovementConstraints;

        SetupSpeedModifiers();

        foreach (var paran in _parameters.AllParameters.OfType<MovementParameter>())
            _movement.OnChangedState += paran.UpdateBaseChangeRate;
    }

    private void ApplyJumpCost()
    {
        _parameters.Stamina.Current -= _parameters.Stamina.JumpCost;
    }

    private void UpdateMovementConstraints(float weight)
    {
        _movement.CanRun = _parameters.Capacity.IsCanRun();
        _movement.CanWalk = _parameters.Capacity.IsCanWalk();
        _movement.CanJump = _parameters.Capacity.IsCanRun();
    }

    private void EnableMovementAfterStaminaRecovery()
    {
        _movement.CanRun = _parameters.Capacity.IsCanRun();
        _movement.CanJump = _parameters.Capacity.IsCanRun();
    }

    private void DisableMovementWhenStaminaEmpty()
    {
        _movement.CanRun = false;
        _movement.CanJump = false;
    }

    private void SetupSpeedModifiers()
    {
        _movement.SpeedMediator.AddModifier(new(0, MoveMode.Walk, (float speed) =>
        {
            if (speed == 0 || _parameters.Capacity.GetCurrentWeightRange() < WeightRange.Ultimate)
                return speed;

            return speed * CalculateCapacityScale(
                WeightRange.Ultimate,
                WeightRange.UltimateImmovable,
                1, 0);
        }));

        _movement.SpeedMediator.AddModifier(new(0, MoveMode.Run, (float speed) =>
        {
            if (speed == 0 || _parameters.Capacity.GetCurrentWeightRange() < WeightRange.Critical)
                return speed;

            return speed * CalculateCapacityScale(
                WeightRange.Critical,
                WeightRange.Ultimate,
                1,
                _movement.WalkSpeed / speed);
        }));
    }

    private float CalculateCapacityScale(WeightRange minRange, WeightRange maxRange, float minScale, float maxScale)
    {
        return Utility.MapRange(
            _parameters.Capacity.Current,
            _parameters.Capacity.GetRangeLoadCapacity(minRange),
            _parameters.Capacity.GetRangeLoadCapacity(maxRange),
            minScale, maxScale, true
        );
    }

    public void Cleanup()
    {
        _movement.OnJump -= ApplyJumpCost;
        _parameters.Stamina.OnRecoverFromZero -= EnableMovementAfterStaminaRecovery;
        _parameters.Stamina.OnReachZero -= DisableMovementWhenStaminaEmpty;
        _parameters.Capacity.OnValueChanged -= UpdateMovementConstraints;

        foreach (var paran in _parameters.AllParameters.OfType<MovementParameter>())
            _movement.OnChangedState -= paran.UpdateBaseChangeRate;
    }

    public void Dispose()
    {
        Cleanup();
    }
}
