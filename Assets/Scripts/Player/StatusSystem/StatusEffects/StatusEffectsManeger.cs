using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public struct  UsageItemInfo
{
    public InventoryItem item;
    public float capacity;
    public float condition;
    public TimeSpan timeUsed;
}

public class StatusEffectsManeger : MonoBehaviour
{
    public Queue<UsageItemInfo> LastUsedItems = new(5);

    private void Add(UsageItemInfo item)
    {
        if (LastUsedItems.Count >= 5)
            LastUsedItems.Dequeue();

        LastUsedItems.Enqueue(item);
    }

    private UsageItemInfo _lastUsedItem;
    public UsageItemInfo LastUsedItem 
    {  
        get => _lastUsedItem; 
        set
        {
            _lastUsedItem = value;
            Add(_lastUsedItem);
            OnChangedLastUsedItem?.Invoke(_lastUsedItem);
        }
    }
    public event Action<UsageItemInfo> OnChangedLastUsedItem;

    [SerializeField] private List<StatusEffect> _statusEffects;

    [Inject] private Player player;

    private void Start()
    {
        foreach (var effect in _statusEffects)
            effect.Init(player);

        _statusEffects[0].OnApply();
    }

    private void OnDestroy()
    {
        foreach (var effect in _statusEffects)
            effect.Dispose();
    }
}
