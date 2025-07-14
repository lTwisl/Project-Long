using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Slot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _capacity;
    [SerializeField] private TMP_Text _condition;
    [SerializeField] private TMP_Text _weight;

    public event Action<UI_Slot> OnClick;

    public InventorySlot Slot { get; private set; }

    public void SetItem(InventorySlot slot)
    {
        Slot = slot;
    }

    public void OnEnable()
    {
        if (Slot == null)
            return;

        UpdateView();

        Slot.OnConditionChanged += _ => UpdateView();
        Slot.OnCapacityChanged += _ => UpdateView();
    }

    private void OnDisable()
    {
        Slot.OnConditionChanged -= _ => UpdateView();
        Slot.OnCapacityChanged -= _ => UpdateView();
    }

    public void UpdateView()
    {
        Debug.Log($"{Time.frameCount}: UpdateView");

        _icon.sprite = Slot.Item.Icon;

        if (Slot.Item.UnitMeasurement == UnitsMeasurement.None)
            _capacity.text = Slot.Capacity.ToString("0.##");
        else
            _capacity.text = Slot.Capacity.ToString("0.##") + $" {Slot.Item.UnitMeasurement}";

        _condition.text = Math.Ceiling(Slot.Condition * 100).ToString() + " %";
        _weight.text = (Slot.Capacity * Slot.GetWeight()).ToString("0.##") + $" Í„";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(Slot.Item.ToString());
        OnClick?.Invoke(this);
    }
}
