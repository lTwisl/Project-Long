using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

public class UI_WindowsController : MonoBehaviour
{
   
    [SerializeField] private Button _btnInventory;
    [SerializeField] private Button _btnClothing;

    [SerializeField] private UI_Inventory _uiInventory;
    [SerializeField] private UI_Clothing _uiClothing;

    private GameObject _activeWindow;

    private Player _player;

    [Inject]
    private void Construct(Player player)
    {
        _player = player;
    }

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

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDisable()
    {
        _btnInventory.onClick.RemoveListener(ToggleInventory);
        _btnClothing.onClick.RemoveListener(ToggleClothing);

        if (_activeWindow != null)
            _activeWindow.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
        Category? category = null;
        if (!string.IsNullOrEmpty(strCategory))
            category = Enum.Parse<Category>(strCategory, true);

        SortInventoryByCategory(category);
    }

    public void SortInventoryByFilter(string strFilter)
    {
        SortInventoryByFilter(Enum.Parse<Inventory.SortingFilter>(strFilter, true));
    }

    public void SortInventoryByCategory(Category? category)
    {
        _player.Inventory.Categoty = category;

        _uiInventory.ShowSlots = _player.Inventory.GetSorteredSlots();
        _uiInventory.UpdateView();
    }

    public void SortInventoryByFilter(Inventory.SortingFilter filter)
    {
        _player.Inventory.Filter = filter;
        _uiInventory.ShowSlots = _player.Inventory.GetSorteredSlots();

        _uiInventory.UpdateView();
    }
}
