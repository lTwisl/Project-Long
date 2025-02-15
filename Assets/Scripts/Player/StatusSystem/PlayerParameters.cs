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
    [field: SerializeField] public MovementStatusParameter Energy { get; private set; }
    [field: SerializeField] public StatusParameter Heat { get; private set; }
    [field: SerializeField] public StatusParameter Toxisity { get; private set; }


    private List<IStatusParameter> _allParameters = new();
    public IReadOnlyList<IStatusParameter> AllParameters => _allParameters;

    public void Init()
    {
        _allParameters.Clear();

        _allParameters.AddRange(new IStatusParameter[] {
            Health,
            Stamina,
            Hunger,
            Thirst,
            Energy,
            Heat,
            Toxisity
        });
    }

    [ContextMenu("AllReset")]
    public void AllReset()
    {
        foreach (var parameter in AllParameters)
        {
            parameter.Reset();
        }
    }
}
