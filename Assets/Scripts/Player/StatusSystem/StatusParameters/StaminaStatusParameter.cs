public sealed class StaminaStatusParameter : DecreasingStatusParameter
{
    public float BaseMoveChangeRate { get; private set; }
    public float SprintChangeRate { get; private set; }

    public StaminaStatusParameter(float max, float baseChangeRate, float baseMoveChangeRate, float sprintChangeRate) : base(max, baseChangeRate)
    {
        BaseMoveChangeRate = baseMoveChangeRate;
        SprintChangeRate = sprintChangeRate;
    }

    public void SetChangeRateByMoveMode(PlayerMovement.PlayerMoveMode mode)
    {
        ChangeRate = mode switch
        {
            PlayerMovement.PlayerMoveMode.BaseMove => BaseMoveChangeRate,
            PlayerMovement.PlayerMoveMode.Sprint => SprintChangeRate,
            _ => ChangeRate,
        };
    }
}
