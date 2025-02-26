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
        Heating = 0,        // Обогрев
        Medicine = 1,       // Лечение
        Clothes = 2,        // Одежда
        FoodAndDrink = 3,   // Еда и питье
        Tools = 4,           // Инструменты
        Materials = 5,       // Материалы
    }


    public enum DegradationType
    {
        None = 0,           // Не портится
        Used = 1,           // Портятся при использовании
        Rate = 2,           // Портятся постоянно
    }

    public enum MethodOfUse
    {
        None = 0,           // Не используется
        OnSelf = 1,         // Используется на себе
        TakeInHand = 2,     // Можно взять в руку
        TakeOffPutOn = 3,   // Можно надейт/снять
    }

    [Flags]
    public enum ActionType
    {
        Repair = 1,         // Ремонтировать
        Charge = 2,         // Зарядить
        Discharge = 4,      // Заправить 
        Refuel = 8,         // Разрядить 
        Deconstruct = 16,   // Разобрать 
    }


    [field: Tooltip("Тип предмета")]
    [field: SerializeField] public ItemType Category { get; protected set; }


    [field: Tooltip("Способ приминения")]
    [field: SerializeField] public MethodOfUse UseType { get; protected set; }

    [field: Tooltip("Способы взаимодействия с предеметом")]
    [field: SerializeField] public ActionType Actions { get; protected set; }


    [field: Tooltip("Название предмета")]
    [field: SerializeField] public string Name { get; private set; } = "InventoryItem";


    [field: Tooltip("Описание предмета")]
    [field: SerializeField, Multiline] public string Description { get; private set; } = string.Empty;


    [field: Tooltip("Иконка предмета")]
    [field: SerializeField] public Sprite Icon { get; private set; }


    [field: Tooltip("Префам предмета")]
    [field: SerializeField] public GameObject ItemPrefab { get; private set; }


    [field: Tooltip("Вес предмета"), Min(0.001f)]
    [field: SerializeField] public float Weight { get; private set; } = 1;


    [field: Tooltip("Пополняемый?")]
    [field: SerializeField] public bool IsStackable { get; protected set; } = false;


    [field: Tooltip("Измеряется как целое?")]
    [field: SerializeField] public bool MeasuredAsInteger { get; protected set; } = false;


    [field: Tooltip("Вместимость предмета"), Min(0.001f)]
    [field: SerializeField] public float MaxCapacity { get; protected set; } = 1;


    [field: Tooltip("Деградирования предмета")]
    [field: SerializeField] public DegradationType DegradeType { get; protected set; }


    [field: Tooltip("Скорость порчи предмета"), Min(0.001f)]
    [field: SerializeField] public float DegradationValue { get; private set; } = 1;
}
