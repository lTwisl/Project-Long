using UnityEngine;

[CreateAssetMenu(fileName = "HeatingItem", menuName = "Items/Heating")]
public class HeatingItem : InventoryItem
{
    public enum HeatingType
    {
        Fuel = 0,
        Igniter = 1,
        Spiral = 2,
        Antifreeze = 3,
    }

    [field: Header("Additional Properties")]
    [field: SerializeField] public HeatingType TypeHeating { get; private set; }
    [field: SerializeField] public float Value {  get; private set; }
    [field: SerializeField, Range(0, 100)] public float ChanceHeating { get; private set; }

    private void OnEnable()
    {
        Category = Category.Heating;
    }

    public override string ToString()
    {
         return base.ToString() + $"Type: {TypeHeating} | Value: {Value}\n" +
            $"ChanceHeating: {ChanceHeating}";
    }
}






