using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "_playerParameters", menuName = "Scriptable Objects/_playerParameters")]
public class PlayerParameters : ScriptableObject
{
    [field: SerializeField] public DecreasingStatusParameter Health { get; private set; } = new DecreasingStatusParameter(100, 0.0f);

    [field: Space(10)]
    [field: SerializeField] public DecreasingStatusParameter Stamina { get; private set; } = new DecreasingStatusParameter(100, 0.0f);


    [field: Header("LoadCapacity")]
    [field: SerializeField] public float MaxLoadCapacity { get; private set; } = 30;
    public float LoadCapacity { get; set; } = 30;
    public float CurrentLoad { get; set; } = 0;
    public bool IsOverLoad => CurrentLoad > LoadCapacity;


    [field: Header("StatusParameters")]
    [field: SerializeField] public DecreasingStatusParameter Hunger { get; private set; } = new DecreasingStatusParameter(100, -0.1f);
    [field: SerializeField] public DecreasingStatusParameter Thirst { get; private set; } = new DecreasingStatusParameter(100, -0.1f);
    [field: SerializeField] public DecreasingStatusParameter Fatigue { get; private set; } = new DecreasingStatusParameter(100, -0.1f);
    [field: SerializeField] public DecreasingStatusParameter Cold { get; private set; } = new DecreasingStatusParameter(100, -0.1f);
    [field: SerializeField] public DecreasingStatusParameter Infection { get; private set; } = new DecreasingStatusParameter(100, -0.1f);


    private List<IStatusParameter> _allParameters = new();
    public IReadOnlyList<IStatusParameter> AllParameters => _allParameters;

    public void Init()
    {
        _allParameters.AddRange(new IStatusParameter[] {
            Health,
            Stamina,
            Hunger,
            Thirst,
            Fatigue,
            Cold,
            Infection
        });

        //Health = MaxHealth;
        LoadCapacity = MaxLoadCapacity;

        Health.Reset();
        Stamina.Reset();
        Hunger.Reset();
        Thirst.Reset();
        Fatigue.Reset();
        Cold.Reset();
        Infection.Reset();
    }
}
