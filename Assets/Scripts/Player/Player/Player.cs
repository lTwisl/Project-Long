using FirstPersonMovement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[SelectionBase]
public class Player : MonoBehaviour
{
    [field: Inject] public Inventory Inventory { get; private set; }
    [field: Inject] public ClothingSystem ClothingSystem { get; private set; }

    [Inject] private World _world;
    [Inject] private PlayerParameters _parameters;

    [SerializeField] private float _degradationScaleOutside = 2;

    // UI References
    [SerializeField] private Slider _slider;
    [SerializeField] private UI_WindowsController _uiWindowsController;

    private PlayerMovement _playerMovement;
    private InputReader _input;

    private void Awake()
    {
        InitializeComponents();
        InitializeSystems();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void Start()
    {
        GetComponent<PlayerParameterHandler>().Bind(Inventory, _playerMovement, _world);

        HandleMovementByCapacityStats();

        SetVisibilityUiPlayer(false);

        UpdateClothesStaminaOffsetMax();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }


    private void InitializeComponents()
    {
        _input = GetComponent<InputReader>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void InitializeSystems()
    {
        Inventory.Init();
        ClothingSystem.Init();
    }

    private void SubscribeToEvents()
    {
        _input.OnChangedVisibilityUiPlayer += SetVisibilityUiPlayer;
        GameTime.OnMinuteChanged += HandleMinuteChange;
        Inventory.OnItemRemoved += ClothingSystem.HandleItemRemoved;
        _world.OnEnterToxicityZone += HandleToxicityZoneEnter;
        _world.OnEnterShelter += HandleEnterShelter;
        _world.OnExitShelter += HandleExitShelter;
    }

    private void UnsubscribeFromEvents()
    {
        _input.OnChangedVisibilityUiPlayer -= SetVisibilityUiPlayer;
        GameTime.OnMinuteChanged -= HandleMinuteChange;
        Inventory.OnItemRemoved -= ClothingSystem.HandleItemRemoved;
        _world.OnEnterToxicityZone -= HandleToxicityZoneEnter;
        _world.OnEnterShelter -= HandleEnterShelter;
        _world.OnExitShelter -= HandleExitShelter;
    }

    // ========================== UI MANAGEMENT ========================== //
    public void SetVisibilityUiPlayer(bool newVisibility)
    {
        _uiWindowsController.gameObject.SetActive(newVisibility);
        Cursor.visible = _uiWindowsController.gameObject.activeSelf;
        Cursor.lockState = !Cursor.visible ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void UpdateClothesStaminaOffsetMax()
    {
        _parameters.Stamina.Mediator.AddModifier(new(0, ValueType.Max, value =>
        {
            value += ClothingSystem.TotalOffsetStamina;
            return value;
        }));
    }

    private void HandleMinuteChange()
    {
        Inventory.Update(1);
        ClothingSystem.Update(1);
    }

    private void HandleEnterShelter(Shelter shelter)
    {
        Inventory.DegradationScale = 1;
        ClothingSystem.DegradationScale = 1;
    }

    private void HandleExitShelter(Shelter shelter)
    {
        Inventory.DegradationScale = _degradationScaleOutside;
        ClothingSystem.DegradationScale = _degradationScaleOutside;
    }

    private void HandleToxicityZoneEnter(ToxicityZone zone)
    {
        if (zone.CurrentType == ToxicityZone.ZoneType.Single)
            _parameters.Toxicity.Current += zone.Toxicity * (1 - ClothingSystem.TotalToxicityProtection / 100);
    }

    public void UpdateCurrentCapacity(InventorySlot _) => _parameters.Capacity.Current = Inventory.Weight;

    public void DropItem(InventorySlot slot)
    {
        if (!Inventory.Slots.Contains(slot)) return;

        Vector3 origin = transform.position + Vector3.up * 1.8f;
        for (float angle = 0f; angle < 360f; angle += 5f)
        {
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * transform.forward;
            Ray ray = new(origin, direction);
            if (Physics.SphereCast(ray, 0.2f, 1.5f)) continue;

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

    public void CancelUseItem(InventorySlot slot)
    {
        slot.Item.UseStrategy.Cancel();
    }

    private void HandleMovementByCapacityStats()
    {
        
    }
}