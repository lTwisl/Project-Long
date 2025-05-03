using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractible
{
    public InventorySlot InventorySlot;

    public virtual InteractionType InteractionType => InteractionType.Click;

    public virtual bool IsCanInteract => true;

    [HideInInspector] public bool IsDirtItem = false;

    private void OnEnable()
    {
        if (IsDirtItem)
            GameTime.OnMinuteChanged += UpdateConditionItem;
    }

    private void OnDisable()
    {
        if (IsDirtItem)
            GameTime.OnMinuteChanged -= UpdateConditionItem;
    }

    public virtual void Interact(Player player)
    {
        player.Inventory.AddItem(InventorySlot.Item, InventorySlot.Capacity, InventorySlot.Condition);
        Destroy(gameObject);
    }

    private void UpdateConditionItem()
    {
        if (IsDirtItem == false)
            return;

        if (InventorySlot?.Item == null)
            return;

        if (InventorySlot.Item.DegradeType == DegradationType.Rate)
            InventorySlot.Condition -= InventorySlot.Item.DegradationValue * 3;
    }

    public static WorldItem PlacementInWorld(InventorySlot slot, Vector3 position, Quaternion rotation)
    {
        WorldItem worldItem = Instantiate(slot.Item.ItemPrefab, position, rotation);
        worldItem.InventorySlot = new(slot.Item, slot.Capacity, slot.Condition);
        worldItem.IsDirtItem = true;

        GameTime.OnMinuteChanged += worldItem.UpdateConditionItem;

        return worldItem;
    }
}
