using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

[Serializable]
public class ClothingSlot
{
    public ClothesType ClothesType;
    public int MaxLayers = 1;

    public List<InventorySlot> Layers = new List<InventorySlot>();
}

[Serializable]
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

    public Dictionary<ClothesType, ClothingSlot> SlotCache { get; private set; }

    public IEnumerable<InventorySlot> UpperClothes { get; private set; }

    public event Action<InventorySlot> OnEquip;
    public event Action<InventorySlot> OnUnequip;


    public void Init()
    {
        InitializeSlotCache();
        UpdateUpperClothes();
    }

    private void InitializeSlotCache()
    {
        foreach (var slot in _bodySlots)
        {
            for (var i = 0; i < slot.MaxLayers; i++)
            {
                slot.Layers.Add(new InventorySlot(null, 0.0f, 0.0f));
            }
        }

        SlotCache = _bodySlots.ToDictionary(s => s.ClothesType, s => s);
    }

    private void UpdateUpperClothes()
    {
        UpperClothes = SlotCache.Values.Select(slot => slot.Layers.Find(slot => slot?.Item != null)).Where(slot => slot != null);
    }

    public void Update(float deltaTime)
    {
        Degradation(deltaTime);
        GettingWet(deltaTime);

        Debug.Log(CalculateTotalTemperatureBonus());
    }

    // Деградация одежды
    private void Degradation(float deltaTime)
    {
        foreach (var clothingSlot in SlotCache.Values)
        {
            foreach (var slot in clothingSlot.Layers)
            {
                if (slot == null || slot.Item == null)
                    continue;

                slot.Condition -= slot.Item.DegradationValue * (UpperClothes.Contains(slot) ? 2f : 1f) * deltaTime;
                if (slot.Condition <= 0)
                    slot.IsWearing = false;
            }
        }
    }

    // Намокание одежды
    private void GettingWet(float deltaTime)
    {
        float normTemp = Utility.MapRange(WeatherSystem.Instance.Temperature, 10, 50, 0, 1, true);

        foreach (var clothingSlot in SlotCache.Values)
        {
            bool updateLower = false;
            foreach (var slot in clothingSlot.Layers)
            {
                if (slot == null)
                    continue;

                if (!UpperClothes.Contains(slot) && !updateLower)
                    continue;

                ClothesItem clothesItem = slot.Item as ClothesItem;

                float wetChange = WeatherSystem.Instance.Wetness * (100f - clothesItem.WaterProtection * slot.Condition) / 100f;
                wetChange -= clothesItem.DryingRatio * normTemp;

                slot.Wet = Mathf.Min(slot.Wet + wetChange, 100f) * deltaTime;

                if (slot.Wet >= 100)
                    updateLower = true;
            }
        }
    }

    public bool TryEquip(InventorySlot invSlot, int layer)
    {
        if (invSlot.Item is not ClothesItem item)
            return false;

        if (!TryGetClothesSlot(item.ClothesType, out ClothingSlot slot))
            return false;

        if (layer >= slot.MaxLayers)
            return false;

        slot.Layers[layer] = invSlot;
        invSlot.IsWearing = true;

        UpdateUpperClothes();
        OnEquip?.Invoke(invSlot);

        return true;
    }

    public void Unequip(InventorySlot invSlot)
    {
        if (invSlot == null)
            return;

        HandleItemRemoved(invSlot);
    }

    public void HandleItemRemoved(InventorySlot slot)
    {
        foreach (var clothingSlot in SlotCache.Values)
        {
            int index = clothingSlot.Layers.FindIndex(s => s == slot);

            if (index == -1)
                continue;

            clothingSlot.Layers[index] = new InventorySlot(null, 0.0f, 0.0f);
            slot.IsWearing = false;

            UpdateUpperClothes();
            OnUnequip?.Invoke(slot);

            break;
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

    public float CalculateTotalTemperatureBonus()
    {
        float total = 0;

        float normForceWind = Utility.MapRange(WeatherWindSystem.Instance.CurrentSpeed, 0, 33, 0, 1, true);

        foreach (var clothingSlot in SlotCache.Values)
        {
            foreach (var slot in clothingSlot.Layers)
            {
                //ClothesItem item = slot.Item as ClothesItem;

                if (slot.Item is ClothesItem item)
                {
                    float value = item.TemperatureBonus * slot.Condition / 100;
                    value *= (100 - slot.Wet * 1.5f) / 100;
                    value += (100 - item.WindProtection * slot.Condition) / 100 * WeatherWindSystem.Instance.MaxWindTemperature * normForceWind;

                    total += value;
                }
                else
                {
                    total += WeatherWindSystem.Instance.MaxWindTemperature * normForceWind;
                }
                
            }
        }

        return total;
    }
}
