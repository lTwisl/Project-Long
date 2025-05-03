using UnityEngine;
using UnityEngine.UI;
using Zenject;


[SelectionBase]
public class Player : MonoBehaviour
{
    [Inject] private World _world;
    [field: Inject] public Inventory Inventory { get; private set; }
    [field: Inject] public ClothingSystem ClothingSystem { get; private set; }

    [SerializeField] private float _degradationScaleOutside = 2;

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

    private void Start()
    {
        HandlerExitShelter(null);
        _playerParameters.Capacity.Current = Inventory.Weight;
    }

    private void OnEnable()
    {

        PlayerInputs.OnChangeVisibilityUiPlayer += SetVisibilityUiPlayer;

        ClothingSystem.OnEquip += UpdateClothesStaminaOffsetMax;
        ClothingSystem.OnUnequip += UpdateClothesStaminaOffsetMax;

        GameTime.OnMinuteChanged += HandleChangedMinute;

        Inventory.OnItemRemoved += ClothingSystem.HandleItemRemoved;

        Inventory.OnItemAdded += UpdateCurrentCapacity;
        Inventory.OnItemRemoved += UpdateCurrentCapacity;

        _world.OnEnterToxicityZone += zone =>
        {
            if (zone.CurrentType == ToxicityZone.ZoneType.Single)
                _playerParameters.Toxicity.Current += zone.Toxicity * (1 - ClothingSystem.TotalToxicityProtection / 100);
        };

        _world.OnEnterShelter += HandlerEnterShelter;
        _world.OnExitShelter += HandlerExitShelter;


    }

    private void OnDisable()
    {
        PlayerInputs.OnChangeVisibilityUiPlayer -= SetVisibilityUiPlayer;

        ClothingSystem.OnEquip -= UpdateClothesStaminaOffsetMax;
        ClothingSystem.OnUnequip -= UpdateClothesStaminaOffsetMax;

        GameTime.OnMinuteChanged -= HandleChangedMinute;

        Inventory.OnItemRemoved -= ClothingSystem.HandleItemRemoved;

        Inventory.OnItemAdded -= UpdateCurrentCapacity;
        Inventory.OnItemRemoved -= UpdateCurrentCapacity;

        _world.OnEnterShelter -= HandlerEnterShelter;
        _world.OnExitShelter -= HandlerExitShelter;
    }

    private void HandleChangedMinute()
    {
        Inventory.Update(1);
        ClothingSystem.Update(1);
    }

    private void HandlerEnterShelter(Shelter _)
    {
        Inventory.DegradationScale = 1;
        ClothingSystem.DegradationScale = 1;
    }

    private void HandlerExitShelter(Shelter _)
    {
        Inventory.DegradationScale = _degradationScaleOutside;
        ClothingSystem.DegradationScale = _degradationScaleOutside;
    }

    private void UpdateClothesStaminaOffsetMax(InventorySlot _) => _playerParameters.Stamina.OffsetMax = ClothingSystem.TotalOffsetStamina;

    public void UpdateCurrentCapacity(InventorySlot _) => _playerParameters.Capacity.Current = Inventory.Weight;

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


        Vector3 origin = transform.position + Vector3.up * 1.8f;
        for (float angle = 0f; angle < 360f; angle += 5f)
        {
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * transform.forward;
            Ray ray = new(origin, direction);
            if (Physics.SphereCast(ray, 0.2f, 1.5f))
                continue;

            if (slot.Item.ItemPrefab != null)
            {
                Vector3 pos = origin + direction * 1.5f;
                WorldItem.PlacementInWorld(slot, pos, transform.rotation);
            }
            break;
        }

        if (slot.Item is ClothingItem clothes && slot.IsWearing)
            ClothingSystem.Unequip(slot);

        Inventory.RemoveItem(slot);
    }

    public void UseItem(InventorySlot slot)
    {
        slot.UseItem();

        if (slot.IsEmpty)
            Inventory.RemoveItem(slot);
    }
}
