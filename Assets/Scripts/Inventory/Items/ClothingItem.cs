using System.Collections.Generic;
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

    [field: Min(0), Tooltip("Температурный бонус [град. Цельсия]")]
    [field: SerializeField] public float TemperatureBonus { get; private set; }

    [field: Range(0, 1), Tooltip("Защита от влаги [%]")]
    [field: SerializeField] public float WaterProtection { get; private set; }

    [field: Range(0, 1), Tooltip("Защита от ветра [%]")]
    [field: SerializeField] public float WindProtection { get; private set; }

    [field: Range(0, 1), Tooltip("Защита от заражения [%]")]
    [field: SerializeField] public float ToxicityProtection { get; private set; }

    [field: Range(0, 1), Tooltip("Защита от физического урона [%]")]
    [field: SerializeField] public float PhysicProtection { get; private set; }

    [field: Range(-1, 1), Tooltip("Добавочное значение к максимальному значению выносливости [%]")]
    [field: SerializeField] public float OffsetStamina { get; private set; }

    [field: Range(0f, 1f), Tooltip("Бонус к трению с поверхностью [ед]")]
    [field: SerializeField] public float FrictionBonus { get; private set; }

    [field: Range(0, 5), Tooltip("Коэффициент впитывания влаги (множитель веса, если равен 0, то вещь не намокает)")]
    [field: SerializeField] public float WaterAbsorptionRatio { get; private set; }

    [field: Space(8), Min(0), Tooltip("Скорость высыхания [ед/мин]")]
    [field: SerializeField] public float DryingRate { get; private set; }


    [field: SerializeField] public List<ScriptableObject> GivesBonus { get; private set; }

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
        return base.ToString() + $"Temperature: {TemperatureBonus} | " +
            $"Water: {WaterProtection}\n" +
            $"Wind: {WindProtection} | " +
            $"_friction: {FrictionBonus}\n" +
            $"Toxisity: {ToxicityProtection}\n";
    }

    private void OnValidate()
    {
        if (DegradeType == DegradationType.Used)
            DegradeType = DegradationType.Rate;
    }
}
