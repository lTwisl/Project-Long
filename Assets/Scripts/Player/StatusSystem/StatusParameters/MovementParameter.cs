using FiniteStateMachine;
using FirstPersonMovement;
using System;
using System.Collections.Generic;
using UnityEngine;
using static FirstPersonMovement.PlayerMovement;

[System.Serializable]
public class MovementParameter : BasePlayerParameter
{

    [field: Tooltip("Скорость изменения в покое [ед/м]"), Space(5)]
    [field: SerializeField] public float IdleChangeRate { get; protected set; }

    [field: Tooltip("Скорость изменения при хотьбе [ед/м]")]
    [field: SerializeField] public float WalkChangeRate { get; protected set; }

    [field: Tooltip("Скорость изменения при беге [ед/м]")]
    [field: SerializeField] public float SprintChangeRate { get; protected set; }

    private Dictionary<Type, float> _stateChangeRates;

    public override void Initialize()
    {
        base.Initialize();
        BaseChangeRate = IdleChangeRate;

        _stateChangeRates = new()
        {
            { typeof(IdleState), IdleChangeRate },
            { typeof(WalkState), WalkChangeRate },
            { typeof(RunState), SprintChangeRate },
        };
    }

    public void UpdateBaseChangeRate(IState state)
    {
        if (_stateChangeRates.TryGetValue(state.GetType(), out float value))
            BaseChangeRate = value;
        else
            BaseChangeRate = 0f;
    }
}

