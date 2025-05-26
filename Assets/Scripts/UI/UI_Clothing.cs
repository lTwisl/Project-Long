using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class UI_Clothing : MonoBehaviour
{
    [SerializeField] private UI_SelectClothing _uiSelectClothes;
    [SerializeField] private TMP_Text _text;

    [SerializeField]
    private SerializedDictionary<ClothesType, List<UI_ClothesSlot>> _uiSlotCache = new SerializedDictionary<ClothesType, List<UI_ClothesSlot>>();

    private ClothingSystem _clothesSystem;

    [Inject]
    private void Construct(Player player)
    {
        _clothesSystem = player.ClothingSystem;
    }

    private void Awake()
    {
        foreach (var group in _uiSlotCache.Values)
        {
            foreach (var slot in group)
            {
                slot.Init(null, _uiSelectClothes);
            }
        }
    }

    private void OnEnable()
    {
        UpdateView();
    }

    public void UpdateView()
    {
        float p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5 = 0;
        foreach (var slot in _clothesSystem.SlotCache.Values)
        {
            if (!_uiSlotCache.TryGetValue(slot.ClothesType, out List<UI_ClothesSlot> layers))
                continue;

            for (int i = 0; i < layers.Count; ++i)
            {
                if (/*slot.Layers.Count <= i*/ slot.Layers[i].Item == null)
                {
                    layers[i].Clear();
                    continue;
                }

                layers[i].Set(slot.Layers[i]);

                var clothes = slot.Layers[i].Item as ClothingItem;
                p1 += clothes.TemperatureBonus;
                p2 += clothes.WaterProtection;
                p3 += clothes.WindProtection;
                p4 += clothes.FrictionBonus;
                p5 += clothes.ToxicityProtection;
            }
        }

        _text.text = $"Temperature = {p1} | Water = {p2} | Wind = {p3} | _friction = {p4} | Toxisity = {p4}";
    }
}
