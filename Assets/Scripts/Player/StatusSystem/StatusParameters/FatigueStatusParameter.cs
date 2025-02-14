public sealed class FatigueStatusParameter : DecreasingStatusParameter
{
    public float BaseMoveChangeRate { get; private set; }
    public float SprintChangeRate { get; private set; }

    public FatigueStatusParameter(float max, float changeRate, float baseMoveChangeRate, float sprintChangeRate) : base(max, changeRate)
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
