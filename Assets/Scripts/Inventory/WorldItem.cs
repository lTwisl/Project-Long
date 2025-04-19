using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractible
{
    public InventorySlot InventorySlot;

    public virtual InteractionType InteractionType => InteractionType.Click;

    public virtual bool IsCanInteract => true;

    public virtual void Interact(Player player)
    {
        player.Inventory.AddItem(InventorySlot.Item, InventorySlot.Capacity, InventorySlot.Condition);
        Destroy(gameObject);
    }
}
