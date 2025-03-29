using UnityEngine;

public enum ClothesType
{
    Hat,
    Outerwear,
    Undershirt,
    Gloves,
    Pants,
    Underpants,
    Socks,
    Boots,
    Accessories,
}

[CreateAssetMenu(fileName = "ClothingItem", menuName = "Items/Clothing")]
public class ClothingItem : InventoryItem
{
    [field: Header("- - Additional Properties - -")]
    [field: SerializeField] public ClothesType ClothingType { get; private set; }
    [field: SerializeField, Min(0)] public float TemperatureBonus { get; private set; }
    [field: SerializeField, Range(0, 100)] public float WaterProtection { get; private set; }
    [field: SerializeField, Range(0, 100)] public float WindProtection { get; private set; }
    [field: SerializeField, Range(0, 100)] public float ToxicityProtection { get; private set; }
    [field: SerializeField, Range(0, 100)] public float PhysicProtection { get; private set; }
    [field: SerializeField, Range(-100, 100)] public float OffsetStamina { get; private set; }
    [field: SerializeField, Range(0f, 1f)] public float FrictionBonus { get; private set; }
    [field: Space(8)]
    [field: SerializeField, Min(0)] public float DryingRate { get; private set; }
    [field: SerializeField, Range(0, 5)] public float WaterAbsorptionRatio { get; private set; }

    private void OnEnable()
    {
        Category = Category.Clothes;
        UseType = MethodOfUse.Wear;
        Actions = ActionType.Repair | ActionType.Deconstruct;

        IsStackable = false;
        MeasuredAsInteger = true;
        MaxCapacity = 1;
        DegradeType = DegradationType.Rate;
        CostOfUse = 0;
    }

    public override string ToString()
    {
        return base.ToString() + $"Temp: {TemperatureBonus} | " +
            $"Water: {WaterProtection}\n" +
            $"Wind: {WindProtection} | " +
            $"Friction: {FrictionBonus}\n" +
            $"Toxisity: {ToxicityProtection}\n";
    }
}
