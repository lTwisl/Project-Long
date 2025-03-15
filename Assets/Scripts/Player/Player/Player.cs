using System;
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

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        

        //m.Pause();


        //LoadData();
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
        Inventory.OnItemRemoved += slot => ClothingSystem.HandleItemRemoved(slot);

        WorldTime.Instance.OnMinuteChanged += _ => Inventory.Update(1);

        


        var m2 = new Process(

            TimeSpan.FromMinutes(1),
            () =>
            {
                Debug.Log("Invoke 2");
            },
            timeTerm => Debug.Log($"Teminate 2: {timeTerm}")
        );

        //var m3 = new Process(

        //    TimeSpan.FromMinutes(2),
        //    () =>
        //    {
        //        Debug.Log("Invoke 3");
        //    },
        //    timeTerm => Debug.Log($"Teminate 3: {timeTerm}")
        //);

        var m = new Process(
        
            TimeSpan.FromMinutes(1),
            () =>
            {
                Debug.Log("Invoke 1");
                m2.Play();
            },
            timeTerm => Debug.Log($"Teminate 1: {timeTerm}")
        );

        var m4 = new Process(

            TimeSpan.FromSeconds(20),
            () =>
            {
                Debug.Log("Invoke 4");
                m2.Kill();
            },
            timeTerm => Debug.Log($"Teminate 4: {timeTerm}")
        );

        m.Play();
        m4.Play();
        
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

    [ContextMenu("InitInventory [Script]")]
    public void InitInventory()
    {
        Inventory.Init();
    }

    public void UseItem(InventorySlot slot)
    {
        slot.UseItem(this);
    }
}
