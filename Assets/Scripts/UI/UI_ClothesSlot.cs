using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_ClothesSlot : MonoBehaviour, IPointerDownHandler
{
    [Inject] private Player _player;

    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _wet;
    [SerializeField] private TMP_Text _type;

    public ClothesItem.ClothesType Region;
    public int IndexLayer;
    public InventorySlot Slot { get; private set; }

    private UI_SelectClothes _uiSelectClothes;

    private void Awake()
    {
        _type.text = Region.ToString();
    }

    public void Init(InventorySlot slot, UI_SelectClothes uiSelectClothes)
    {
        Slot = slot;
        _uiSelectClothes = uiSelectClothes;

        UpdateView();
    }

    private void OnEnable()
    {
        if (Slot == null)
            return;

        UpdateView();
    }

    public void Set(InventorySlot slot)
    {
        Slot = slot;
        UpdateView();
    }

    public void Clear()
    {
        Slot = null;
        UpdateView();
    }

    public void UpdateView()
    {
        if (Slot == null)
        {
            _icon.sprite = null;
            _wet.text = "Wet";
            return;
        }

        _icon.sprite = Slot.Item.Icon;
        _wet.text = Slot.Wet.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        List<IReadOnlyInventorySlot> slots = new();
        foreach (var slot in _player.Inventory.Slots)
        {
            if (slot.Item.Category == InventoryItem.ItemType.Clothes
                && (slot.Item as ClothesItem).TypeClothes == Region)
            {
                if (_player.ClothingSystem.SlotCache.TryGetValue(Region, out ClothingSlot clothingSlot))
                {
                    if (clothingSlot.Layers.Contains(slot) && Slot != slot)
                        continue;
                    else
                        slots.Add(slot);
                }
                else
                    slots.Add(slot);
            }
        }

        _uiSelectClothes.Slots = slots;
        _uiSelectClothes.CurrentUiClothesSlot = this;
        _uiSelectClothes.UpdateView();
    }
}
