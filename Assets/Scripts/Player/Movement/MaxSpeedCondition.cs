using StatsModifiers;


namespace FirstPersonMovement
{
    public enum MoveMode
    {
        Walk,
        Run,
        Crouch,
    }

    public readonly struct MaxSpeedCondition : ICondition
    {
        public readonly MoveMode MoveMode;

        public MaxSpeedCondition(MoveMode valueType)
        {
            MoveMode = valueType;
        }

        public bool Equals(ICondition other)
        {
            return other is MaxSpeedCondition condition && MoveMode == condition.MoveMode;
        }
    }
}
