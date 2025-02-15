[System.Serializable]
public sealed class HealthStatusParameter : StatusParameter
{
    public void SetChangeRate(float newChangeRate)
    {
        ChangeRate = newChangeRate;
    }
}