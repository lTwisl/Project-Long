using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
[SelectionBase]
public class UI_ClothesSlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _wet;
    [SerializeField] private TMP_Text _type;

    public ClothesType ClothesType;
    public int IndexLayer;
    public InventorySlot Slot { get; private set; }

    private UI_SelectClothing _uiSelectClothes;

    private Player _player;

    [Inject]
    private void Construct(Player player)
    {
        _player = player;
    }


    private void Awake()
    {
        _type.text = ClothesType.ToString() + $"_{IndexLayer}";
    }

    public void Init(InventorySlot slot, UI_SelectClothing uiSelectClothes)
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
        if (Slot == null || Slot.Item == null)
        {
            _icon.sprite = null;
            _wet.text = "Wet";
            return;
        }

        _icon.sprite = Slot.Item.Icon;
        _wet.text = Slot.Wet.ToString("0.00");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        List<InventorySlot> slots = new();
        foreach (var invSlot in _player.Inventory.Slots)
        {
            if (invSlot.Item.Category == Category.Clothes
                && (invSlot.Item as ClothingItem).ClothingType == ClothesType)
            {
                if (_player.ClothingSystem.Contains(invSlot) && Slot != invSlot)
                    continue;

                slots.Add(invSlot);
            }
        }

        _uiSelectClothes.Slots = slots;
        _uiSelectClothes.CurrentUiClothesSlot = this;
        _uiSelectClothes.UpdateView();
    }
}
