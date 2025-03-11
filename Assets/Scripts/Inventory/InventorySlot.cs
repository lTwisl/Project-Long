using System;
using UnityEngine;

public interface IReadOnlyInventorySlot
{
    public event Action<float> OnCapacityChanged;
    public event Action<float> OnConditionChanged;

    public InventoryItem Item { get; }
    public float Capacity { get; }
    public float Condition { get; }
}


[Serializable]
public class InventorySlot : IReadOnlyInventorySlot
{
    [field: SerializeField] public InventoryItem Item { get; private set; }

    [SerializeField, Min(0.001f)] private float _capacity = 1f;
    public float Capacity 
    {
        get => _capacity;
        set
        {
            _capacity = value;
            OnCapacityChanged?.Invoke(_capacity);
        }
    } 

    [SerializeField, Range(0.001f, 100f)] private float _condition = 100f;
    public float Condition 
    {
        get => _condition; 
        set
        {
            _condition = value;
            OnConditionChanged?.Invoke(_condition);
        }
    }

    public event Action<float> OnCapacityChanged;
    public event Action<float> OnConditionChanged;

    // Äכÿ ןנוהלועמג מהוזהû
    public bool IsWearing { get; set; }
    public float Wet { get; set; }


    public bool IsEmpty => Item == null || Capacity <= 0;
    public bool IsFull => Item != null && Capacity >= Item.MaxCapacity;

    public InventorySlot(InventoryItem item, float capacity, float condition)
    {
        SetItem(item, capacity, condition);
    }

    private void SetItem(InventoryItem item, float capacity, float condition)
    {
        if (item == null)
            return;

        Item = item;
        Capacity = capacity;
        Condition = condition;
    }

    public void UseItem(Player player)
    {
        Item.Use(player);

        Capacity -= Item.CostOfUse;
    }
}
