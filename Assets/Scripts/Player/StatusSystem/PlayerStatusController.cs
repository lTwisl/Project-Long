using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;


[RequireComponent(typeof(PlayerMovement))]
public class PlayerStatusController : MonoBehaviour
{
    public float MaxExternalHeat { get; private set; }
    
    private List<IExternalHeat> _externalHeats = new List<IExternalHeat>();

    private PlayerMovement _playerMovement;
    private PlayerParameters _playerParameters;

    [Inject]
    private void Construct(PlayerParameters playerParameters)
    {
        _playerParameters = playerParameters;
    }

    private void Awake()
    {
        _playerParameters.AllReset();

        _playerMovement = GetComponent<PlayerMovement>();

        AutoSubscribe();
    }

    private void FixedUpdate()
    {
        foreach (var parameter in _playerParameters.AllParameters)
        {
            parameter.UpdateParameter(WorldTime.Instance.FixedDeltaTime / 60f);
        }

        //Debug.Log(GetMaxExternalHeatsByPosiotion());
    }

    private void OnDestroy()
    {
        UnsubscribeAll();
    }

    private void EnergyChanged(float value)
    {
        if (value > 0.5f * _playerParameters.Energy.Max)
            return;

        float value01 = Utility.MapRange(value, 0, 0.5f * _playerParameters.Energy.Max, 1, 0, true);

        _playerParameters.OffsetMaxLoadCapacity = Mathf.Lerp(0, 15, value01);
    }

    private void ChangedMoveMode(PlayerMovement.PlayerMoveMode mode)
    {
        foreach (var param in _playerParameters.AllParameters.OfType<MovementStatusParameter>())
        {
            param.SetChangeRateByMoveMode(mode);
        }
    }

    private void AutoSubscribe()
    {
        foreach (var param in _playerParameters.AllParameters.OfType<StatusParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.OnReachZero += () => _playerParameters.Health.AddChangeRate(param.DecreasedHealthRate);
            param.OnRecoverFromZero += () => _playerParameters.Health.AddChangeRate(-param.DecreasedHealthRate);
        }

        _playerParameters.Energy.OnValueChanged += EnergyChanged;
        _playerMovement.OnChangedMoveMode += ChangedMoveMode;
    }

    private void UnsubscribeAll()
    {
        foreach (var param in _playerParameters.AllParameters.OfType<StatusParameter>())
        {
            if (Mathf.Approximately(param.DecreasedHealthRate, 0))
                continue;

            param.UnsubscribeAll();
        }

        _playerParameters.Energy.OnValueChanged -= EnergyChanged;
        _playerMovement.OnChangedMoveMode -= ChangedMoveMode;
    }


    public void Add(ParameterType parameter, float value)
    {
        _playerParameters.Add(parameter, value);
    }

    public void AddExternalHeat(IExternalHeat externalHeat)
    {
        _externalHeats.Add(externalHeat);

        if (externalHeat.Temp > MaxExternalHeat)
            MaxExternalHeat = externalHeat.Temp;
    }

    public void RemoveExternalHeat(IExternalHeat externalHeat)
    {
        _externalHeats.Remove(externalHeat);

        if (externalHeat.Temp == MaxExternalHeat)
        {
            if (_externalHeats.Count == 0)
                MaxExternalHeat = 0;
            else
                MaxExternalHeat = _externalHeats.Max(p => p.Temp);
        }
    }

    public float GetMaxExternalHeatsByPosiotion()
    {
        if (_externalHeats.Count == 0)
            return 0;

        return _externalHeats.Max(p =>
        {
            return Utility.MapRange((p.Position - transform.position).sqrMagnitude,
                Mathf.Pow(p.MinRadius, 2), Mathf.Pow(p.MaxRadius, 2),
                p.Temp, 0, true);
        });
    }


#if UNITY_EDITOR
    private void OnGUI()
    {
        if (_playerParameters == null)
            return;

        GUILayout.Label($"\n\nЗдоровье: {_playerParameters.Health.Current:f1}");
        GUILayout.Label($"Выносливость: {_playerParameters.Stamina.Current:f1}");
        GUILayout.Label($"Сытость: {_playerParameters.FoodBalance.Current:f1}");
        GUILayout.Label($"Жажда: {_playerParameters.WaterBalance.Current:f1}");
        GUILayout.Label($"Бодрость: {_playerParameters.Energy.Current:f1}");
        GUILayout.Label($"Тепло: {_playerParameters.Heat.Current:f1}");
        GUILayout.Label($"Заражённость: {_playerParameters.Toxicity.Current:f1}");
    }
#endif
}
