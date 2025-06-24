using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClothingSystems
{

    [CreateAssetMenu(fileName = "ClothingSystemConfig", menuName = "ClothingSystemConfig")]
    public class ClothingSystemConfig : ScriptableObject
    {
        [Serializable]
        public struct BodyTypeData
        {
            [field: SerializeField, Range(0, 1)] public float ToxicityProtection { get; private set; }

            [SerializeField] private ClothesType[] _clothesTypes;
            public readonly IReadOnlyList<ClothesType> ClothesTypes => _clothesTypes;
        }

        [Serializable]
        public struct ClothesTypeData
        {
            [field: SerializeField, Min(1)] public int CountSlots { get; private set; }
            [field: SerializeField] public bool IsOuter { get; private set; }
        }

        [field: SerializeField] public float DegradationScaleOutside { get; private set; } = 2;

        [SerializedDictionary("Body Type")]
        [field: SerializeField] private SerializedDictionary<BodyType, BodyTypeData> _bodyTypeDataMap = new();

        [SerializedDictionary("Clothes Type")]
        [field: SerializeField] private SerializedDictionary<ClothesType, ClothesTypeData> _clothesTypeDataMap = new();

        public IReadOnlyDictionary<BodyType, BodyTypeData> BodyTypeDataMap => _bodyTypeDataMap;
        public IReadOnlyDictionary<ClothesType, ClothesTypeData> ClothesTypeDataMap => _clothesTypeDataMap;
        private int _summaryCountSlots;


        public bool GetIsOuter(ClothesType type) => _clothesTypeDataMap[type].IsOuter;
        public int GetCountSlots(ClothesType type) => _clothesTypeDataMap[type].CountSlots;
        public int GetSummaryCountSlots()
        {
            if (_summaryCountSlots != 0)
                return _summaryCountSlots;

            int count = 0;
            foreach (var kvp in _bodyTypeDataMap)
            {
                if (kvp.Key == BodyType.Accessories)
                    continue;

                foreach (var kvp2 in kvp.Value.ClothesTypes)
                {
                    count += GetCountSlots(kvp2);
                }
            }

            _summaryCountSlots = count;
            return count;
        }

        public float GetToxicityProtection(BodyType type) => _bodyTypeDataMap[type].ToxicityProtection;
        public IReadOnlyList<ClothesType> GetClothesTypes(BodyType type) => _bodyTypeDataMap[type].ClothesTypes;


        private void Reset()
        {
            _bodyTypeDataMap.Clear();
            foreach (var t in Enum.GetValues(typeof(BodyType)).Cast<BodyType>())
                _bodyTypeDataMap.Add(t, new BodyTypeData());

            _clothesTypeDataMap.Clear();
            foreach (var t in Enum.GetValues(typeof(ClothesType)).Cast<ClothesType>())
                _clothesTypeDataMap.Add(t, new ClothesTypeData());

            _summaryCountSlots = GetSummaryCountSlots();
        }
    }
}