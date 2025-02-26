using System;
using UnityEngine;

[System.Serializable]
public struct PairParamterAndValue
{
    public ParameterType ParameterType;
    public float Value;
}

public abstract class InventoryItem : ScriptableObject
{
    public enum ItemType
    {
        Heating = 0,        // �������
        Medicine = 1,       // �������
        Clothes = 2,        // ������
        FoodAndDrink = 3,   // ��� � �����
        Tools = 4,           // �����������
        Materials = 5,       // ���������
    }


    public enum DegradationType
    {
        None = 0,           // �� ��������
        Used = 1,           // �������� ��� �������������
        Rate = 2,           // �������� ���������
    }

    public enum MethodOfUse
    {
        None = 0,           // �� ������������
        OnSelf = 1,         // ������������ �� ����
        TakeInHand = 2,     // ����� ����� � ����
        TakeOffPutOn = 3,   // ����� ������/�����
    }

    [Flags]
    public enum ActionType
    {
        Repair = 1,         // �������������
        Charge = 2,         // ��������
        Discharge = 4,      // ��������� 
        Refuel = 8,         // ��������� 
        Deconstruct = 16,   // ��������� 
    }


    [field: Tooltip("��� ��������")]
    [field: SerializeField] public ItemType Category { get; protected set; }


    [field: Tooltip("������ ����������")]
    [field: SerializeField] public MethodOfUse UseType { get; protected set; }

    [field: Tooltip("������� �������������� � ����������")]
    [field: SerializeField] public ActionType Actions { get; protected set; }


    [field: Tooltip("�������� ��������")]
    [field: SerializeField] public string Name { get; private set; } = "InventoryItem";


    [field: Tooltip("�������� ��������")]
    [field: SerializeField, Multiline] public string Description { get; private set; } = string.Empty;


    [field: Tooltip("������ ��������")]
    [field: SerializeField] public Sprite Icon { get; private set; }


    [field: Tooltip("������ ��������")]
    [field: SerializeField] public GameObject ItemPrefab { get; private set; }


    [field: Tooltip("��� ��������"), Min(0.001f)]
    [field: SerializeField] public float Weight { get; private set; } = 1;


    [field: Tooltip("�����������?")]
    [field: SerializeField] public bool IsStackable { get; protected set; } = false;


    [field: Tooltip("���������� ��� �����?")]
    [field: SerializeField] public bool MeasuredAsInteger { get; protected set; } = false;


    [field: Tooltip("����������� ��������"), Min(0.001f)]
    [field: SerializeField] public float MaxCapacity { get; protected set; } = 1;


    [field: Tooltip("�������������� ��������")]
    [field: SerializeField] public DegradationType DegradeType { get; protected set; }


    [field: Tooltip("�������� ����� ��������"), Min(0.001f)]
    [field: SerializeField] public float DegradationValue { get; private set; } = 1;
}
