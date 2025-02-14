public sealed class HealthStatusParameter : DecreasingStatusParameter
{
    public HealthStatusParameter(float max, float changeRate) : base(max, changeRate)
    {

    }

    public void SetChangeRate(float newChangeRate)
    {
        ChangeRate = newChangeRate;
    }
}