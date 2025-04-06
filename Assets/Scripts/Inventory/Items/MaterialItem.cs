using UnityEngine;

[CreateAssetMenu(fileName = "MaterialItem", menuName = "Items/Items")]
public class MaterialItem : InventoryItem
{
    private void OnEnable()
    {
        Category = Category.Materials;
        Actions = ActionType.Deconstruct;
    }
}
