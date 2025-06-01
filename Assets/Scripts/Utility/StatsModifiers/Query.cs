using System;

namespace StatsModifiers
{
    public class Query<T>
    {
        public readonly T Condition;

        public float Value;

        public Query(T condition, float value)
        {
            Condition = condition;
            Value = value;
        }
    }
}
