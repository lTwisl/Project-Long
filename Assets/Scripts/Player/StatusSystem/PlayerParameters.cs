using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerParameters", menuName = "Scriptable Objects/PlayerParameters")]
public class PlayerParameters : ScriptableObject
{
    [field: SerializeField] public HealthStatusParameter Health { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public StaminaStatusParameter Stamina { get; private set; }


    [field: Header("LoadCapacity")]
    [field: SerializeField] public float MaxLoadCapacity { get; set; } = 30;
    public float CurrentLoad { get; set; } = 0;
    public bool IsOverLoad => CurrentLoad > MaxLoadCapacity;


    [field: Header("StatusParameters")]
    [field: SerializeField] public MovementStatusParameter Hunger { get; private set; }
    [field: SerializeField] public MovementStatusParameter Thirst { get; private set; }
    [field: SerializeField] public MovementStatusParameter Fatigue { get; private set; }
    [field: SerializeField] public StatusParameter Cold { get; private set; }
    [field: SerializeField] public StatusParameter Infection { get; private set; }


    private List<StatusParameter> _allParameters = new();
    public IReadOnlyList<StatusParameter> AllParameters => _allParameters;

    public void Init()
    {
        _allParameters.Clear();

        _allParameters.AddRange(new StatusParameter[] {
            Health,
            Stamina,
            Hunger,
            Thirst,
            Fatigue,
            Cold,
            Infection
        });

        Health.Reset();
        Stamina.Reset();
        Hunger.Reset();
        Thirst.Reset();
        Fatigue.Reset();
        Cold.Reset();
        Infection.Reset();
    }
}
