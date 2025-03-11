using UnityEngine;

public enum ClothesType
{
    Hat,
    Outerwear,
    Undergarments,
    Gloves,
    Trousers,
    Underpants,
    Socks,
    Boots,
    Accessories,
}

[CreateAssetMenu(fileName = "ClothesItem", menuName = "Items/Clothes")]
public class ClothesItem : InventoryItem
{
    [field: Header("Additional Properties")]
    [field: SerializeField] public ClothesType ClothesType { get; private set; }
    [field: SerializeField] public float WindProtection { get; private set; }
    [field: SerializeField] public float WaterProtection { get; private set; }
    [field: SerializeField] public float TemperatureBonus { get; private set; }
    [field: SerializeField] public float ToxisityProtection { get; private set; }
    [field: SerializeField, Range(0f, 1f)] public float FrictionBonus { get; private set; }

    private void OnEnable()
    {
        Category = Category.Clothes;
        UseType = MethodOfUse.Wear;
        Actions = ActionType.Repair | ActionType.Deconstruct;

        IsStackable = false;
        MeasuredAsInteger = true;
        MaxCapacity = 1;
        DegradeType = DegradationType.Rate;
    }

    public override string ToString()
    {
        return base.ToString() + $"Temp: {TemperatureBonus} | " +
            $"Water: {WaterProtection}\n" +
            $"Wind: {WindProtection} | " +
            $"Friction: {FrictionBonus}\n" +
            $"Toxisity: {ToxisityProtection}\n";
    }
}
