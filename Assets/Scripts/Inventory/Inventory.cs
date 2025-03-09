using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    private class ItemNameComparer : IComparer<InventorySlot>
    {
        public int Compare(InventorySlot x, InventorySlot y)
        {
            if (x == null || y == null)
                return 0;
            return string.Compare(x.Item.Name, y.Item.Name, StringComparison.Ordinal);
        }
    }

    private class ItemConditionComparer : IComparer<InventorySlot>
    {
        public int Compare(InventorySlot x, InventorySlot y)
        {
            if (x == null || y == null)
                return 0;
            return x.Condition.CompareTo(y.Condition);
        }
    }

    private class ItemWeightComparer : IComparer<InventorySlot>
    {
        public int Compare(InventorySlot x, InventorySlot y)
        {
            if (x == null || y == null)
                return 0;
            return (x.Capacity * x.Item.Weight).CompareTo(y.Capacity * y.Item.Weight);
        }
    }

    public enum SortingFilter
    {
        Alphabet,
        Condition,
        Weight,
    }

    [HideInInspector] public SortingFilter Filter;
    public InventoryItem.ItemType? Categoty;

    public event Action<IReadOnlyInventorySlot> OnItemAdded;
    public event Action<IReadOnlyInventorySlot> OnItemRemoved;

    [SerializeField] public List<InventorySlot> _initSlots;
    public LinkedList<InventorySlot> Slots { get; private set; }

    [field: SerializeField, DisableEdit] public float Weight { get; private set; }

    public void Init()
    {
        Slots = new LinkedList<InventorySlot>(_initSlots);
        RecalculateWeight();
    }

    public void AddItem(InventoryItem item, float capacity, float condition)
    {
        if (item == null || capacity <= 0 || condition <= 0)
            return;

        Weight += item.Weight * capacity;

        OnItemAdded?.Invoke(new InventorySlot(item, capacity, condition));

        if (!item.IsStackable)
        {
            Slots.AddLast(new InventorySlot(item, capacity, condition));
            return;
        }

        InventorySlot slotForStak = Slots.Where(slot => slot.Item == item && slot.Condition == condition && !slot.IsFull).FirstOrDefault();

        if (slotForStak == null)
        {
            Slots.AddLast(new InventorySlot(item, capacity, condition));
            return;
        }

        float newCapacity = slotForStak.Capacity + capacity;

        if (newCapacity <= item.MaxCapacity)
        {
            slotForStak.Capacity = newCapacity;
            return;
        }

        float remains = capacity - (item.MaxCapacity - slotForStak.Capacity);
        slotForStak.Capacity = item.MaxCapacity;

        Slots.AddLast(new InventorySlot(item, remains, condition));
    }

    public void RemoveItem(IReadOnlyInventorySlot slot)
    {
        Slots.Remove(slot as InventorySlot);
    }

    public int CountSlots => Slots.Count;

    public void Clear() => Slots.Clear();

    public bool Contains(InventoryItem item, float minCapacity = -1, float minCondition = -1)
    {
        var slots = Slots.Where(slot => slot.Item == item);

        if (slots.Any())
            return false;

        bool conditionMet = (minCondition <= 0) || slots.Any(slot => slot.Condition >= minCondition);
        bool capacityMet = slots.Sum(slot => slot.Capacity) >= minCapacity;

        return conditionMet && capacityMet;
    }


    public void Update(float deltaTime)
    {
        if (Slots.Count == 0)
            return;

        LinkedListNode<InventorySlot> currentNode = Slots.First;

        while (currentNode != null)
        {
            LinkedListNode<InventorySlot> nextNode = currentNode.Next;
            InventorySlot slot = currentNode.Value;
            bool shouldRemove = false;

            if (slot.IsEmpty)
            {
                shouldRemove = true;
            }
            else
            {
                InventoryItem item = slot.Item;

                if ((item.DegradeType == InventoryItem.DegradationType.Rate && (item is not ClothesItem)) || ((item is ClothesItem) && slot.IsWearing))
                {
                    slot.Condition -= item.DegradationValue * deltaTime;
                    shouldRemove = slot.Condition <= 0;
                }
            }

            if (shouldRemove)
            {
                Slots.Remove(currentNode);
                OnItemRemoved?.Invoke(currentNode.Value);
            }

            currentNode = nextNode;
        }
    }

    public List<IReadOnlyInventorySlot> GetSorteredSlots()
    {
        return GetSorteredSlots(Filter, Categoty);
    }

    public List<IReadOnlyInventorySlot> GetSorteredSlots(SortingFilter filter, InventoryItem.ItemType? category)
    {
        IComparer<InventorySlot> comparer = filter switch
        {
            SortingFilter.Alphabet => new ItemNameComparer(),
            SortingFilter.Condition => new ItemConditionComparer(),
            SortingFilter.Weight => new ItemWeightComparer(),
            _ => throw new NotImplementedException(),
        };

        return new List<IReadOnlyInventorySlot>(GetSorteredSlotsByCategoty(category).OrderBy(s => s, comparer));
    }

    private IEnumerable<InventorySlot> GetSorteredSlotsByCategoty(InventoryItem.ItemType? category)
    {
        if (category != null)
            return Slots.Where(p => p.Item.Category == category);
        
        return Slots;
    }

    public void RecalculateWeight()
    {
        if (Slots.Count == 0)
            return;

        foreach (var slot in Slots)
        {
            Weight += slot.Capacity * slot.Item.Weight;
        }
    }
}
