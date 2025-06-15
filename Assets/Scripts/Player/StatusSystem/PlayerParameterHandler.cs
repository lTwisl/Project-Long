using FirstPersonMovement;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public partial class PlayerParameterHandler : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private bool _showDebugInfoOnScreen = false;
#endif

    [Inject] private PlayerParameters _parameters;

    private InventoryInteractionSystem _inventorySystem;
    private ClothingInteractionSystem _clothingInteractionSystem;
    private MovementInteractionSystem _movementSystem;
    private StatModifierSystem _statModifierSystem;

    private List<IDisposable> _disposables = new();
    private World _world;
    private Inventory _inventory;
    private ClothingSystems.ClothingSystem _clothingSystem;
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _parameters.Initialize();
    }

    public void Bind(Inventory inventory, ClothingSystems.ClothingSystem clothingSystem, PlayerMovement playerMovement, World world)
    {
        _inventory = inventory;
        _clothingSystem = clothingSystem;
        _playerMovement = playerMovement;
        _world = world;

        // Инициализируем системы
        _inventorySystem = new InventoryInteractionSystem();
        _inventorySystem.Initialize(_parameters, _inventory);
        _disposables.Add(_inventorySystem);

        _clothingInteractionSystem = new ClothingInteractionSystem();
        _clothingInteractionSystem.Initialize(_parameters, _clothingSystem);
        _disposables.Add(_clothingInteractionSystem);

        _movementSystem = new MovementInteractionSystem();
        _movementSystem.Initialize(_parameters, _playerMovement);
        _disposables.Add(_movementSystem);

        _statModifierSystem = new StatModifierSystem();
        _statModifierSystem.Initialize(_parameters);
        _disposables.Add(_statModifierSystem);

        // Подписываемся на события окружения
        _world.OnChangedTotalToxicity += UpdateToxicityBaseChangeRate;
        _world.OnChangedTotalTemperature += UpdateHeatBaseChangeRate;
    }

    private void OnEnable()
    {
        GameTime.OnTimeChanged += UpdatePlayerParameters;
    }

    private void OnDisable()
    {
        GameTime.OnTimeChanged -= UpdatePlayerParameters;
    }

    private void OnDestroy()
    {
        foreach (var disposable in _disposables)
            disposable.Dispose();

        _world.OnChangedTotalToxicity -= UpdateToxicityBaseChangeRate;
        _world.OnChangedTotalTemperature -= UpdateHeatBaseChangeRate;

    }

    private void UpdatePlayerParameters()
    {
        foreach (var parameter in _parameters.AllParameters)
        {
            parameter.UpdateParameter(GameTime.DeltaTime / 60f);
        }
    }

    private void UpdateToxicityBaseChangeRate(float value)
    {
        _parameters.Toxicity.BaseChangeRate = value;
        //_parameters.ModifyParameter(ParameterType.Toxicity, value);
    }

    private void UpdateHeatBaseChangeRate(float value)
    {
        _parameters.Heat.BaseChangeRate = value;
        //_parameters.ModifyParameter(ParameterType.Heat, value);
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (_showDebugInfoOnScreen == false)
            return;

        if (_parameters != null)
        {

            GUIStyle boxStyle = new(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                richText = true
            };

            Rect rect = new(5, 5, 400, 250);
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            string statsText = $"<size=30><color=white>" +
                $"Здоровье: {_parameters.Health.Current:f1} <b>/</b> {_parameters.Health.Max}\n" +
                $"Выносливость: {_parameters.Stamina.Current:f1} <b>/</b> {_parameters.Stamina.Max}\n" +
                $"Сытость: {_parameters.FoodBalance.Current:f1} <b>/</b> {_parameters.FoodBalance.Max}\n" +
                $"Жажда: {_parameters.WaterBalance.Current:f1} <b>/</b> {_parameters.WaterBalance.Max}\n" +
                $"Бодрость: {_parameters.Energy.Current:f1} <b>/</b> {_parameters.Energy.Max}\n" +
                $"Тепло: {_parameters.Heat.Current:f1} <b>/</b> {_parameters.Heat.Max}\n" +
                $"Заражённость: {_parameters.Toxicity.Current:f1} <b>/</b> {_parameters.Toxicity.Max}" +
                $"</color></size>";

            GUI.Box(rect, statsText, boxStyle);
            GUI.color = Color.white;
        }

        if (_clothingSystem != null)
        {
            GUIStyle boxStyle = new(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                richText = true
            };

            Rect rect = new(5, 300, 450, 850);
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            string statsText = $"<size=24><color=white>";

            statsText += $"Total Temp Bonus (summury: {_clothingSystem.TotalTemperatureBonus:F2}):\n";
            foreach (var item in _clothingSystem.ClothingSlotGroups)
                statsText += $" - {item.BodyType}: {item.TotalTemperatureBonus:F2}\n";

            statsText += $"Total Offset Stamina (summury: {_clothingSystem.TotalOffsetStamina:F2}):\n";
            foreach (var item in _clothingSystem.ClothingSlotGroups)
                statsText += $" - {item.BodyType}: {item.TotalOffsetStamina:F2}\n";

            statsText += $"Total Friction Bonus (summury: {_clothingSystem.TotalFrictionBonus:F2}):\n";
            foreach (var item in _clothingSystem.ClothingSlotGroups)
                statsText += $" - {item.BodyType}: {item.TotalFrictionBonus:F2}\n";

            statsText += $"Total Physic Protection (summury: {_clothingSystem.TotalPhysicProtection:F2}):\n";
            foreach (var item in _clothingSystem.ClothingSlotGroups)
                statsText += $" - {item.BodyType}: {item.TotalPhysicProtection:F2}\n";

            statsText += $"Total Toxicity Protection:\n";
            foreach (var item in _clothingSystem.ClothingSlotGroups)
                statsText += $" - {item.BodyType}: {item.TotalToxicityProtection:F2}\n";

            statsText += "</color></size>";

            GUI.Box(rect, statsText, boxStyle);
            GUI.color = Color.white;
        }

    }
#endif
}