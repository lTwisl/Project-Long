using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UI_Inventory : MonoBehaviour
{
    [Inject] private Player _player;

    [SerializeField] private UI_Slot _uiSlotPrefab;
    [SerializeField] private GridLayoutGroup _uiGrid;

    private List<UI_Slot> _uiSlots = new List<UI_Slot>();

    [SerializeField] private UI_ItemDetails _uiDetails;
    [SerializeField] private Button _btnUse;
    [SerializeField] private Button _btnDrop;

    public List<IReadOnlyInventorySlot> ShowSlots { get; set; }

    private UI_Slot _selectesSlot;

    private void OnEnable()
    {
        UpdateView();

        _btnDrop.onClick.AddListener(() =>
        {
            _player.DropItem(_selectesSlot.Slot as InventorySlot);
            UpdateView();
        });

        _btnUse.onClick.AddListener(() =>
        {
            _player.UseItem(_selectesSlot.Slot as InventorySlot); 
            UpdateView();
        });
    }

    private void CreateUiSlot(IReadOnlyInventorySlot slot)
    {
        var uiSlot = Instantiate(_uiSlotPrefab, _uiGrid.transform);
        uiSlot.Init(slot);
        _uiSlots.Add(uiSlot);

        uiSlot.OnClick += slot =>
        {
            _selectesSlot = slot;
            _btnUse.gameObject.SetActive(slot.Slot.Item.UseType != MethodOfUse.None);
            _uiDetails.UpdateView(_selectesSlot.Slot.Item);
        };
    }

    public void UpdateView()
    {
        ShowSlots = _player.Inventory.GetSorteredSlots();

        // Удалить лишние слоты интерфейса.
        if (_uiSlots.Count > ShowSlots.Count)
        {
            for (int j = ShowSlots.Count; j < _uiSlots.Count; ++j)
            {
                Destroy(_uiSlots[j].gameObject);
            }
            _uiSlots.RemoveRange(ShowSlots.Count, _uiSlots.Count - ShowSlots.Count);
        }

        int i = 0;
        foreach (var slot in ShowSlots)
        {
            if (i < _uiSlots.Count)
            {
                _uiSlots[i].Init(slot);
            }
            else
            {
                CreateUiSlot(slot);
            }

            ++i;
        }
    }
}
