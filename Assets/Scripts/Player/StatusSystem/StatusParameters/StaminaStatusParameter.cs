using UnityEngine;

[System.Serializable]
public class StaminaStatusParameter : MovementStatusParameter
{
    [field: SerializeField] public float Reload {  get; private set; }
    private float _timer = 0f;

    public override void UpdateParameter(float deltaTime)
    {
        if (_timer >= 0)
        {
            _timer -= deltaTime;
            return;
        }

        base.UpdateParameter(deltaTime);

        if (Current <= 0)
            _timer = Reload;
    }
}

