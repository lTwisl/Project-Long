using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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
            if (x == null || y == null)
                return 0;
            return x.Condition.CompareTo(y.Condition);
        }
    }

    // Вес
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

    [SerializeField] private List<InventorySlot> _initSlots;
    public LinkedList<InventorySlot> Slots { get; private set; }

    [field: SerializeField, DisableEdit] public float Weight { get; private set; }

    public void Init()
    {
        Slots = new LinkedList<InventorySlot>(_initSlots);
    }

    public void AddItem(InventoryItem item, float capacity, float condition)
    {
        if (item == null || capacity <= 0 || condition <= 0)
            return;

        Weight += item.Weight * capacity;

        OnItemAdded?.Invoke(new InventorySlot(item, capacity, condition));

        if (!item.IsStackable)
        {
            //_slots.Add(new InventorySlot() { Item = item, Capacity = capacity, Condition = condition });
            Slots.AddLast(new InventorySlot(item, capacity, condition));
            return;
        }

        //int indexForStack = _slots.FindIndex(slot => slot.Item == item && slot.Condition == condition && !slot.IsFull);
        InventorySlot slotForStak = Slots.Where(slot => slot.Item == item && slot.Condition == condition && !slot.IsFull).FirstOrDefault();

        if (slotForStak == null)
        {
            Slots.AddLast(new InventorySlot(item, capacity, condition));
            return;
        }

        /*if (indexForStack == -1)
        {
            //_slots.Add(new InventorySlot() { Item = item, Capacity = capacity, Condition = condition });
            _slots.AddLast(new InventorySlot() { Item = item, Capacity = capacity, Condition = condition });
            return;
        }

        InventorySlot slotForStak = _slots[indexForStack];*/

        float newCapacity = slotForStak.Capacity + capacity;

        if (newCapacity <= item.MaxCapacity)
        {
            slotForStak.Capacity = newCapacity;
            return;
        }

        float remains = capacity - (item.MaxCapacity - slotForStak.Capacity);
        slotForStak.Capacity = item.MaxCapacity;

        //_slots.Add(new InventorySlot() { Item = item, Capacity = remains, Condition = condition });
        Slots.AddLast(new InventorySlot(item, remains, condition));
    }

    public void RemoveItem(IReadOnlyInventorySlot slot)
    {
        Slots.Remove(slot as InventorySlot);
    }

    /*public IReadOnlyInventorySlot this[int i]
    {
        get { return Slots[i]; }
    }*/

    public int CountSlots => Slots.Count;

    public void Clear() => Slots.Clear();

    /*public void Sort()
    {
        IComparer<InventorySlot> comparer = Filter switch
        {
            SortingFilter.Alphabet => new ItemNameComparer(),
            SortingFilter.Condition => new ItemConditionComparer(),
            SortingFilter.Weight => new ItemWeightComparer(),
            _ => throw new NotImplementedException(),
        };

        Slots.Sort(comparer);
    }*/

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
                    //if (item.Category != InventoryItem.ItemType.Clothes)
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

        var slots = new List<IReadOnlyInventorySlot>(Slots.OrderBy(s => s, comparer));

        if (category == null)
            return slots;

        return new List<IReadOnlyInventorySlot>(slots.Where(p => p.Item.Category == category));
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
