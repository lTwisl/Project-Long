using EditorAttributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


[System.Serializable]
public class RandItem
{
    [field: SerializeField] public float Probability { get; private set; }
    [field: SerializeField] public InventoryItem Item { get; private set; }
    [field: SerializeField] public Vector2 MinMaxCapacity { get; private set; }
    [field: SerializeField] public Vector2 MinMaxCondition { get; private set; }

    public RandItem(float probability, InventoryItem item, Vector2 minMaxCapacity, Vector2 minMaxCondition)
    {
        Probability = probability;
        Item = item;
        MinMaxCapacity = minMaxCapacity;
        MinMaxCondition = minMaxCondition;
    }

    public bool Collapse(out InventorySlot slot)
    {
        if (Random.value > Probability)
        {
            slot = null;
            return false;
        }


        slot =  new InventorySlot(
           Item,
           Random.Range(MinMaxCapacity.x, MinMaxCapacity.y),
           Random.Range(MinMaxCondition.x, MinMaxCondition.y)
           );
        return true;
    }
}


public class Storage : MonoBehaviour, IInteractible, IShowable
{
    public bool ShowScriptInfo { get; set; } // Для SceneScriptsControlsWindow

    [field: SerializeField] public InteractionType InteractionType { get; protected set; }

    [SerializeField] private InitializerListSlots _initializationSlots = new();
    [SerializeField] private List<InventorySlot> _randomSlots = new();

    public Inventory Inventory { get; protected set; }

    public virtual bool IsCanInteract { get; protected set; } = true;

    [Inject] private World _world;

    private void Awake()
    {
        _initializationSlots.Items.AddRange(_randomSlots);
        Inventory = new Inventory(_world, _initializationSlots);
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

    [Space(20)]
    
    [SerializeField, PropertyDropdown] private RandomItemsPatternPreset _randomPreset;

    [Button("Collapse Rand Slots", buttonHeight: 40)]
    public void Foo()
    {
        _randomSlots.Clear();

        if (_randomPreset == null)
            return;

        if (_randomPreset.GetRandItems().Count == 0)
            return;

        int index = 0;
        while (_randomSlots.Count + _initializationSlots.Items.Count < _randomPreset.MaxCountItems)
        {
            if (index >= _randomPreset.GetRandItems().Count)
                index = 0;

            if (_randomPreset.GetRandItems()[index].Collapse(out InventorySlot slot))
                _randomSlots.Add(slot);

            index++;
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
