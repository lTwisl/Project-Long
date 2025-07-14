using System;
using System.Collections.Generic;


[Serializable]
public class InitializerList<SelfType, ByType>
{
    public List<SelfType> Items = new();
}


[Serializable]
public class InitializerListSlots : InitializerList<InventorySlot, InventoryItem>
{
    public static implicit operator List<InventorySlot>(InitializerListSlots exists)
        => exists.Items;
}

[Serializable]
public class InitializerListRandSlots : InitializerList<RandItem, InventoryItem>
{
    public static implicit operator List<RandItem>(InitializerListRandSlots exists)
        => exists.Items;
}
