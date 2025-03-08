using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractible
{
    public InventorySlot InventorySlot;

    public InteractionType InteractionType => InteractionType.Instant;

    public bool IsCanInteract => true;

    public void Interact(Player player)
    {
        player.Inventory.AddItem(InventorySlot.Item, InventorySlot.Capacity, InventorySlot.Condition);
        Destroy(gameObject);
    }
}
