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
    Heating = 0,        // Обогрев
    Medicine = 1,       // Лечение
    Clothes = 2,        // Одежда
    FoodAndDrink = 3,   // Еда и питье
    Tools = 4,          // Инструменты
    Materials = 5,      // Материалы
}

public enum DegradationType
{
    None = 0,           // Не портится
    Used = 1,           // Портятся при использовании
    Rate = 2,           // Портятся постоянно
}

public enum MethodOfUse
{
    None = 0,          // Не используется
    OnSelf = 1,        // Используется на себе
    EquipHand = 2,     // Можно взять в руку
    Wear = 3,          // Можно надейт/снять
}

[Flags]
public enum ActionType
{
    Repair = 1,         // Ремонтировать
    Charge = 2,         // Зарядить
    Discharge = 4,      // Разрядить  
    Deconstruct = 8,   // Разобрать 
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
    [field: Tooltip("Тип предмета")]
    [field: SerializeField] public Category Category { get; protected set; }


    [field: Tooltip("Способ приминения")]
    [field: SerializeField] public MethodOfUse UseType { get; protected set; }

    [field: DisableField(nameof(UseType), MethodOfUse.None)]
    [field: SerializeField] public UseStrategy UseStrategy { get; protected set; }


    [field: Tooltip("Название предмета")]
    [field: SerializeField] public string Name { get; private set; } = "InventoryItem";


    [field: Tooltip("Описание предмета")]
    [field: SerializeField, TextArea(3, 150)] public string Description { get; private set; } = string.Empty;


    [field: Tooltip("Иконка предмета")]
    [field: SerializeField, AssetPreview(150, 150)] public Sprite Icon { get; private set; }


    [field: Tooltip("Префам предмета")]
    [field: SerializeField] public WorldItem ItemPrefab { get; private set; }


    [field: Tooltip("Вес предмета [кг]"), Min(0.001f), Space(10)]
    [field: SerializeField] public float Weight { get; private set; } = 1;


    [field: Tooltip("Пополняемый?")]
    [field: SerializeField] public bool IsStackable { get; protected set; } = false;


    [field: Tooltip("Измеряется как целое?")]
    [field: SerializeField] public bool MeasuredAsInteger { get; protected set; } = false;


    [field: Tooltip("Вместимость предмета"), Min(0.001f)]
    [field: SerializeField] public float MaxCapacity { get; protected set; } = 1;

    [field: Tooltip("Единица измерения")]
    [field: SerializeField] public UnitsMeasurement UnitMeasurement { get; protected set; }

    [field: Tooltip("Цена за использование")]
    [field: SerializeField, Min(0.001f)] public float CostOfUse { get; protected set; } = 1f;

    [field: Tooltip("Способ деградировании предмета"), Space(10)]
    [field: SerializeField] public DegradationType DegradeType { get; protected set; }

    [field: Tooltip("Скорость порчи предмета [ед/мин]"), Min(0.001f)]
    [field: SerializeField] public double DegradationValue { get; private set; } = 1;

    [field: Tooltip("Способы взаимодействия с предеметом"), Space(10)]
    [field: SerializeField] public ActionType Actions { get; protected set; }

    [field: Tooltip("Получаемые предметы после разбора")]
    [field: SerializeField] public List<InventorySlot> DeconstructRecipe { get; protected set; }

    [field: Tooltip("Необходимый инструмент и необходимые материалы для починки")]
    [field: SerializeField] public RepairRecipe RepairRecipe { get; protected set; }

    [field: Tooltip("Этим предметом заряжается или получает при разряжении")]
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
