using UnityEngine;

[CreateAssetMenu(fileName = "ToolItem", menuName = "Items/Tools")]
public class ToolItem : InventoryItem
{
    private void OnEnable()
    {
        Category = Category.Tools;
        Actions = ActionType.Repair;

        IsStackable = false;
        MeasuredAsInteger = true;
        MaxCapacity = 1;
        DegradeType = DegradationType.Used;
    }
}
