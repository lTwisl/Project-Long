using FiniteStateMachine;
using ImprovedTimers;
using UnityEngine;
using static FirstPersonMovement.PlayerMovement;

[System.Serializable]
public class StaminaParameter : MovementParameter
{
    [field: Tooltip("Мгновенное уменьшение текущего значения в следствии прыжка"), Space(5)]
    [field: SerializeField] public float JumpCost { get; protected set; }

    [Tooltip("Штраф за достижения нуля [мин]")]
    [SerializeField] private float _reload = 1;

    CountdownTimer _countdownTimer;

    public override void Initialize()
    {
        base.Initialize();
        _countdownTimer = new(_reload * GameTime.TimeScale);
    }

    public override void UpdateParameter(float deltaTime)
    {
        if (_countdownTimer.IsFinished)
            base.UpdateParameter(deltaTime);

        if (Current <= 0 && !_countdownTimer.IsRunning)
        {
            _countdownTimer.Start();
            base.UpdateParameter(deltaTime);
        }
    }

    public override void SetChangeRateByMoveMode(IState state)
    {
        if (state is JumpingState)
        {
            Current -= JumpCost;
            BaseChangeRate = 0;
            return;
        }

        base.SetChangeRateByMoveMode(state);
    }

    
}

