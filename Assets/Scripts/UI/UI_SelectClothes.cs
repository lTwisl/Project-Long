using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UI_SelectClothes : MonoBehaviour
{
    [Inject] private Player _player;

    [SerializeField] private Image _image;
    [SerializeField] private Button _btnPutOn;
    [SerializeField] private Button _btnPutOff;

    [SerializeField] private Button _btnPrev;
    [SerializeField] private Button _btnNext;

    [SerializeField] private TMP_Text _text;

    public List<IReadOnlyInventorySlot> Slots = new List<IReadOnlyInventorySlot>();
    public UI_ClothesSlot CurrentUiClothesSlot;

    private int _currentIndex;

    private void Awake()
    {
        _btnPutOn.onClick.AddListener(() =>
        {
            if (CurrentUiClothesSlot == null || Slots.Count == 0)
                return;

            _player.ClothingSystem.Unequip(CurrentUiClothesSlot.Slot);
            _player.ClothingSystem.TryEquip(Slots[_currentIndex] as InventorySlot, CurrentUiClothesSlot.IndexLayer);
            CurrentUiClothesSlot.Set(Slots[_currentIndex] as InventorySlot);
        });

        _btnPutOff.onClick.AddListener(() =>
        {

            if (!_player.ClothingSystem.SlotCache.TryGetValue(CurrentUiClothesSlot.ClothesType, out ClothingSlot clothingSlot))
                return;

            _player.ClothingSystem.Unequip(CurrentUiClothesSlot.Slot);
            CurrentUiClothesSlot.Clear();
        });

        _btnPrev.onClick.AddListener(() =>
        {
            _currentIndex = Mathf.Clamp(_currentIndex - 1, 0, Slots.Count - 1);
            UpdateView();
        });

        _btnNext.onClick.AddListener(() =>
        {
            _currentIndex = Mathf.Clamp(_currentIndex + 1, 0, Slots.Count - 1);
            UpdateView();
        });
    }

    public void UpdateView()
    {
        if (Slots.Count == 0)
        {
            _image.sprite = null;
            return;
        }

        _currentIndex = Mathf.Clamp(_currentIndex, 0, Slots.Count - 1);
        _image.sprite = Slots[_currentIndex].Item.Icon;

        var clothes = Slots[_currentIndex].Item as ClothesItem;

        _text.text = $"{clothes.Name}: " +
            $"Temp = {clothes.TemperatureBonus}" +
            $" | Water = {clothes.WaterProtection}" +
            $" | Wind = {clothes.WindProtection}" +
            $" | Friction = {clothes.FrictionBonus}" +
            $" | Toxisity = {clothes.ToxisityProtection}";
    }
}
