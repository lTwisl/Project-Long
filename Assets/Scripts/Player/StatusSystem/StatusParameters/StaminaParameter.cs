using FiniteStateMachine;
using UnityEngine;
using static FirstPersonMovement.PlayerMovement;

[System.Serializable]
public class StaminaParameter : MovementParameter
{
    [field: Tooltip("Мгновенное уменьшение текущего значения в следствии прыжка"), Space(5)]
    [field: SerializeField] public float JumpCost { get; protected set; }

    [Tooltip("Штраф за достижения нуля [мин]")]
    [SerializeField] private float _reload = 1;

    private float _timer = 0f;

    public override void UpdateParameter(float deltaTime)
    {
        if (Current <= 0 && _timer < 0)
        {
            _timer = _reload;
            base.UpdateParameter(deltaTime);
        }

        if (_timer >= 0)
        {
            _timer -= deltaTime;
            return;
        }

        base.UpdateParameter(deltaTime);
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

