using EditorAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ReplenishmentParameter
{
    public ParameterType ParameterType;
    public float Value;
}

public enum Category
{
    Heating = 0,        // �������
    Medicine = 1,       // �������
    Clothes = 2,        // ������
    FoodAndDrink = 3,   // ��� � �����
    Tools = 4,          // �����������
    Materials = 5,      // ���������
}

public enum DegradationType
{
    None = 0,           // �� ��������
    Used = 1,           // �������� ��� �������������
    Rate = 2,           // �������� ���������
}

public enum MethodOfUse
{
    None = 0,          // �� ������������
    OnSelf = 1,        // ������������ �� ����
    EquipHand = 2,     // ����� ����� � ����
    Wear = 3,          // ����� ������/�����
}

[Flags]
public enum ActionType
{
    Repair = 1,         // �������������
    Charge = 2,         // ��������
    Discharge = 4,      // ���������  
    Deconstruct = 8,   // ��������� 
}

public enum UnitsMeasurement
{
    None,
    Kg,
    L,
    Unit,
    Charge,
}

public interface ReplenishingPlayerParameters
{
    public List<ReplenishmentParameter> ReplenishmentParameters { get; }
}

public interface GiverOfBonuses
{
    public List<ScriptableObject> GivesBonus { get; }
}

public interface HealingDiseases
{
    public List<ScriptableObject> HealsDisease { get; }
}




[System.Serializable]
public struct RepairRecipe
{
    public ToolItem Tool;
    public List<InventorySlot> Items;
}

public abstract class InventoryItem : ScriptableObject
{
    [field: Tooltip("��� ��������")]
    [field: SerializeField] public Category Category { get; protected set; }


    [field: Tooltip("������ ����������")]
    [field: SerializeField] public MethodOfUse UseType { get; protected set; }

    [field: DisableField(nameof(UseType), MethodOfUse.None)]
    [field: SerializeField] public UseStrategy UseStrategy { get; protected set; }


    [field: Tooltip("�������� ��������")]
    [field: SerializeField] public string Name { get; private set; } = "InventoryItem";


    [field: Tooltip("�������� ��������")]
    [field: SerializeField, TextArea(3, 150)] public string Description { get; private set; } = string.Empty;


    [field: Tooltip("������ ��������")]
    [field: SerializeField, AssetPreview(150, 150)] public Sprite Icon { get; private set; }


    [field: Tooltip("������ ��������")]
    [field: SerializeField] public WorldItem ItemPrefab { get; private set; }


    [field: Tooltip("��� �������� [��]"), Min(0.001f), Space(10)]
    [field: SerializeField] public float Weight { get; private set; } = 1;


    [field: Tooltip("�����������?")]
    [field: SerializeField] public bool IsStackable { get; protected set; } = false;


    [field: Tooltip("���������� ��� �����?")]
    [field: SerializeField] public bool MeasuredAsInteger { get; protected set; } = false;


    [field: Tooltip("����������� ��������"), Min(0.001f)]
    [field: SerializeField] public float MaxCapacity { get; protected set; } = 1;

    [field: Tooltip("������� ���������")]
    [field: SerializeField] public UnitsMeasurement UnitMeasurement { get; protected set; }

    [field: Tooltip("���� �� �������������")]
    [field: SerializeField, Min(0.001f)] public float CostOfUse { get; protected set; } = 1f;

    [field: Tooltip("������ �������������� ��������"), Space(10)]
    [field: SerializeField] public DegradationType DegradeType { get; protected set; }

    [field: Tooltip("�������� ����� �������� [��/���]"), Min(0.001f)]
    [field: SerializeField] public double DegradationValue { get; private set; } = 1;

    [field: Tooltip("������� �������������� � ����������"), Space(10)]
    [field: SerializeField] public ActionType Actions { get; protected set; }

    [field: Tooltip("���������� �������� ����� �������")]
    [field: SerializeField] public List<InventorySlot> DeconstructRecipe { get; protected set; }

    [field: Tooltip("����������� ���������� � ����������� ��������� ��� �������")]
    [field: SerializeField] public RepairRecipe RepairRecipe { get; protected set; }

    [field: Tooltip("���� ��������� ���������� ��� �������� ��� ����������")]
    [field: SerializeField] public InventoryItem ChargeRecipe { get; protected set; }

    public override string ToString()
    {
        return $"Asset: {name} | " +
            $"Item: {Name}\n" +
            $"Description: {Description}\n";
    }

    public void Use(InventorySlot parentSlot)
    {
        if (UseStrategy == null)
            return;

        UseStrategy.Execute(parentSlot);
    }
}
