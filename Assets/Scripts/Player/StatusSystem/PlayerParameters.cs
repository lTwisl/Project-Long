using FirstPersonMovement;
using StatsModifiers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum WeightRange
{
    Acceptable = 0,
    Critical = 1,
    Ultimate = 2,
    UltimateImmovable = 3,
}

public enum ParameterType
{
    Health,
    Stamina,
    FoodBalance,
    WaterBalance,
    Energy,
    Heat,
    Toxicity,
    Capacity,
}

[CreateAssetMenu(fileName = "Player Parameters", menuName = "Scriptable Objects/Player Parameters File")]
public class PlayerParameters : ScriptableObject
{
    [field: Header("<size=16>- - General Parameters:</size>")]
    [field: SerializeField] public HealthParameter Health { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public StaminaParameter Stamina { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public CapacityParameter Capacity { get; private set; }

    [field: Header("<size=16>- - Status Parameters:</size>")]
    [field: SerializeField] public FoodBalanceParameter FoodBalance { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public WaterBalanceParameter WaterBalance { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public MovementParameter Energy { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public HeatParameter Heat { get; private set; }



    [field: Space(10)]
    [field: SerializeField] public ToxicityParameter Toxicity { get; private set; }


    private Dictionary<ParameterType, PlayerParameter> _statusParameterCache;
    public IEnumerable<IPlayerParameter> AllParameters => _statusParameterCache.Values.AsEnumerable();

    public void Initialize()
    {
        _statusParameterCache = new Dictionary<ParameterType, PlayerParameter>()
        {
            { ParameterType.Health, Health},
            { ParameterType.Stamina, Stamina},
            { ParameterType.FoodBalance, FoodBalance},
            { ParameterType.WaterBalance, WaterBalance},
            { ParameterType.Energy, Energy},
            { ParameterType.Heat, Heat},
            { ParameterType.Toxicity, Toxicity},
            { ParameterType.Capacity, Capacity},
        };

        foreach (var parameter in AllParameters)
            parameter.Initialize();

        GameTime.OnTimeChanged += Update;
    }

    public void Update()
    {
        foreach (var parameter in AllParameters)
        {
            parameter.UpdateParameter(GameTime.DeltaTime / 60f);
        }
    }


    public void Bind(Inventory inventory, PlayerMovement playerMovement, World world)
    {
        Capacity.Bind(inventory);

        foreach (var param in AllParameters.OfType<MovementParameter>())
            param.Bind(playerMovement);

        ApplyEnergyDependentModifiers();
        UpdateStaminaModifierByCapacity();
        UpdateFoodModifierByCapacity();
        UpdateWaterModifierByCapacity();

        foreach (var param in AllParameters.OfType<BasePlayerParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.OnReachZero += () => Health.Mediator.AddModifier(param.DecreasedHealthModifier);
            param.OnRecoverFromZero += () => Health.Mediator.RemoveModifier(param.DecreasedHealthModifier);
        }

        Toxicity.Bind(world);
        Heat.Bind(world);
    }

    public void AddModifire(ParameterType parameter, StatModifier<ValueType> modifier)
    {
        _statusParameterCache[parameter].Mediator.AddModifier(modifier);
    }

    public void ModifyParameter(ParameterType parameter, float value)
    {
        _statusParameterCache[parameter].Current += value;
    }

    public IPlayerParameter GetParameter(ParameterType parameter)
    {
        return _statusParameterCache[parameter];
    }

    // Parameter Modifiers
    private void ApplyEnergyDependentModifiers()
    {
        Capacity.Mediator.AddModifier(new(0, ValueType.Max, value =>
        {
            if (Energy.Current > 0.5f * Energy.Max)
                return value;

            float value01 = Utility.MapRange(Energy.Current, 0, 0.5f * Energy.Max, 1, 0, true);
            return value - 15f * value01;
        }));
    }

    // Inventory Load Effects
    private void UpdateStaminaModifierByCapacity()
    {
        Stamina.Mediator.AddModifier(new(0, ValueType.ChangeRate, value =>
        {
            float scale = CalculateCapacityScale(WeightRange.Critical, WeightRange.Ultimate, 1, 3);

            return value > 0
                ? value * Utility.MapRange(scale, 1, 3, 1, 0, true)
                : value * scale;
        }));
    }

    private void UpdateFoodModifierByCapacity()
    {
        FoodBalance.Mediator.AddModifier(new(0, ValueType.ChangeRate, value =>
        {
            float scale = CalculateCapacityScale(WeightRange.Critical, WeightRange.Ultimate, 1, 2);

            return value > 0
                ? value * Utility.MapRange(scale, 1, 2, 1, 0, true)
                : value * scale;
        }));
    }

    private void UpdateWaterModifierByCapacity()
    {
        WaterBalance.Mediator.AddModifier(new(0, ValueType.ChangeRate, value =>
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
            Capacity.Current,
            Capacity.GetRangeLoadCapacity(minRange),
            Capacity.GetRangeLoadCapacity(maxRange),
            minScale, maxScale, true
        );
    }
}
