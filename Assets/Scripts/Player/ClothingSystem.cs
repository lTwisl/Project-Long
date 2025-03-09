using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ClothingSlot
{
    public ClothesItem.ClothesType Region;
    public int MaxLayers = 1;

    public List<InventorySlot> Layers = new List<InventorySlot>();
}

[System.Serializable]
public class ClothingSystem
{
    [SerializeField]
    private List<ClothingSlot> _bodySlots = new List<ClothingSlot>
    {
        new ClothingSlot { Region = ClothesItem.ClothesType.Hat, MaxLayers = 2 },
        new ClothingSlot { Region = ClothesItem.ClothesType.Outerwear, MaxLayers = 3 },
        new ClothingSlot { Region = ClothesItem.ClothesType.Undergarments, MaxLayers = 2 },
        new ClothingSlot { Region = ClothesItem.ClothesType.Gloves, MaxLayers = 1 },
        new ClothingSlot { Region = ClothesItem.ClothesType.Trousers, MaxLayers = 2 },
        new ClothingSlot { Region = ClothesItem.ClothesType.Underpants, MaxLayers = 2 },
        new ClothingSlot { Region = ClothesItem.ClothesType.Socks, MaxLayers = 2 },
        new ClothingSlot { Region = ClothesItem.ClothesType.Boots, MaxLayers = 1 },
        new ClothingSlot { Region = ClothesItem.ClothesType.Accessories, MaxLayers = 2 }
    };

    [SerializeField] public Dictionary<ClothesItem.ClothesType, ClothingSlot> SlotCache {  get; private set; }

    public void Init()
    {
        InitializeSlotCache();
    }

    private void InitializeSlotCache()
    {
        SlotCache = _bodySlots.ToDictionary(s => s.Region, s => s);
    }

    public bool TryEquip(InventorySlot invSlot, int layer)
    {
        if (invSlot.Item is not ClothesItem item)
            return false;

        if (!SlotCache.TryGetValue(item.TypeClothes, out ClothingSlot slot))
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

    public bool TryGetClothesSlot(ClothesItem.ClothesType Region, out ClothingSlot clothingSlot)
    {
        return SlotCache.TryGetValue(Region, out clothingSlot);
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
