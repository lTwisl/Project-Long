namespace StatsModifiers
{
    public class Query
    {
        public readonly ICondition Condition;

        public float Value;

        public Query(ICondition condition, float value)
        {
            Condition = condition;
            Value = value;
        }
    }
}
