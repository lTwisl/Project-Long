using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ClothesSlot : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _wet;

    public ClothesItem.ClothesType Region;
    public int IndexLayer;
    public InventorySlot Slot { get; private set; }

    public void Init(InventorySlot slot)
    {
        Slot = slot;

        UpdateView();
    }

    private void OnEnable()
    {
        if (Slot == null)
            return;

        UpdateView();
    }

    public void UpdateView()
    {
        _icon.sprite = Slot.Item.Icon;
        _wet.text = Slot.Wet.ToString();
    }
}
