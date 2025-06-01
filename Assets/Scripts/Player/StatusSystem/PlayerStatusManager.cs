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
        //SubscribeToEvents();
        GameTime.OnTimeChanged += HandleTimeUpdate;
    }

    private void OnDisable()
    {
        //UnsubscribeFromEvents();
        GameTime.OnTimeChanged -= HandleTimeUpdate;
    }

    // Time-based Updates
    private void HandleTimeUpdate()
    {
        //UpdateWorldAffectedParameters();
        UpdateAllParameters();
    }

    /*private void UpdateWorldAffectedParameters()
    {
        PlayerParameters.Heat.BaseChangeRate = _world.TotalTemperature;
        PlayerParameters.Toxicity.BaseChangeRate = _world.TotalToxicity;
    }*/

    private void UpdateAllParameters()
    {
        foreach (var parameter in PlayerParameters.AllParameters)
        {
            parameter.UpdateParameter(GameTime.DeltaTime / 60f);
        }
    }

    /*// Event Management
    private void SubscribeToEvents()
    {
        SubscribeToParameterEvents();
    }

    private void SubscribeToParameterEvents()
    {
        foreach (var param in PlayerParameters.AllParameters.OfType<BasePlayerParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0)) continue;

            param.OnReachZero += () => PlayerParameters.Health.Mediator.AddModifier(param.DecreasedHealthModifier);
            param.OnRecoverFromZero += () => PlayerParameters.Health.Mediator.RemoveModifier(param.DecreasedHealthModifier);
        }
    }*/

    /*private void UnsubscribeFromEvents()
    {
        UnsubscribeFromParameterEvents();
    }

    private void UnsubscribeFromParameterEvents()
    {
        foreach (var param in PlayerParameters.AllParameters.OfType<BasePlayerParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0)) 
                continue;
            param.UnsubscribeAll();
        }
    }*/

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