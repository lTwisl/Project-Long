using StatsModifiers;

public enum ValueType
{
    Max = 0,
    ChangeRate = 1,
}

public readonly struct ParameterTypeCondition : ICondition
{
    public readonly ValueType ValueType;

    public ParameterTypeCondition(ValueType valueType)
    {
        ValueType = valueType;
    }

    public bool Equals(ICondition other)
    {
        return other is ParameterTypeCondition condition && 
            ValueType == condition.ValueType;
    }
}
