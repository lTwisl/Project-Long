[System.Serializable]
public sealed class HealthStatusParameter : BaseStatusParameter
{
    public void AddChangeRate(float newChangeRate)
    {
        ChangeRate += newChangeRate;
    }
}