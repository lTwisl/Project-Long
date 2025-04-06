using EditorAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Storage : MonoBehaviour, IInteractible
{
    [field: SerializeField] public Inventory Inventory { get; protected set; }

    [field: SerializeField] public InteractionType InteractionType { get; protected set; }

    private string _storageId;
    public virtual bool IsCanInteract { get; protected set; } = true;

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
    private void ItemsToInventory()
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
#endif
}
