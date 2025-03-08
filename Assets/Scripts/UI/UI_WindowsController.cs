using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UI_WindowsController : MonoBehaviour
{
    [Inject] private Player _player;

    [SerializeField] private Button _btnInventory;
    [SerializeField] private Button _btnClothing;

    [SerializeField] private UI_Inventory _uiInventory;
    [SerializeField] private UI_Clothing _uiClothing;

    private GameObject _activeWindow;

    private void OnEnable()
    {
        _btnInventory.onClick.AddListener(ToggleInventory);
        _btnClothing.onClick.AddListener(ToggleClothing);

        if (_activeWindow != null)
            _activeWindow.SetActive(true);
        else
        {
            _uiInventory.gameObject.SetActive(true);
            _activeWindow = _uiInventory.gameObject;
        }
    }

    private void OnDisable()
    {
        _btnInventory.onClick.RemoveListener(ToggleInventory);
        _btnClothing.onClick.RemoveListener(ToggleClothing);

        if (_activeWindow != null)
            _activeWindow.SetActive(false);
    }

    public void ToggleInventory()
    {
        if (_activeWindow != null)
            _activeWindow.SetActive(false);

        _uiInventory.gameObject.SetActive(!_uiInventory.gameObject.activeSelf);
        _activeWindow = _uiInventory.gameObject;
    }

    public void ToggleClothing()
    {
        if (_activeWindow != null)
            _activeWindow.SetActive(false);

        _uiClothing.gameObject.SetActive(!_uiClothing.gameObject.activeSelf);
        _activeWindow = _uiClothing.gameObject;
    }

    public void SortInventoryByCategory(string strCategory)
    {
        if (Enum.TryParse(strCategory, true, out InventoryItem.ItemType category))
            _player.Inventory.Categoty = category;
        else
            _player.Inventory.Categoty = null;

        _uiInventory.ShowSlots = _player.Inventory.GetSorteredSlots();
        _uiInventory.UpdateView();
    }

    public void SortInventoryByFilter(string strFilter)
    {
        if (Enum.TryParse(strFilter, true, out Inventory.SortingFilter filter))
        {
            _player.Inventory.Filter = filter;
            _uiInventory.ShowSlots = _player.Inventory.GetSorteredSlots();
        }

        _uiInventory.UpdateView();
    }
}
