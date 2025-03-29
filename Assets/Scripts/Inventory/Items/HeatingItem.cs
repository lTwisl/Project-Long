using UnityEngine;

[CreateAssetMenu(fileName = "HeatingItem", menuName = "Items/Heating")]
public class HeatingItem : InventoryItem
{
    public enum HeatingType
    {
        AlcoholFuel = 0,
        HeatingSpiral = 1,
        ThermalIsolPaste = 2,
    }

    [field: Header("Additional Properties")]
    [field: SerializeField] public HeatingType TypeHeating { get; private set; }
    [field: SerializeField] public float Value {  get; private set; }
    [field: SerializeField, Range(0, 100)] public float ChanceHeating { get; private set; }

    private void OnEnable()
    {
        Category = Category.Heating;
        DegradeType = DegradationType.Used;
    }

    public override string ToString()
    {
         return base.ToString() + $"Type: {TypeHeating} | Temp: {Value}\n" +
            $"ChanceHeating: {ChanceHeating}";
    }
}