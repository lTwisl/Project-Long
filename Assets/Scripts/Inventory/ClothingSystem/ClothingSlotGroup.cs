using System.Collections.Generic;

namespace ClothingSystems
{
    public class ClothingSlotGroup
    {
        private const float _tempWet = 1.5f;
        private const float _tempWind = 1.5f;

        private List<InventorySlot> _upperSlots;
        private List<InventorySlot> _lowerSlots;

        private float _totalFrictionBonus;
        private float _totalTemperatureBonus;
        private float _totalWaterProtection;
        private float _totalWindProtection;
        private float _totalToxicityProtection;
        private float _totalPhysicProtection;
        private float _totalOffsetStamina;

        public void UpdateSlots()
        {
            UpdateUpperSlots();
            UpdateLowerSlots();
        }

        private void UpdateUpperSlots()
        {
            _totalFrictionBonus = 0f;
            foreach (var slot in _upperSlots)
            {
                var clothing = slot.Item as ClothingItem;
                float condition = (float)slot.Condition;

                _totalFrictionBonus += clothing.FrictionBonus * condition;

                float tempByClothing = clothing.TemperatureBonus * condition * (1 - slot.Wet * _tempWet);
                float tempByWind = (1 - clothing.WindProtection * condition) * ;
                _totalTemperatureBonus += tempByClothing + tempByWind;

            }
        }

        private void UpdateLowerSlots() 
        { 
            
        }
    }
}
