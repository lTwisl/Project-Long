using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class UI_Clothing : MonoBehaviour
{
    [Inject] private Player _player;

    [SerializeField] private List<UI_ClothesSlot> _bodySlots;

    private Dictionary<ClothesItem.ClothesType, List<UI_ClothesSlot>> _uiSlotCache = new Dictionary<ClothesItem.ClothesType, List<UI_ClothesSlot>>();

    private void Awake()
    {
        var groupedSlots = _bodySlots
            .GroupBy(slot => slot.Region)
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
        foreach (var slot in _player.ClothingSystem.SlotCache.Values)
        {
            if (!_uiSlotCache.TryGetValue(slot.Region, out List<UI_ClothesSlot> layers))
                continue;

            for (int i = 0; i < Mathf.Min(slot.Layers.Count, layers.Count); ++i)
            {
                layers[i].Init(slot.Layers[i]);
            }
        }
    }
}
