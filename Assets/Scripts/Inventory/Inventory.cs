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
    public Category? Categoty;

    public event Action<InventorySlot> OnItemAdded;
    public event Action<InventorySlot> OnItemRemoved;


    [SerializeField] public List<InventorySlot> InitSlots = new();
    public LinkedList<InventorySlot> Slots { get; private set; }

    [field: SerializeField, DisableEdit] public float Weight { get; private set; }

    private float _degradationScale = 1f;
    public float DegradationScale
    {
        get => _degradationScale;
        set => _degradationScale = Mathf.Max(0, value);
    }

    public void Init()
    {
        Slots = new LinkedList<InventorySlot>(InitSlots);

        RecalculateWeight();
    }

    /// <summary>
    /// Основной метод добавления предмета в инвентарь
    /// </summary>
    public void AddItem(InventoryItem item, float quantity, double condition)
    {
        if (!IsValidAdditionParameters(item, quantity, condition))
            return;

        Weight += item.Weight * quantity;

        if (!item.IsStackable)
        {
            CreateNewSlot(item, quantity, condition);
            return;
        }

        TryAddToExistingSlots(item, quantity, condition);
    }

    /// <summary>
    /// Проверяет валидность входных параметров для добавления предмета
    /// </summary>
    /// <returns>
    /// True - если предмет существует, количество и состояние положительные,
    /// а вес предмета корректен
    /// </returns>
    private bool IsValidAdditionParameters(InventoryItem item, float quantity, double condition)
    {
        return item != null
            && quantity > 0
            && condition > 0
            && item.Weight >= 0;
    }

    /// <summary>
    /// Пытается добавить предмет в существующие подходящие слоты,
    /// создает новые слоты для остатка
    /// </summary>
    private void TryAddToExistingSlots(InventoryItem item, float quantity, double condition)
    {
        float remainingQuantity = quantity;
        InventorySlot existingSlot = FindPartialSlot(item, condition);

        if (existingSlot != null)
        {
            remainingQuantity = FillExistingSlot(existingSlot, remainingQuantity, item.MaxCapacity);
        }

        if (remainingQuantity > 0)
        {
            CreateNewSlotsWithRemaining(item, remainingQuantity, condition, item.MaxCapacity);
        }
    }

    /// <summary>
    /// Ищет частично заполненный слот с совпадающими характеристиками
    /// </summary>
    /// <returns>
    /// Первый найденный слот, удовлетворяющий условиям:
    /// - Тот же тип предмета
    /// - Такое же состояние
    /// - Не заполнен до максимума
    /// </returns>
    private InventorySlot FindPartialSlot(InventoryItem item, double condition)
    {
        return Slots.FirstOrDefault(slot =>
            slot.Item == item
            && (int)(slot.Condition * 100) == (int)(condition * 100)
            && !slot.IsFull);
    }

    /// <summary>
    /// Заполняет существующий слот и возвращает остаток количества
    /// </summary>
    /// <returns>Не поместившееся количество</returns>
    private float FillExistingSlot(InventorySlot slot, float quantity, float maxCapacity)
    {
        float availableSpace = maxCapacity - slot.Capacity;
        float addedQuantity = Math.Min(quantity, availableSpace);

        slot.Capacity += addedQuantity;
        OnItemAdded?.Invoke(new InventorySlot(slot.Item, addedQuantity, slot.Condition));

        return quantity - addedQuantity;
    }

    /// <summary>
    /// Создает новые слоты для оставшегося количества, распределяя:
    /// 1. Полные слоты до максимальной вместимости
    /// 2. Один слот для остатка (если есть)
    /// </summary>
    private void CreateNewSlotsWithRemaining(InventoryItem item, float quantity, double condition, float maxCapacity)
    {
        int fullSlotsCount = (int)(quantity / maxCapacity);
        float remainder = quantity % maxCapacity;

        for (int i = 0; i < fullSlotsCount; i++)
        {
            CreateNewSlot(item, maxCapacity, condition);
        }

        if (remainder > 0)
        {
            CreateNewSlot(item, remainder, condition);
        }
    }

    /// <summary>
    /// Создает новый слот и инициирует событие добавления предмета
    /// </summary>
    private void CreateNewSlot(InventoryItem item, float quantity, double condition)
    {
        InventorySlot newSlot = new InventorySlot(item, quantity, condition);
        Slots.AddLast(newSlot);
        OnItemAdded?.Invoke(newSlot);
    }



    public void RemoveItem(InventorySlot slot)
    {
        if (!Slots.Remove(slot))
            return;

        Weight -= slot.GetWeight();
        OnItemRemoved?.Invoke(slot);
    }


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

                // Деградация предмета
                if (item.DegradeType == DegradationType.Rate && item is not ClothingItem && item is not HeatingItem)
                {
                    slot.Condition -= item.DegradationValue * DegradationScale * deltaTime;
                    shouldRemove = slot.Condition <= 0;
                }
            }

            if (shouldRemove)
            {
                RemoveItem(slot);
            }

            currentNode = nextNode;
        }
    }


    public List<InventorySlot> GetSorteredSlots()
    {
        return GetSorteredSlots(Filter, Categoty);
    }


    public List<InventorySlot> GetSorteredSlots(SortingFilter filter, Category? category)
    {
        IComparer<InventorySlot> comparer = filter switch
        {
            SortingFilter.Alphabet => new ItemNameComparer(),
            SortingFilter.Condition => new ItemConditionComparer(),
            SortingFilter.Weight => new ItemWeightComparer(),
            _ => throw new NotImplementedException(),
        };

        return new List<InventorySlot>(GetSorteredSlotsByCategoty(category).OrderBy(s => s, comparer));
    }


    private IEnumerable<InventorySlot> GetSorteredSlotsByCategoty(Category? category)
    {
        if (category != null)
            return Slots.Where(slot => slot.Item.Category == category);

        return Slots;
    }


    public void RecalculateWeight()
    {
        Weight = 0;

        if (Slots.Count == 0)
            return;

        foreach (var slot in Slots)
        {
            Weight += slot.GetWeight();
        }
    }
}
