using UnityEngine;

[CreateAssetMenu(fileName = "MaterialItem", menuName = "Items/Materials")]
public class MaterialItem : InventoryItem
{
    private void OnEnable()
    {
        Category = Category.Materials;
        Actions = ActionType.Deconstruct;
    }
}
