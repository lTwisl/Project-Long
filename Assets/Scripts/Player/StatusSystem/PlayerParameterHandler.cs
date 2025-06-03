using FirstPersonMovement;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerParameterHandler : MonoBehaviour
{
    [Inject] private PlayerParameters _parameters;

    private InventoryInteractionSystem _inventorySystem;
    private MovementSystem _movementSystem;
    private StatModifierSystem _statModifierSystem;

    private List<IDisposable> _disposables = new();
    private World _world;
    private Inventory _inventory;
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _parameters.Initialize();
    }

    public void Bind(Inventory inventory, PlayerMovement playerMovement, World world)
    {
        _inventory = inventory;
        _playerMovement = playerMovement;
        _world = world;

        // Инициализируем системы
        _inventorySystem = new InventoryInteractionSystem();
        _inventorySystem.Initialize(_parameters, _inventory);
        _disposables.Add(_inventorySystem);

        _movementSystem = new MovementSystem();
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

        if (_world != null)
        {
            _world.OnChangedTotalToxicity -= UpdateToxicityBaseChangeRate;
            _world.OnChangedTotalTemperature -= UpdateHeatBaseChangeRate;
        }
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
        _parameters.ModifyParameter(ParameterType.Toxicity, value);
    }

    private void UpdateHeatBaseChangeRate(float value)
    {
        _parameters.ModifyParameter(ParameterType.Heat, value);
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (_parameters == null) return;

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
#endif
}