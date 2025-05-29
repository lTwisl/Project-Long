using UnityEngine;

[CreateAssetMenu(fileName = "EquipHandStrategy", menuName = "Use Item Strategy/EquipHandStrategy")]
public class EquipHandStrategy : UseStrategy
{
    /*public override void Execute(InventoryItem item)
    {
        if (item is not ToolItem tool)
            return;

        Debug.Log($"Equip item (ToolItem) {item.Name} in hand");
    }*/
}
