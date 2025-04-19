[System.Serializable]
public class ToxicityStatusParameter : BaseStatusParameter
{
    public override void Reset()
    {
        base.Reset();
        Current = 0;
    }
}

