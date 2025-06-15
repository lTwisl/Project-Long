using System.Collections.Generic;
using UnityEngine;

public enum ClothesType
{
    Hat,
    Jacket,
    Shirt,
    Gloves,
    Pants,
    Underpants,
    Socks,
    Boots,
    Accessories,
}

public enum ClothingLayer
{
    Upper,
    Lower,
}

[CreateAssetMenu(fileName = "ClothingItem", menuName = "Items/Clothing")]
public class ClothingItem : InventoryItem, IGiverOfEffects
{
    [field: Header("- - Additional Properties - -")]
    [field: SerializeField] public ClothesType ClothingType { get; private set; }

    [field: Min(0), Tooltip("������������� ����� [����. �������]")]
    [field: SerializeField] public float TemperatureBonus { get; private set; }

    [field: Range(0, 1), Tooltip("������ �� ����� [%]")]
    [field: SerializeField] public float WaterProtection { get; private set; }

    [field: Range(0, 1), Tooltip("������ �� ����� [%]")]
    [field: SerializeField] public float WindProtection { get; private set; }

    [field: Range(0, 1), Tooltip("������ �� ��������� [%]")]
    [field: SerializeField] public float ToxicityProtection { get; private set; }

    [field: Range(0, 1), Tooltip("������ �� ����������� ����� [%]")]
    [field: SerializeField] public float PhysicProtection { get; private set; }

    [field: Range(-1, 1), Tooltip("���������� �������� � ������������� �������� ������������ [%]")]
    [field: SerializeField] public float StaminaBonus { get; private set; }

    [field: Range(0f, 1f), Tooltip("����� � ������ � ������������ [��]")]
    [field: SerializeField] public float FrictionBonus { get; private set; }

    [field: Range(0, 5), Tooltip("����������� ���������� ����� (��������� ����, ���� ����� 0, �� ���� �� ��������)")]
    [field: SerializeField] public float WaterAbsorptionRatio { get; private set; }

    [field: Min(0), Tooltip("�������� ��������� [��/���]"), Space]
    [field: SerializeField] public float DryingRate { get; private set; }

    [field: SerializeField] public List<ScriptableObject> GivesEffects { get; private set; }

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
            $"Friction: {FrictionBonus}\n" +
            $"Toxisity: {ToxicityProtection}\n" +
            $"OffsetStamina: {StaminaBonus}\n";
    }

    private void OnValidate()
    {
        if (DegradeType == DegradationType.Used)
            DegradeType = DegradationType.Rate;
    }
}
