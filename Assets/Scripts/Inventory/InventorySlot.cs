using System;
using UnityEngine;

public interface IReadOnlyInventorySlot
{
    public InventoryItem Item { get; }
    public float Capacity { get; }
    public float Condition { get; }
}


[Serializable]
public class InventorySlot : IReadOnlyInventorySlot
{
    [field: SerializeField] public InventoryItem Item { get; set; }

    [field: SerializeField, Min(0.001f)] public float Capacity { get; set; } = 1f;

    [field: SerializeField, Range(0.001f, 100f)] public float Condition { get; set; } = 100f;


    public bool IsEmpty => Item == null || Capacity == 0;
    public bool IsFull => Item != null && Capacity >= Item.MaxCapacity;


    public void SetItem(InventoryItem item, float count, float condition)
    {
        Item = item;
        Capacity = count;
        Condition = condition;
    }

    public void Clear()
    {
        Item = null;
        Capacity = 0;
        Condition = 0;
    }
}
