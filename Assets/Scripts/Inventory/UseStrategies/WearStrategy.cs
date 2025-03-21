using UnityEngine;

[CreateAssetMenu(fileName = "WearStrategy", menuName = "Scriptable Objects/WearStrategy")]
public class WearStrategy : UseStrategy
{
    public override void Execute(InventoryItem item)
    {
        if (item is not ClothingItem) 
            return;
    }
}
