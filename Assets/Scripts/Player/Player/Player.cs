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
    public GameObject MainCamera { get; private set; }

    private InteractionController _interactionController;

    private void Awake()
    {
        Inventory.Init();
        ClothingSystem.Init();

        PlayerInputs = GetComponent<PlayerInputs>();
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        _interactionController = new InteractionController(this, _slider, 2f);

        //LoadData();
    }

    private void Start()
    {
        Inventory.OnItemRemoved += slot => ClothingSystem.HandleItemRemoved(slot);

        WorldTime.Instance.OnMinuteChanged += _ => Inventory.Update(1);
    }

    private void Update()
    {
        _interactionController.Update(Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _uiWindowsController.gameObject.SetActive(!_uiWindowsController.gameObject.activeSelf);
            Cursor.visible = _uiWindowsController.gameObject.activeSelf;
            Cursor.lockState = !Cursor.visible ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    private void LoadData()
    {
        string json = PlayerPrefs.GetString("PlayerInventory");
        Inventory = JsonUtility.FromJson<Inventory>(json);
    }

    private void OnDestroy()
    {
        string json = JsonUtility.ToJson(Inventory);
        PlayerPrefs.SetString("PlayerInventory", json);
    }

    [ContextMenu("DropFirstItem [Script]")]
    public void DropFirstItem()
    {
        if (Inventory.CountSlots == 0)
            return;

        InventorySlot firstSlot = Inventory.Slots.First.Value;

        if (firstSlot == null)
            return;

        if (firstSlot.Item?.ItemPrefab == null)
            return;

        WorldItem item = Instantiate(firstSlot.Item.ItemPrefab, transform.position, transform.rotation);
        item.InventorySlot = new InventorySlot(firstSlot.Item, firstSlot.Capacity, firstSlot.Condition);

        Inventory.RemoveItem(firstSlot);
    }

    [ContextMenu("EquipFirst [Script]")]
    public void EquipFirst()
    {
        ClothingSystem.TryEquip(Inventory.Slots.First.Value, 0);
    }

    [ContextMenu("InitInventory [Script]")]
    public void InitInventory()
    {
        Inventory.Init();
    }
}
