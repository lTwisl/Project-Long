using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

public class UI_Clothing : MonoBehaviour
{
    [SerializeField] private List<UI_ClothesSlot> _bodySlots;
    [SerializeField] private UI_SelectClothing _uiSelectClothes;
    [SerializeField] private TMP_Text _text;

    private Dictionary<ClothesType, List<UI_ClothesSlot>> _uiSlotCache = new Dictionary<ClothesType, List<UI_ClothesSlot>>();

    private ClothingSystem _clothesSystem;

    [Inject]
    private void Construct(Player player)
    {
        _clothesSystem = player.ClothingSystem;
    }

    private void Awake()
    {
        _bodySlots.ForEach(slot => slot.Init(null, _uiSelectClothes));

        var groupedSlots = _bodySlots
            .GroupBy(slot => slot.ClothesType)
            .Select(group => new 
            {
                Type = group.Key,
                Slots = group.OrderBy(slot => slot.IndexLayer).ToList()
            });

        foreach (var group in groupedSlots)
        {
            _uiSlotCache[group.Type] = group.Slots;
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
                p5 += clothes.ToxisityProtection;
            }
        }

        _text.text = $"Temp = {p1} | Water = {p2} | Wind = {p3} | Friction = {p4} | Toxisity = {p4}";
    }
}
