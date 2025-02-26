using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    // Алфавит
    private class ItemNameComparer : IComparer<InventorySlot>
    {
        public int Compare(InventorySlot x, InventorySlot y)
        {
            return string.Compare(x.Item.Name, y.Item.Name, StringComparison.Ordinal);
        }
    }

    // Состояние
    private class ItemConditionComparer : IComparer<InventorySlot>
    {
        public int Compare(InventorySlot x, InventorySlot y)
        {
            return x.Condition - y.Condition > 0 ? 1 : -1;
        }
    }

    // Вес
    private class ItemWeightComparer : IComparer<InventorySlot>
    {
        public int Compare(InventorySlot x, InventorySlot y)
        {
            return x.Capacity * x.Item.Weight - y.Capacity * y.Item.Weight > 0 ? 1 : -1;
        }
    }

    public enum SortingFilter
    {
        Alphabet,
        Condition,
        Weight,
    }

    public SortingFilter Filter;
    public InventoryItem.ItemType Categoty;

    [SerializeField] private List<InventorySlot> _slots;

    private float _weight;

    public void AddItem(InventoryItem item, float capacity, float condition)
    {
        _weight += item.Weight * capacity;

        int lastIndex = _slots.FindLastIndex((InventorySlot slot) =>
        {
            return slot.Item == item;
        });

        if (lastIndex == -1)
            lastIndex = _slots.Count - 1;

        if (!item.IsStackable)
        {
            _slots.Add(new InventorySlot() { Item = item, Capacity = capacity, Condition = condition });
            return;
        }

        int indexForStack = _slots.FindIndex((InventorySlot slot) =>
        {
            return slot.Item == item && slot.Condition == condition && !slot.IsFull;
        });

        if (indexForStack == -1)
        {
            _slots.Add(new InventorySlot() { Item = item, Capacity = capacity, Condition = condition });
            return;
        }

        InventorySlot slotForStak = _slots[indexForStack];

        float newCapacity = slotForStak.Capacity + capacity;

        if (newCapacity <= item.MaxCapacity)
        {
            slotForStak.Capacity = newCapacity;
            return;
        }

        float remains = capacity - (item.MaxCapacity - slotForStak.Capacity);
        slotForStak.Capacity = item.MaxCapacity;

        _slots.Add(new InventorySlot() { Item = item, Capacity = remains, Condition = condition });
    }

    public IReadOnlyInventorySlot this[int i]
    {
        get { return _slots[i]; }
    }

    public void Sort()
    {
        IComparer<InventorySlot> comparer = Filter switch
        {
            SortingFilter.Alphabet => new ItemNameComparer(),
            SortingFilter.Condition => new ItemConditionComparer(),
            SortingFilter.Weight => new ItemWeightComparer(),
            _ => throw new NotImplementedException(),
        };

        _slots.Sort(comparer);
    }


    public void Update(float deltaTime)
    {
        for (int i = 0; i < _slots.Count; ++i)
        {
            InventorySlot slot = _slots[i];
            if (slot.Item.DegradeType == InventoryItem.DegradationType.Rate && slot.Item.Category != InventoryItem.ItemType.Clothes)
            {
                slot.Condition -= slot.Item.DegradationValue * deltaTime;

                if (slot.Condition <= 0)
                {
                    _slots.RemoveAt(i);
                }
            }
        }
    }

    public List<IReadOnlyInventorySlot> GetSlotsWithCategoty(InventoryItem.ItemType categoty)
    {
        return _slots.Where(p => p.Item.Category == categoty).Select(p => p as IReadOnlyInventorySlot).ToList();
    }

    
}
