[System.Serializable]
public class ToxicityParameter : PlayerParameter
{
    public override void Initialize()
    {
        base.Initialize();
        Current = 0;
    }

    public void Bind(World world)
    {
        world.OnChangedTotalToxicity += v => BaseChangeRate = v;
    }
}

