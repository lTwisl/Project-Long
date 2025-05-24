using FiniteStateMachine;
using UnityEngine;
using static FirstPersonMovement.PlayerMovement;

[System.Serializable]
public class StaminaParameter : MovementParameter
{
    [field: Tooltip("Мгновенное уменьшение текущего значения в следствии прыжка"), Space(5)]
    [field: SerializeField] public float DecreaseDueJump { get; protected set; }

    [Tooltip("Штраф за достижения нуля [м]")]
    [SerializeField] private float _reload = 1;

    private float _timer = 0f;

    public override void UpdateParameter(float deltaTime)
    {
        if (_timer >= 0)
        {
            _timer -= deltaTime;
            return;
        }

        base.UpdateParameter(deltaTime);

        if (Current <= 0)
            _timer = _reload;
    }

    public override void SetChangeRateByMoveMode(IState state)
    {
        if (state is JumpingState)
        {
            Current -= DecreaseDueJump;
            BaseChangeRate = 0;
            return;
        }

        base.SetChangeRateByMoveMode(state);
    }
}

