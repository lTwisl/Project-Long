using UnityEngine;

[CreateAssetMenu(fileName = "EquipHandStrategy", menuName = "Scriptable Objects/EquipHandStrategy")]
public class EquipHandStrategy : UseStrategy
{
    public override void Execute(InventoryItem item, Player player)
    {
        if (item is not ToolItem tool)
            return;

        Debug.Log($"Equip item (ToolItem) {item.Name} in hand");
    }
}
