[System.Serializable]
public class ToxicityStatusParameter : StatusParameter
{
    public override void Reset()
    {
        base.Reset();
        Current = 0;
    }
}

