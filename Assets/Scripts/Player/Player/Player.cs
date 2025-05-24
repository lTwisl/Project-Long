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
        HandleMovementByCapacityStats();

        SetVisibilityUiPlayer(false);
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
        _input.OnChangeVisibilityUiPlayer += SetVisibilityUiPlayer;
        ClothingSystem.OnEquip += UpdateClothesStaminaOffsetMax;
        ClothingSystem.OnUnequip += UpdateClothesStaminaOffsetMax;
        GameTime.OnMinuteChanged += HandleMinuteChange;
        Inventory.OnItemRemoved += ClothingSystem.HandleItemRemoved;
        Inventory.OnItemAdded += UpdateCurrentCapacity;
        Inventory.OnItemRemoved += UpdateCurrentCapacity;
        _world.OnEnterToxicityZone += HandleToxicityZoneEnter;
        _world.OnEnterShelter += HandleEnterShelter;
        _world.OnExitShelter += HandleExitShelter;
    }

    private void UnsubscribeFromEvents()
    {
        _input.OnChangeVisibilityUiPlayer -= SetVisibilityUiPlayer;
        ClothingSystem.OnEquip -= UpdateClothesStaminaOffsetMax;
        ClothingSystem.OnUnequip -= UpdateClothesStaminaOffsetMax;
        GameTime.OnMinuteChanged -= HandleMinuteChange;
        Inventory.OnItemRemoved -= ClothingSystem.HandleItemRemoved;
        Inventory.OnItemAdded -= UpdateCurrentCapacity;
        Inventory.OnItemRemoved -= UpdateCurrentCapacity;
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

    private void UpdateClothesStaminaOffsetMax(InventorySlot _)
    {
        _parameters.Stamina.OffsetMax = ClothingSystem.TotalOffsetStamina;
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

    private void HandleMovementByCapacityStats()
    {
        _playerMovement.SpeedMediator.AddModifier(new(0, new MaxSpeedCondition(MoveMode.Walk), (float speed) =>
        {
            if (speed == 0 || _parameters.Capacity.GetCurrentWeightRange() < WeightRange.Ultimate)
                return speed;

            float scale = Utility.MapRange(_parameters.Capacity.Current,
                _parameters.Capacity.GetRangeLoadCapacity(WeightRange.Ultimate),
                _parameters.Capacity.GetRangeLoadCapacity(WeightRange.UltimateImmovable),
            1, 0, true);

            return speed * scale;
        }));

        _playerMovement.SpeedMediator.AddModifier(new(0, new MaxSpeedCondition(MoveMode.Run), (float speed) =>
        {
            if (speed == 0 || _parameters.Capacity.GetCurrentWeightRange() < WeightRange.Critical)
                return speed;

            float scale = Utility.MapRange(_parameters.Capacity.Current,
                _parameters.Capacity.GetRangeLoadCapacity(WeightRange.Critical),
                _parameters.Capacity.GetRangeLoadCapacity(WeightRange.Ultimate),
                1, _playerMovement.WalkSpeed / speed, true);

            return speed * scale;
        }));

        _parameters.Capacity.OnValueChanged += _ =>
        {
            _playerMovement.CanRun = _parameters.Capacity.IsCanSprint();
            _playerMovement.CanWalk = _parameters.Capacity.IsCanWalk();
        };
    }

}