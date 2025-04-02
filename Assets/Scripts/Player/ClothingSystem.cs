using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

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
    private List<ClothingSlot> _bodySlots = new List<ClothingSlot>
    {
        new ClothingSlot { ClothesType = ClothesType.Hat, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Outerwear, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Undershirt, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Gloves, MaxLayers = 1 },
        new ClothingSlot { ClothesType = ClothesType.Pants, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Underpants, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Socks, MaxLayers = 2 },
        new ClothingSlot { ClothesType = ClothesType.Boots, MaxLayers = 1 },
        new ClothingSlot { ClothesType = ClothesType.Accessories, MaxLayers = 2 }
    };

    public Dictionary<ClothesType, ClothingSlot> SlotCache { get; private set; }

    public IEnumerable<InventorySlot> UpperClothes { get; private set; }

    public event Action<InventorySlot> OnEquip;
    public event Action<InventorySlot> OnUnequip;

    public float TotalTemperatureBonus { get; private set; }
    public float TotalFrictionBonus { get; private set; }
    public float TotalOffsetStamina { get; private set; }
    public float TotalToxicityProtection { get; private set; }

    private readonly World _world;

    [Inject]
    public ClothingSystem(World world)
    {
        _world = world;
    }

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

        RecalculateTotalTemperatureBonus();
        RecalculateTotalFrictionBonus();
        RecalculateTotalToxicityBonus();
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

                if (clothingSlot.ClothesType == ClothesType.Accessories)
                    slot.Condition -= slot.Item.DegradationValue * deltaTime;
                else
                    slot.Condition -= slot.Item.DegradationValue * (UpperClothes.Contains(slot) ? 2f : 1f) * deltaTime;

                if (slot.Condition <= 0)
                    slot.IsWearing = false;

            }
        }
    }

    // Намокание одежды
    private void GettingWet(float deltaTime)
    {
        float normTemp = Utility.MapRange(_world.Weather.Temperature, 10, 50, 0, 1, true);

        foreach (var clothingSlot in SlotCache.Values)
        {
            if (clothingSlot.ClothesType == ClothesType.Accessories)
                continue;

            bool updateLower = false;
            foreach (var slot in clothingSlot.Layers)
            {
                if (slot == null)
                    continue;

                if (!UpperClothes.Contains(slot) && !updateLower)
                    continue;

                ClothingItem clothesItem = slot.Item as ClothingItem;

                float wetChange = _world.Weather.Wetness * (100f - clothesItem.WaterProtection * slot.Condition) / 100f;
                wetChange -= clothesItem.DryingRate * normTemp;

                slot.Wet = Mathf.Min(slot.Wet + wetChange, 100f) * deltaTime;

                if (slot.Wet >= 100)
                    updateLower = true;
            }
        }
    }

    public bool TryEquip(InventorySlot invSlot, int layer)
    {
        if (invSlot.Item is not ClothingItem item)
            return false;

        if (!TryGetClothesSlot(item.ClothingType, out ClothingSlot slot))
            return false;

        if (layer >= slot.MaxLayers)
            return false;

        slot.Layers[layer] = invSlot;
        invSlot.IsWearing = true;


        UpdateUpperClothes();
        TotalOffsetStamina += (invSlot.Item as ClothingItem).OffsetStamina;

        invSlot.UseItem();

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
            TotalOffsetStamina -= (slot.Item as ClothingItem).OffsetStamina;

            OnUnequip?.Invoke(slot);
            break;
        }
    }

    public bool TryGetClothesSlot(ClothesType clothesType, out ClothingSlot clothingSlot)
    {
        return SlotCache.TryGetValue(clothesType, out clothingSlot);
    }

    public bool Contains(InventorySlot item)
    {
        foreach (var slot in SlotCache.Values)
        {
            if (slot.Layers.Contains(item))
                return true;
        }

        return false;
    }

    private void RecalculateTotalTemperatureBonus()
    {
        TotalTemperatureBonus = 0;

        float t = _world.GetWindLocalIntensity();
        float normForceWind = Utility.MapRange(_world.GetWindLocalIntensity(), 0, 33, 0, 1, true);

        foreach (var clothingSlot in SlotCache.Values)
        {
            foreach (var slot in clothingSlot.Layers)
            {
                if (slot.Item is ClothingItem item)
                {
                    float value = item.TemperatureBonus * slot.Condition / 100;
                    value *= (100 - slot.Wet * 1.5f) / 100;
                    value += (100 - item.WindProtection * slot.Condition) / 100 * _world.Wind.MaxWindTemperature * normForceWind;

                    TotalTemperatureBonus += value;
                }
                else
                {
                    TotalTemperatureBonus += _world.Wind.MaxWindTemperature * normForceWind;
                }

            }
        }
    }

    private void RecalculateTotalFrictionBonus()
    {
        TotalFrictionBonus = 0;
        foreach (var clothingSlot in SlotCache.Values)
        {
            foreach (var slot in clothingSlot.Layers)
            {
                if (slot.Item is not ClothingItem item)
                    continue;

                TotalFrictionBonus += item.FrictionBonus * slot.Condition / 100f;
            }
        }
    }

    private void RecalculateTotalToxicityBonus()
    {
        TotalToxicityProtection = 0;
        foreach (var clothingSlot in SlotCache.Values)
        {
            foreach (var slot in clothingSlot.Layers)
            {
                if (slot.Item is not ClothingItem item)
                    continue;

                TotalToxicityProtection += item.ToxicityProtection * slot.Condition / 100f;
            }
        }
    }
}
