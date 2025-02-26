using UnityEngine;

public class Player : MonoBehaviour
{
    public Inventory Inventory;
    public InventorySlot Slot;

    private void Start()
    {
        
    }

    [ContextMenu("Add Item")]
    public void AddItem()
    {
        Inventory.AddItem(Slot.Item, Slot.Capacity, Slot.Condition);
    }

    private void Update()
    {
        Inventory.Update(Time.deltaTime);
    }

    [ContextMenu("Sort")]
    public void SortInventory()
    {
        Inventory.Sort();
    }

    [ContextMenu("PrintWithSelectedCategory")]
    public void PrintWithSelectedCategory()
    {
        string s = "";
        foreach (var slot in Inventory.GetSlotsWithCategoty(Inventory.Categoty))
        {
            s += slot.Item.Name + "\n";
        }
        Debug.Log(s);
    }
}
