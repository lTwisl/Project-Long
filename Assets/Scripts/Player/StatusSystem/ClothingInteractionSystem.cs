using System;

public class ClothingInteractionSystem : IDisposable
{
    private PlayerParameters _parameters;
    private ClothingSystems.ClothingSystem _clothingSystem;

    public void Initialize(PlayerParameters parameters, ClothingSystems.ClothingSystem clothingSystem)
    {
        _parameters = parameters;
        _clothingSystem = clothingSystem;

        _parameters.Stamina.Mediator.AddModifier(new(0, ValueType.Max, value => value + _clothingSystem.TotalOffsetStamina));

        _parameters.Heat.Mediator.AddModifier(new(0, ValueType.ChangeRate, value => value + _clothingSystem.TotalTemperatureBonus));

        _parameters.Toxicity.Mediator.AddModifier(new(0, ValueType.ChangeRate, value =>
        {
            float potection = 0f;
            foreach (var item in _clothingSystem.ClothingSlotGroups)
            {
                potection += value * item.TotalToxicityProtection;
            }

            return value - potection;
        }));
    }

    public void Cleanup()
    {

    }

    public void Dispose()
    {
        Cleanup();
    }
}
