using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ClothingSlot
{
    public ClothesType ClothesType;
    public int MaxLayers = 1;

    public List<InventorySlot> Layers = new List<InventorySlot>();
}

[System.Serializable]
public class ClothingSystem
{
    [SerializeField]
    private List<ClothingSlot> _bodySlots = new List<ClothingSlot>
    {
        new ClothingSlot { ClothesType = ClothesType.Hat, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Outerwear, MaxLayers = 3 },
        new ClothingSlot { ClothesType = ClothesType.Undergarments, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Gloves, MaxLayers = 1 },
        new ClothingSlot { ClothesType = ClothesType.Trousers, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Underpants, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Socks, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Boots, MaxLayers = 1 },
        new ClothingSlot { ClothesType = ClothesType.Accessories, MaxLayers = 2 }
    };

    [SerializeField] public Dictionary<ClothesType, ClothingSlot> SlotCache {  get; private set; }

    public void Init()
    {
        InitializeSlotCache();
    }

    private void InitializeSlotCache()
    {
        SlotCache = _bodySlots.ToDictionary(s => s.ClothesType, s => s);
    }

    public bool TryEquip(InventorySlot invSlot, int layer)
    {
        if (invSlot.Item is not ClothesItem item)
            return false;

        if (!SlotCache.TryGetValue(item.ClothesType, out ClothingSlot slot))
            return false;

        if (layer >= slot.MaxLayers)
            return false;

        while (slot.Layers.Count <= layer)
            slot.Layers.Add(null);

        slot.Layers[layer] = invSlot;
        invSlot.IsWearing = true;
        return true;
    }

    public void Unequip(InventorySlot invSlot)
    {
        if (invSlot == null)
            return;

        HandleItemRemoved(invSlot);
        invSlot.IsWearing = false;
    }

    public void HandleItemRemoved(IReadOnlyInventorySlot slot)
    {
        foreach (var clothingSlot in SlotCache.Values)
        {
            clothingSlot.Layers.RemoveAll(layer => layer == slot);
        }
    }

    public bool TryGetClothesSlot(ClothesType clothesType, out ClothingSlot clothingSlot)
    {
        return SlotCache.TryGetValue(clothesType, out clothingSlot);
    }

    public bool Contains(IReadOnlyInventorySlot item)
    {
        foreach (var slot in SlotCache.Values)
        {
            if (slot.Layers.Contains(item))
                return true;
        }

        return false;
    }
}
