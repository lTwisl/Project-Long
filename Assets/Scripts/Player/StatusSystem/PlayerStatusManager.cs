using FirstPersonMovement;
using System.Linq;
using UnityEngine;
using Zenject;

public class PlayerStatusManager : MonoBehaviour
{
    // Dependencies
    public PlayerParameters PlayerParameters { get; private set; }
    private World _world;
    private PlayerMovement _playerMovement;

    [Inject]
    private void Construct(PlayerParameters playerParameters, World world)
    {
        PlayerParameters = playerParameters;
        _world = world;
    }

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        GameTime.OnTimeChanged += HandleTimeUpdate;
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        GameTime.OnTimeChanged -= HandleTimeUpdate;
    }

    // Time-based Updates
    private void HandleTimeUpdate()
    {
        UpdateWorldAffectedParameters();
        UpdateAllParameters();
    }

    private void UpdateWorldAffectedParameters()
    {
        PlayerParameters.Heat.BaseChangeRate = _world.TotalTemperature;
        PlayerParameters.Toxicity.BaseChangeRate = _world.TotalToxicity;
    }

    private void UpdateAllParameters()
    {
        foreach (var parameter in PlayerParameters.AllParameters)
        {
            parameter.UpdateParameter(GameTime.DeltaTime / 60f);
        }
    }

    // Movement Handling
    private void OnMovementStateChanged(FiniteStateMachine.IState state)
    {
        if (state == null)
            return;

        foreach (var param in PlayerParameters.AllParameters.OfType<MovementParameter>())
            param.SetChangeRateByMoveMode(state);
    }

    // Parameter Modifiers
    private void ApplyEnergyDependentModifiers()
    {
        PlayerParameters.Capacity.Mediator.AddModifier(new(0, new ParameterTypeCondition(ValueType.Max), value =>
        {
            if (PlayerParameters.Energy.Current > 0.5f * PlayerParameters.Energy.Max)
                return value;

            float value01 = Utility.MapRange(PlayerParameters.Energy.Current, 0, 0.5f * PlayerParameters.Energy.Max, 1, 0, true);
            return value - 15f * value01;
        }));
    }

    // Inventory Load Effects
    private void UpdateStaminaModifierByCapacity()
    {
        PlayerParameters.Stamina.Mediator.AddModifier(new(0, new ParameterTypeCondition(ValueType.ChangeRate), value =>
        {
            float scale = CalculateCapacityScale(WeightRange.Critical, WeightRange.Ultimate, 1, 3);

            return value > 0
                ? value * Utility.MapRange(scale, 1, 3, 1, 0, true)
                : value * scale;
        }));
    }

    private void UpdateFoodModifierByCapacity()
    {
        PlayerParameters.FoodBalance.Mediator.AddModifier(new(0, new ParameterTypeCondition(ValueType.ChangeRate), value =>
        {
            float scale = CalculateCapacityScale(WeightRange.Critical, WeightRange.Ultimate, 1, 2);

            return value > 0
                ? value * Utility.MapRange(scale, 1, 2, 1, 0, true)
                : value * scale;
        }));
    }

    private void UpdateWaterModifierByCapacity()
    {
        PlayerParameters.WaterBalance.Mediator.AddModifier(new(0, new ParameterTypeCondition(ValueType.ChangeRate), value =>
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
            PlayerParameters.Capacity.Current,
            PlayerParameters.Capacity.GetRangeLoadCapacity(minRange),
            PlayerParameters.Capacity.GetRangeLoadCapacity(maxRange),
            minScale, maxScale, true
        );
    }

    // Event Management
    private void SubscribeToEvents()
    {
        SubscribeToParameterEvents();
        SubscribeToMovementEvents();
    }

    private void SubscribeToParameterEvents()
    {
        foreach (var param in PlayerParameters.AllParameters.OfType<BasePlayerParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0)) continue;

            param.OnReachZero += () => PlayerParameters.Health.Mediator.AddModifier(param.DecreasedHealthModifier);
            param.OnRecoverFromZero += () => PlayerParameters.Health.Mediator.RemoveModifier(param.DecreasedHealthModifier);
        }

        ApplyEnergyDependentModifiers();
        UpdateStaminaModifierByCapacity();
        UpdateFoodModifierByCapacity();
        UpdateWaterModifierByCapacity();
    }

    private void SubscribeToMovementEvents()
    {
        _playerMovement.OnChangedState += OnMovementStateChanged;
    }

    private void UnsubscribeFromEvents()
    {
        UnsubscribeFromParameterEvents();
        UnsubscribeFromMovementEvents();
    }

    private void UnsubscribeFromParameterEvents()
    {
        foreach (var param in PlayerParameters.AllParameters.OfType<BasePlayerParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0)) 
                continue;
            param.UnsubscribeAll();
        }
    }

    private void UnsubscribeFromMovementEvents()
    {
        _playerMovement.OnChangedState -= OnMovementStateChanged;
    }

    // Public Interface
    public void AdjustParameter(ParameterType parameter, float value)
    {
        PlayerParameters.ModifyParameter(parameter, value);
    }

    // Debug UI
#if UNITY_EDITOR
    private void OnGUI()
    {
        if (PlayerParameters == null) 
            return;

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.richText = true;

        Rect rect = new Rect(5, 5, 400, 250);
        GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        GUI.Box(rect, $"<size=30><color=white>" +
            $"Здоровье: {PlayerParameters.Health.Current:f1} <b>/</b> {PlayerParameters.Health.Max}\n" +
            $"Выносливость: {PlayerParameters.Stamina.Current:f1} <b>/</b> {PlayerParameters.Stamina.Max}\n" +
            $"Сытость: {PlayerParameters.FoodBalance.Current:f1} <b>/</b> {PlayerParameters.FoodBalance.Max}\n" +
            $"Жажда: {PlayerParameters.WaterBalance.Current:f1} <b>/</b> {PlayerParameters.WaterBalance.Max}\n" +
            $"Бодрость: {PlayerParameters.Energy.Current:f1} <b>/</b> {PlayerParameters.Energy.Max}\n" +
            $"Тепло: {PlayerParameters.Heat.Current:f1} <b>/</b> {PlayerParameters.Heat.Max}\n" +
            $"Заражённость: {PlayerParameters.Toxicity.Current:f1} <b>/</b> {PlayerParameters.Toxicity.Max}" +
            $"</color></size>", boxStyle);
        GUI.color = Color.white;
    }
#endif
}