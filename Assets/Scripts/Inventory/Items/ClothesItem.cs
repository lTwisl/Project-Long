using UnityEngine;

[CreateAssetMenu(fileName = "ClothesItem", menuName = "Items/Clothes")]
public class ClothesItem : InventoryItem
{
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

    [field: Header("Additional Properties")]
    [field: SerializeField] public ClothesType TypeClothes { get; private set; }
    [field: SerializeField] public float WindProtection { get; private set; }
    [field: SerializeField] public float WaterProtection { get; private set; }
    [field: SerializeField] public float TemperatureBonus { get; private set; }
    [field: SerializeField] public float ToxisityProtection { get; private set; }
    [field: SerializeField, Range(0f, 1f)] public float FrictionBonus { get; private set; }

    private void OnEnable()
    {
        Category = ItemType.Clothes;
        UseType = MethodOfUse.TakeOffPutOn;
        Actions = ActionType.Repair | ActionType.Deconstruct;

        IsStackable = false;
        MeasuredAsInteger = true;
        MaxCapacity = 1;
        DegradeType = DegradationType.Rate;
    }

    public override string GetInfo()
    {
        return base.GetInfo() + $"Temp = {TemperatureBonus}" +
            $" | Water = {WaterProtection}" +
            $" | Wind = {WindProtection}" +
            $" | Friction = {FrictionBonus}" +
            $" | Toxisity = {ToxisityProtection}";
    }
}
