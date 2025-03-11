using UnityEngine;

[CreateAssetMenu(fileName = "WearStrategy", menuName = "Scriptable Objects/WearStrategy")]
public class WearStrategy : UseStrategy
{
    public override void Execute(InventoryItem item, Player player)
    {
        if (item is not ClothesItem) 
            return;
    }
}
