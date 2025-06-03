using FiniteStateMachine;
using UnityEngine.LightTransport;

[System.Serializable]
public class ToxicityParameter : PlayerParameter
{
    public override void Initialize()
    {
        base.Initialize();
        Current = 0;
    }
}

