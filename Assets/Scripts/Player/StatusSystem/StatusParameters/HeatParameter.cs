
[System.Serializable]
public class HeatParameter : BasePlayerParameter
{
    public void Bind(World world)
    {
        world.OnChangedTotalTemperature += v => BaseChangeRate = v;
    }
}

