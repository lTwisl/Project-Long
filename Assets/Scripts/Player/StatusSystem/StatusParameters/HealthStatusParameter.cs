using UnityEngine;

[System.Serializable]
public class HealthStatusParameter : StatusParameter
{
    [field: SerializeField] public float RegenerationRate = 10; 
    public override void ChangeParameter(float deltaSeconds)
    {
        float newChangeRate = ChangeRate < 0 ? ChangeRate : RegenerationRate;
        Current = Mathf.Clamp(Current + newChangeRate * deltaSeconds, 0f, Max + OffsetMax);
    }
}

