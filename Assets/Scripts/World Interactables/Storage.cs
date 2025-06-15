using EditorAttributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Zenject;

public class Storage : MonoBehaviour, IInteractible, IShowable
{
    public bool ShowScriptInfo { get; set; } // Для SceneScriptsControlsWindow


    [field: SerializeField] public InteractionType InteractionType { get; protected set; }
    [SerializeField] private List<InventorySlot> _initSlots = new();

    public Inventory Inventory { get; protected set; }

    public virtual bool IsCanInteract { get; protected set; } = true;

    [Inject] private World _world;


    private void Awake()
    {
        Inventory = new Inventory(_world, _initSlots);
    }

    public virtual void Interact(Player player)
    {
        IsCanInteract = false;

        foreach (var slot in Inventory.Slots)
        {
            player.Inventory.AddItem(slot.Item, slot.Capacity, slot.Condition);
        }

        AfterInteract();
    }

    public virtual void AfterInteract()
    {
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
        _initSlots.Clear();
        foreach (var item in _initItems)
        {
            if (item == null)
                continue;
            _initSlots.Add(new(item, item.MaxCapacity, 1));
        }
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
