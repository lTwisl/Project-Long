using EditorAttributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Storage : MonoBehaviour, IInteractible, IShowable
{
    [field: SerializeField] public Inventory Inventory { get; protected set; }

    [field: SerializeField] public InteractionType InteractionType { get; protected set; }

    private string _storageId;
    public virtual bool IsCanInteract { get; protected set; } = true;
    public bool ShowScriptInfo { get; set; }

    private void Awake()
    {
        _storageId = gameObject.name + transform.position.ToString();
        Inventory.Init();
        //LoadData();
    }

    private void LoadData()
    {
        string json = PlayerPrefs.GetString(_storageId);
        Inventory = JsonUtility.FromJson<Inventory>(json);
    }

    private void OnDestroy()
    {
        /*string json = JsonUtility.ToJson(Inventory);
        PlayerPrefs.SetString(_storageId, json);*/
    }

    public virtual void Interact(Player player)
    {
        IsCanInteract = false;

        foreach (var slot in Inventory.Slots)
        {
            player.Inventory.AddItem(slot.Item, slot.Capacity, slot.Condition);
        }

        Inventory.Clear();
    }

#if UNITY_EDITOR
    [Space(10)]
    [SerializeField] private List<InventoryItem> _initItems;
    [Button("Добавить предметы в инвентарь", buttonHeight: 40)]
    public void ItemsToInventory()
    {
        Undo.RecordObject(this, "Undo Add Inventory Items");
        EditorUtility.SetDirty(this);
        foreach (var item in _initItems)
        {
            if (item == null)
                continue;

            Inventory.InitSlots.Add(new InventorySlot(item, 1, 100));
        }
        //_initItems.Clear();
    }

    private void OnDrawGizmos()
    {
        if (!ShowScriptInfo) return;

        // 1. Текстовая информация
        GUIStyle textStyle = new GUIStyle
        {
            normal = { textColor = new Color(1f, 0.5f, 0f, 1f) },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 18,
            richText = true
        };
        Handles.Label(transform.position + Vector3.up * 1f, $"<b>🗃\n{name}</b>\n", textStyle);
    }

#endif
}
