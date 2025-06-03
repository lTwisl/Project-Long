using UnityEngine;

[System.Serializable]
public class HealthParameter : PlayerParameter
{
    [field: SerializeField, Space(5)] public float RegenerationRate = 10; 
    public override void ChangeParameter(float deltaSeconds)
    {
        float newChangeRate = ChangeRate < 0 ? ChangeRate : RegenerationRate;

        Current = Mathf.Clamp(Current + newChangeRate * deltaSeconds, 0f, Max);
    }
}

