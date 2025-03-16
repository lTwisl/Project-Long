using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class Player : MonoBehaviour
{
    [field: SerializeField] public Inventory Inventory { get; private set; }
    [field: SerializeField] public ClothingSystem ClothingSystem { get; private set; }

    [Header("UI")]
    [SerializeField] private Slider _slider;
    [SerializeField] private UI_WindowsController _uiWindowsController;

    public PlayerInputs PlayerInputs { get; private set; }
    public Camera MainCamera { get; private set; }

    private InteractionController _interactionController;

    private void Awake()
    {
        Inventory.Init();
        ClothingSystem.Init();

        PlayerInputs = GetComponent<PlayerInputs>();
        MainCamera = Camera.main;

        _interactionController = new InteractionController(this, _slider, 2f);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void OnEnable()
    {
        PlayerInputs.OnChangeVisibilityUiPlayer += SetVisibilityUiPlayer;
    }

    private void OnDisable()
    {
        PlayerInputs.OnChangeVisibilityUiPlayer -= SetVisibilityUiPlayer;
    }

    private void Start()
    {
        Inventory.OnItemRemoved += slot => ClothingSystem.HandleItemRemoved(slot as InventorySlot);

        WorldTime.Instance.OnMinuteChanged += _ => Inventory.Update(1);
        WorldTime.Instance.OnMinuteChanged += _ => ClothingSystem.Update(1);
    }

    private void Update()
    {
        _interactionController.Update(Time.deltaTime);
    }

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
        if (Inventory.CountSlots == 0)
            return;

        if (!Inventory.Slots.Contains(slot))
            return;

        if (slot.Item.ItemPrefab != null)
        {
            WorldItem item = Instantiate(slot.Item.ItemPrefab, transform.position, transform.rotation);
            item.InventorySlot = new InventorySlot(slot.Item, slot.Capacity, slot.Condition);
        }

        Inventory.RemoveItem(slot);
    }

    public void UseItem(InventorySlot slot)
    {
        slot.UseItem(this);
    }
}
