using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClothingSystems
{
    [System.Serializable]
    public class ClothingSlotGroup
    {
        private readonly BodyType _bodyType;
        private readonly Dictionary<ClothesType, ClothingSlot> _slotMap = new();

        private const float _tempWet = 0.5f;

        [field: SerializeField, DisableEdit] public float TotalFrictionBonus { get; private set; }
        [field: SerializeField, DisableEdit] public float TotalTemperatureBonus { get; private set; }
        [field: SerializeField, DisableEdit] public float TotalToxicityProtection { get; private set; }
        [field: SerializeField, DisableEdit] public float TotalPhysicProtection { get; private set; }
        [field: SerializeField, DisableEdit] public float TotalOffsetStamina { get; private set; }

        public BodyType BodyType => _bodyType;

        private readonly World _world;
        private readonly ClothingSystemConfig _config;


        // Кэшируем часто используемые значения
        private bool _allUpperWet;
        private bool _allUpperDry;
        private float _dryingRateFactor;

        public float MaxToxicityProtection => _config.GetToxicityProtection(_bodyType);

        public ClothingSlotGroup(World world, ClothingSystemConfig config, BodyType bodyType)
        {
            _world = world;
            _config = config;

            _bodyType = bodyType;

            foreach (var type in config.GetClothesTypes(_bodyType))
                _slotMap.Add(type, new(type, _config.GetIsOuter(type), _config.GetCountSlots(type)));
        }

        public void UpdateSlots(float deltaTime)
        {
            TotalFrictionBonus = 0f;
            TotalTemperatureBonus = 0f;
            TotalToxicityProtection = 0f;
            TotalOffsetStamina = 0f;
            TotalPhysicProtection = 0f;

            _dryingRateFactor = Utility.MapRange(_world.TotalTemperature, 10, 80, 0, 1, true);

            // Определяем состояние верхней одежды один раз в кадре
            _allUpperWet = true;
            _allUpperDry = true;

            foreach (var item in _slotMap.Values)
            {
                if (!item.IsOuter)
                    continue;

                foreach (var slot in item.Slots)
                {
                    if (slot == null)
                        continue;

                    if (slot.Wet < 1 - float.Epsilon)
                        _allUpperWet = false;
                    if (slot.Wet > float.Epsilon)
                        _allUpperDry = false;

                    if (!_allUpperWet && !_allUpperDry)
                        break;
                }
            }

            UpdateAllSlots(deltaTime);
        }

        private void UpdateAllSlots(float deltaTime)
        {
            float normForceWindRatio = Utility.MapRange(_world.Wind.CurrentSpeed, 0, WeatherWindSystem.MaxWindIntensity, 0, 1, true);

            foreach (var item in _slotMap.Values)
            {
                for (int i = 0; i < item.Slots.Count; ++i)
                {
                    var slot = item.Slots[i];

                    
                    if (slot == null)
                    {
                        if (BodyType != BodyType.Accessories)
                            TotalTemperatureBonus += WeatherWindSystem.MaxWindTemperature * normForceWindRatio / _config.GetSummaryCountSlots();
                        continue;
                    }

                    if (slot.Item is not ClothingItem clothing)
                    {
                        if (BodyType != BodyType.Accessories)
                            TotalTemperatureBonus += WeatherWindSystem.MaxWindTemperature * normForceWindRatio / _config.GetSummaryCountSlots();
                        continue;
                    }
                    

                    float condition = (float)slot.Condition;

                    if (item.IsOuter)
                    {
                        TotalFrictionBonus += clothing.FrictionBonus * condition;

                        float addWet = _world.TotalWetness * (1 - clothing.WaterProtection * condition) -
                            (clothing.DryingRate * _dryingRateFactor);

                        slot.Wet = Mathf.Clamp01(slot.Wet + addWet * deltaTime);
                    }
                    else
                    {
                        float addWet = _world.TotalWetness * (1 - clothing.WaterProtection * condition) -
                            (clothing.DryingRate * _dryingRateFactor);

                        if ((_allUpperWet && addWet >= 0) || (_allUpperDry && addWet < 0))
                            slot.Wet = Mathf.Clamp01(slot.Wet + addWet * deltaTime);
                    }


                    float tempByClothing = clothing.TemperatureBonus * condition - clothing.TemperatureBonus * slot.Wet * _tempWet;

                    float tempByWind = 0;
                    if (BodyType != BodyType.Accessories)
                        tempByWind = (1 - clothing.WindProtection * condition) * WeatherWindSystem.MaxWindTemperature * normForceWindRatio / _config.GetSummaryCountSlots();

                    TotalTemperatureBonus += tempByClothing + tempByWind;

                    TotalToxicityProtection += clothing.ToxicityProtection * condition;
                    TotalToxicityProtection = Mathf.Clamp(TotalToxicityProtection, 0, MaxToxicityProtection);

                    TotalPhysicProtection += clothing.PhysicProtection;

                    TotalOffsetStamina += clothing.StaminaBonus;

                    slot.Condition -= clothing.DegradationValue * deltaTime * _world.DegradationScale;

                    if (slot.MarkedForRemoval)
                        item.Slots[i] = null;
                }
            }
        }

        public bool TryEquip(InventorySlot slot, int index)
        {
            if (slot == null)
                return false;

            if (slot.Item is not ClothingItem clothing)
                return false;

            if (_slotMap.TryGetValue(clothing.ClothingType, out var slotList))
            {
                if (slotList.Slots.Count <= index || index < 0)
                    return false;

                slotList.Slots[index] = slot;
                slot.IsWearing = true;
                return true;
            }

            return false;
        }

        public bool TryUnequip(InventorySlot slot)
        {
            foreach (var item in _slotMap.Values)
            {
                int index = item.Slots.IndexOf(slot);
                if (index == -1)
                    continue;
                
                item.Slots[index] = null;
                slot.IsWearing = false;
                return true;
            }

            return false;
        }

        public bool Contains(InventorySlot slot)
        {
            if (slot.Item is not ClothingItem clothing)
                return false;

            if (!_slotMap.TryGetValue(clothing.ClothingType, out var slots))
                return false;

            return slots.Slots.Contains(slot);
        }

        public IEnumerable<(ClothesType, List<InventorySlot>)> GetAllInventorySlotsByType(bool? isOuter = null)
        {
            if (isOuter.HasValue)
            {
                foreach (var clothingSlot in _slotMap.Values)
                {
                    if (clothingSlot.IsOuter != isOuter.Value)
                        continue;

                    yield return (clothingSlot.Type, clothingSlot.Slots);
                }
            }
            else
            {
                foreach (var clothingSlot in _slotMap.Values)
                    yield return (clothingSlot.Type, clothingSlot.Slots);
            }
        }

        public IEnumerable<InventorySlot> GetAllInventorySlots(bool? isOuter = null)
        {
            if (isOuter.HasValue)
            {
                foreach (var clothingSlot in _slotMap.Values)
                {
                    if (isOuter != null && clothingSlot.IsOuter != isOuter)
                        continue;

                    foreach (var slot in clothingSlot.Slots)
                        yield return slot;
                }
            }
            else
            {
                foreach (var clothingSlot in _slotMap.Values)
                {
                    foreach (var slot in clothingSlot.Slots)
                        yield return slot;
                }
            }
        }

        public void ForEachInventorySlot(Action<InventorySlot> action, bool? isOuter = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (isOuter.HasValue)
            {
                bool targetIsOuter = isOuter.Value;
                foreach (var clothingSlot in _slotMap.Values)
                {
                    if (clothingSlot.IsOuter != targetIsOuter)
                        continue;

                    foreach (var slot in clothingSlot.Slots)
                        action(slot);
                }
            }
            else
            {
                foreach (var clothingSlot in _slotMap.Values)
                    foreach (var slot in clothingSlot.Slots)
                        action(slot);
            }
        }

        public void ForEachInventorySlotsByType(Action<ClothesType, List<InventorySlot>> action, bool? isOuter = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (isOuter.HasValue)
            {
                bool targetIsOuter = isOuter.Value;
                foreach (var clothingSlot in _slotMap.Values)
                {
                    if (clothingSlot.IsOuter != targetIsOuter)
                        continue;

                    action(clothingSlot.Type, clothingSlot.Slots);
                }
            }
            else
            {
                foreach (var clothingSlot in _slotMap.Values)
                    action(clothingSlot.Type, clothingSlot.Slots);
            }
        }

        private class ClothingSlot
        {
            public readonly ClothesType Type;
            public readonly bool IsOuter;
            public readonly List<InventorySlot> Slots;

            public ClothingSlot(ClothesType type, bool isOuter, int count)
            {
                Type = type;
                IsOuter = isOuter;
                Slots = new(count);
                for (int i = 0; i < count; ++i)
                    Slots.Add(null);
            }
        }
    }
}