using UnityEngine;

[CreateAssetMenu(fileName = "MaterialItem", menuName = "Items/Materials")]
public class MaterialItem : InventoryItem
{
    private void OnEnable()
    {
        Category = ItemType.Materials;
        Actions = ActionType.Deconstruct;
    }
}
