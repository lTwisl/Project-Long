using System;
using UnityEngine;

[Serializable]
public class InventorySlot : IDisposable
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

            if (_capacity <= 0)
                Dispose();
        }
    }

    [SerializeField, Range(0.001f, 1)] private double _condition = 1f;
    public double Condition
    {
        get => _condition;
        set
        {
            _condition = value;
            OnConditionChanged?.Invoke(_condition);

            if (_condition <= 0)
                Dispose();
        }
    }

    public bool MarkedForRemoval { get; private set; } = false;

    public event Action<float> OnCapacityChanged;
    public event Action<double> OnConditionChanged;

    // Äכÿ ןנוהלועמג מהוזהû
    public bool IsWearing { get; set; }
    [field: SerializeField] public float Wet { get; set; }


    public bool IsEmpty => Item == null || Capacity <= 0 || Condition <= 0;
    public bool IsFull => Item != null && Capacity >= Item.MaxCapacity;


    public InventorySlot(InventoryItem item, float capacity, double condition)
    {
        SetItem(item, capacity, condition);
    }


    public void SetItem(InventoryItem item, float capacity, double condition)
    {
        if (item == null)
            return;

        Item = item;
        Capacity = capacity;
        Condition = condition;
    }


    public void UseItem()
    {
        Item.Use(this);
    }


    public void Clear()
    {
        Item = null;
        Capacity = 0.0f;
        Condition = 0.0f;
    }


    public float GetWeight()
    {
        if (Item is ClothingItem clothes)
            return Capacity * Item.Weight * (1 + clothes.WaterAbsorptionRatio * Wet);
        return Capacity * Item.Weight;
    }

    public void Dispose()
    {
        if (MarkedForRemoval)
            return;

        OnCapacityChanged = null;
        OnConditionChanged = null;

        MarkedForRemoval = true;
    }
}
