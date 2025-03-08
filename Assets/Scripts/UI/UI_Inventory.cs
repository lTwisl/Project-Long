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

    public List<IReadOnlyInventorySlot> ShowSlots { get; set; }

    private void Start()
    {
        ShowSlots = new(_player.Inventory.Slots);

        UpdateView();
    }

    private void OnEnable()
    {
        UpdateView();
    }

    private void CreateUiSlot(IReadOnlyInventorySlot slot)
    {
        var uiSlot = Instantiate(_uiSlotPrefab, _uiGrid.transform);
        uiSlot.Init(slot);
        _uiSlots.Add(uiSlot);
    }

    public void UpdateView()
    {
        if (ShowSlots == null)
            return;

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
