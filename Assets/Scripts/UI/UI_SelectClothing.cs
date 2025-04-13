using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UI_SelectClothing : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Button _btnPutOn;
    [SerializeField] private Button _btnPutOff;

    [SerializeField] private Button _btnPrev;
    [SerializeField] private Button _btnNext;

    [SerializeField] private TMP_Text _text;

    public List<InventorySlot> Slots = new List<InventorySlot>();
    public UI_ClothesSlot CurrentUiClothesSlot;

    private int _currentIndex;


    private ClothingSystem _clothesSystem;

    [Inject]
    private void Construct(Player player)
    {
        _clothesSystem = player.ClothingSystem;
    }


    private void Awake()
    {
        _btnPutOn.onClick.AddListener(() =>
        {
            if (CurrentUiClothesSlot == null || Slots.Count == 0)
                return;

            _clothesSystem.Unequip(CurrentUiClothesSlot.Slot);
            _clothesSystem.TryEquip(Slots[_currentIndex], CurrentUiClothesSlot.IndexLayer);
            CurrentUiClothesSlot.Set(Slots[_currentIndex]);
        });

        _btnPutOff.onClick.AddListener(() =>
        {

            if (!_clothesSystem.TryGetClothesSlot(CurrentUiClothesSlot.ClothesType, out ClothingSlot clothingSlot))
                return;

            _clothesSystem.Unequip(CurrentUiClothesSlot.Slot);
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

        var clothes = Slots[_currentIndex].Item as ClothingItem;

        _text.text = $"{clothes.Name}: " +
            $"Temperature = {clothes.TemperatureBonus}" +
            $" | Water = {clothes.WaterProtection}" +
            $" | Wind = {clothes.WindProtection}" +
            $" | Friction = {clothes.FrictionBonus}" +
            $" | Toxisity = {clothes.ToxicityProtection}";
    }
}
