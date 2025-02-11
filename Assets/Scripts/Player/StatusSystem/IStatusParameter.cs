using System;

public interface IStatusParameter
{
    float Current { get; }
    float Max { get; }
    event Action<float> OnValueChanged;
    void UpdateParameter(float deltaTime);
}


