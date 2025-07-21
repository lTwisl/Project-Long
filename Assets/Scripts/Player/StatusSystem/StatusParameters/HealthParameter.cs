using UnityEngine;

[System.Serializable]
public class HealthParameter : PlayerParameter
{
    [field: SerializeField, Space(5)] public float RegenerationRate = 10; 
    public override void ChangeCurrent(float deltaTime)
    {
        float newChangeRate = ChangeRate < 0 ? ChangeRate : RegenerationRate;

        Current = Mathf.Clamp(Current + newChangeRate * deltaTime, 0f, Max);
    }
}

