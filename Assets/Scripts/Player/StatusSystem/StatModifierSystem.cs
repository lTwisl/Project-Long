using System;
using System.Linq;
using UnityEngine;
// ================== СИСТЕМА МОДИФИКАТОРОВ ==================
public class StatModifierSystem : IDisposable
{
    private PlayerParameters _parameters;

    public void Initialize(PlayerParameters parameters)
    {
        _parameters = parameters;
        ApplyCapacityModifierByEnergy();
        ApplyStaminaModifierByCapacity();
        ApplyFoodModifierByCapacity();
        ApplyWaterModifierByCapacity();
        SetupHealthDependency();
    }

    private void ApplyCapacityModifierByEnergy()
    {
        _parameters.Capacity.Mediator.AddModifier(new(0, ValueType.Max, value =>
        {
            if (_parameters.Energy.Current > 0.5f * _parameters.Energy.Max)
                return value;

            float value01 = Utility.MapRange(
                _parameters.Energy.Current,
                0, 0.5f * _parameters.Energy.Max,
                1, 0, true);

            return value - 15f * value01;
        }));
    }

    private void ApplyStaminaModifierByCapacity()
    {
        _parameters.Stamina.Mediator.AddModifier(new(0, ValueType.ChangeRate, value =>
        {
            float scale = CalculateCapacityScale(WeightRange.Critical, WeightRange.Ultimate, 1, 3);
            return value > 0
                ? value * Utility.MapRange(scale, 1, 3, 1, 0, true)
                : value * scale;
        }));
    }

    private void ApplyFoodModifierByCapacity()
    {
        _parameters.FoodBalance.Mediator.AddModifier(new(0, ValueType.ChangeRate, value =>
        {
            float scale = CalculateCapacityScale(WeightRange.Critical, WeightRange.Ultimate, 1, 2);
            return value > 0
                ? value * Utility.MapRange(scale, 1, 2, 1, 0, true)
                : value * scale;
        }));
    }

    private void ApplyWaterModifierByCapacity()
    {
        _parameters.WaterBalance.Mediator.AddModifier(new(0, ValueType.ChangeRate, value =>
        {
            float scale = CalculateCapacityScale(WeightRange.Critical, WeightRange.Ultimate, 1, 2);
            return value > 0
                ? value * Utility.MapRange(scale, 1, 2, 1, 0, true)
                : value * scale;
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

    private void SetupHealthDependency()
    {
        foreach (var param in _parameters.AllParameters.OfType<BasePlayerParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.OnReachZero += () =>
                _parameters.Health.Mediator.AddModifier(param.DecreasedHealthModifier);

            param.OnRecoverFromZero += () =>
                _parameters.Health.Mediator.RemoveModifier(param.DecreasedHealthModifier);
        }
    }

    public void Cleanup()
    {
        // Удаление всех модификаторов
    }

    public void Dispose()
    {
        Cleanup();
    }
}
