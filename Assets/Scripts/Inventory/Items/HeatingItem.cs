using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "HeatingItem", menuName = "Items/Heating")]
public class HeatingItem : InventoryItem
{
    public enum HeatingType
    {
        Fuel = 0,
        HeatingElement = 1,
        ThermalInsulationPaste = 2,
    }

    [field: Header("Additional Properties")]
    [field: OnValueChanged(nameof(OnTypeHeatingChanged))]
    [field: SerializeField] public HeatingType TypeHeating { get; private set; }

    [field: Tooltip("Только для HeatingElement")]
    [field: SerializeField, Min(0f)] public float MaxTemperature { get; private set; }

    [field: Tooltip("Только для HeatingElement и ThermalInsulationPaste")]
    [field: SerializeField, Range(0, 100)] public float ChanceHeating { get; private set; }


    private void OnEnable()
    {
        Category = Category.Heating;
    }

    public override string ToString()
    {
        return base.ToString() + $"Type: {TypeHeating} | Temperature: {MaxTemperature}\n" +
           $"ChanceHeating: {ChanceHeating}";
    }

    public void OnTypeHeatingChanged()
    {
        DegradeType = TypeHeating switch
        {
            HeatingType.Fuel => DegradationType.Rate,
            HeatingType.HeatingElement => DegradationType.Used,
            _ => DegradationType.None,
        };
    }
}
