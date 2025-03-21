using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[SelectionBase]
public class Player : MonoBehaviour
{
    [field: SerializeField] public Inventory Inventory { get; private set; }
    [field: SerializeField] public ClothingSystem ClothingSystem { get; private set; }

    [Header("UI")]
    [SerializeField] private Slider _slider;
    [SerializeField] private UI_WindowsController _uiWindowsController;

    [Inject] private PlayerParameters _playerParameters;

    public PlayerInputs PlayerInputs { get; private set; }
    public Camera MainCamera { get; private set; }

    

    

    private void Awake()
    {
        Inventory.Init();
        ClothingSystem.Init();

        PlayerInputs = GetComponent<PlayerInputs>();
        MainCamera = Camera.main;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }



    private void OnEnable()
    {
        PlayerInputs.OnChangeVisibilityUiPlayer += SetVisibilityUiPlayer;

        ClothingSystem.OnEquip += UpdateClothesOffsetMax;
        ClothingSystem.OnUnequip += UpdateClothesOffsetMax;

        WorldTime.Instance.OnMinuteChanged += InventoryUpdateMinuteChanged;
        WorldTime.Instance.OnMinuteChanged += ClothingSystemUpdateMinuteChanged;

        Inventory.OnItemRemoved += ClothingSystem.HandleItemRemoved;
    }

    private void OnDisable()
    {
        PlayerInputs.OnChangeVisibilityUiPlayer -= SetVisibilityUiPlayer;

        ClothingSystem.OnEquip -= UpdateClothesOffsetMax;
        ClothingSystem.OnUnequip -= UpdateClothesOffsetMax;

        WorldTime.Instance.OnMinuteChanged -= InventoryUpdateMinuteChanged;
        WorldTime.Instance.OnMinuteChanged -= ClothingSystemUpdateMinuteChanged;

        Inventory.OnItemRemoved -= ClothingSystem.HandleItemRemoved;
    }

    private void InventoryUpdateMinuteChanged(TimeSpan _) => Inventory.Update(1);
    private void ClothingSystemUpdateMinuteChanged(TimeSpan _) => ClothingSystem.Update(1);

    private void UpdateClothesOffsetMax(InventorySlot _) => _playerParameters.Stamina.OffsetMax = ClothingSystem.TotalOffsetStamina;

    public void SetVisibilityUiPlayer(bool newVisibility)
    {
        _uiWindowsController.gameObject.SetActive(newVisibility);
        Cursor.visible = _uiWindowsController.gameObject.activeSelf;
        Cursor.lockState = !Cursor.visible ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void LoadData()
    {
        string json = PlayerPrefs.GetString("PlayerInventory");
        Inventory = JsonUtility.FromJson<Inventory>(json);
    }

    private void OnDestroy()
    {
        /*string json = JsonUtility.ToJson(Inventory);
        PlayerPrefs.SetString("PlayerInventory", json);*/
    }

    public void DropItem(InventorySlot slot)
    {
        if (Inventory.Slots.Count == 0)
            return;

        if (!Inventory.Slots.Contains(slot))
            return;

        if (slot.Item.ItemPrefab != null)
        {
            WorldItem item = Instantiate(slot.Item.ItemPrefab, transform.position, transform.rotation);
            item.InventorySlot = new InventorySlot(slot.Item, slot.Capacity, slot.Condition);
        }

        if (slot.Item is ClothingItem clothes && slot.IsWearing)
            ClothingSystem.Unequip(slot);
        
        Inventory.RemoveItem(slot);
    }

    public void UseItem(InventorySlot slot)
    {
        slot.UseItem();
    }
}
