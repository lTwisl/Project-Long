using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ClothingSystems
{
    public enum BodyType
    {
        HeadAndBrushes,
        Torso,
        Legs,
        Feet,
        Accessories,
    }

    public class ClothingSystem
    {
        public event Action<InventorySlot> OnEquip;
        public event Action<InventorySlot> OnUnequip;

        public float TotalTemperatureBonus { get; private set; }
        public float TotalFrictionBonus { get; private set; }
        public float TotalOffsetStamina { get; private set; }
        public float TotalPhysicProtection { get; private set; }

        public IEnumerable<ClothingSlotGroup> ClothingSlotGroups => _groups.Values;
        private Dictionary<BodyType, ClothingSlotGroup> _groups;

        private ClothingSystemConfig _config;
        private World _world;



        public ClothingSystem(ClothingSystemConfig config, World world)
        {
            Initialize(config, world);
        }

        public void Initialize(ClothingSystemConfig config, World world)
        {
            _config = config;
            _world = world;

            _groups = new();

            foreach (var t in Enum.GetValues(typeof(BodyType)).Cast<BodyType>())
                _groups.Add(t, new ClothingSlotGroup(_world, _config, t));

            OnEquip += slot => TotalOffsetStamina += (slot.Item as ClothingItem).StaminaBonus;
            OnUnequip += slot => TotalOffsetStamina -= (slot.Item as ClothingItem).StaminaBonus;
        }

        public IEnumerator UpdateGroups(float deltaTime)
        {
            float dt = deltaTime / ClothingSlotGroups.Count();

            while (true)
            {
                float totalTemperatureBonus = 0f;
                float totalFrictionBonus = 0f;
                float totalPhysicProtection = 0f;

                foreach (var item in ClothingSlotGroups)
                {
                    item.UpdateSlots(dt);

                    totalTemperatureBonus += item.TotalTemperatureBonus;
                    totalPhysicProtection += item.TotalPhysicProtection;

                    totalFrictionBonus += item.TotalFrictionBonus;

                    yield return null;
                }

                TotalTemperatureBonus = totalTemperatureBonus;

                TotalFrictionBonus = 0.4f * totalFrictionBonus / (totalFrictionBonus + 0.2f);
                TotalPhysicProtection = totalPhysicProtection / (totalPhysicProtection + 0.5f);
            }
        }

        public bool TryEquip(InventorySlot slot, int index)
        {
            foreach (var item in ClothingSlotGroups)
            {
                if (item.TryEquip(slot, index))
                {
                    OnEquip?.Invoke(slot);
                    return true;
                }
            }

            return true;
        }

        public bool TryUnequip(InventorySlot slot)
        {
            foreach (var item in ClothingSlotGroups)
            {
                if (item.TryUnequip(slot))
                {
                    OnUnequip?.Invoke(slot);
                    return true;
                }
            }

            return false;
        }

        public bool Contains(InventorySlot slot)
        {
            foreach (var item in ClothingSlotGroups)
            {
                if (item.Contains(slot))
                    return true;
            }

            return false;
        }

        public IEnumerable<InventorySlot> GetAllInventorySlots(bool? isOuter = null)
        {
            foreach (var group in ClothingSlotGroups)
                foreach (var slot in group.GetAllInventorySlots(isOuter))
                    yield return slot;
        }

        public IEnumerable<(ClothesType, List<InventorySlot>)> GetAllInventorySlotsByType(bool? isOuter = null)
        {
            foreach (var group in ClothingSlotGroups)
                foreach (var (type, slots) in group.GetAllInventorySlotsByType(isOuter))
                    yield return (type, slots);
        }

        public void ForEachInventorySlot(Action<InventorySlot> action, bool? isOuter = null)
        {
            foreach (var group in ClothingSlotGroups)
                group.ForEachInventorySlot(action, isOuter);
        }

        public void ForEachInventorySlotByType(Action<ClothesType, List<InventorySlot>> action, bool? isOuter = null)
        {
            foreach (var group in ClothingSlotGroups)
                group.ForEachInventorySlotsByType(action, isOuter);
        }

        public override string ToString()
        {
            return $"TotalTemperatureBonus: {TotalTemperatureBonus} | " +
                $"TotalFrictionBonus: {TotalFrictionBonus}\n" +
                $"TotalOffsetStamina: {TotalOffsetStamina} | " +
                $"TotalPhysicProtection: {TotalPhysicProtection}";
        }
    }
}