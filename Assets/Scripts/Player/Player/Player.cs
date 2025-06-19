using FirstPersonMovement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using ClothingSystems;


[SelectionBase]
public class Player : MonoBehaviour
{
    public Inventory Inventory { get; private set; }
    public ClothingSystem ClothingSystem { get; private set; }

    [Inject] private ClothingSystemConfig _clothingSystemConfig;

    [Inject] private World _world;
    [Inject] private PlayerParameters _parameters;

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
        GetComponent<PlayerParameterHandler>().Bind(Inventory, ClothingSystem, _playerMovement, _world);

        SetVisibilityUiPlayer(false);

        StartCoroutine(ClothingSystem.UpdateGroups(GameTime.DeltaTime / 60f));
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
        Inventory = new(_world);
        ClothingSystem = new(_clothingSystemConfig, _world);
    }

    private void SubscribeToEvents()
    {
        GameTime.OnTimeChanged += HandleMinuteChange;
        
        _input.OnChangedVisibilityUiPlayer += SetVisibilityUiPlayer;
        _world.OnEnterToxicityZone += HandleToxicityZoneEnter;
    }

    private void UnsubscribeFromEvents()
    {
        GameTime.OnTimeChanged -= HandleMinuteChange;

        _input.OnChangedVisibilityUiPlayer -= SetVisibilityUiPlayer;
        _world.OnEnterToxicityZone -= HandleToxicityZoneEnter;
    }

    // ========================== UI MANAGEMENT ========================== //
    public void SetVisibilityUiPlayer(bool newVisibility)
    {
        _uiWindowsController.gameObject.SetActive(newVisibility);
        Cursor.visible = _uiWindowsController.gameObject.activeSelf;
        Cursor.lockState = !Cursor.visible ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void HandleMinuteChange()
    {
        Inventory.Update(GameTime.DeltaTime / 60f);
        //ClothingSystem.UpdateGroups(GameTime.DeltaTime / 60f);
    }

    private void HandleToxicityZoneEnter(ToxicityZone zone)
    {
        if (zone.CurrentType == ToxicityZone.ZoneType.Single)
        {
            float protection = 0f;
            foreach (var item in ClothingSystem.ClothingSlotGroups)
            {
                protection += zone.Toxicity * item.TotalToxicityProtection;
            }

            _parameters.Toxicity.Current += zone.Toxicity - protection;
        }
    }

    public void DropItem(InventorySlot slot)
    {
        if (!Inventory.Contains(slot)) return;

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
            ClothingSystem.TryUnequip(slot);

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
}