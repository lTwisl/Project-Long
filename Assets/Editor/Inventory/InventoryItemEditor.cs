using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(InventoryItem), true)]
public class InventoryItemEditor : Editor
{
    [SerializeField] private EquipHandStrategy _equipHandStrategy;
    [SerializeField] private UseOnSelfStrategy _useOnSelfStrategy;
    [SerializeField] private WearStrategy _wearStrategy;

    private SerializedProperty _categoryProp;
    private SerializedProperty _useTypeProp;
    private SerializedProperty _useStrategyProp;
    private SerializedProperty _actionsProp;
    private SerializedProperty _nameProp;
    private SerializedProperty _descriptionProp;
    private SerializedProperty _iconProp;
    private SerializedProperty _objectItemProp;
    private SerializedProperty _isStackableProp;
    private SerializedProperty _measuredAsIntegerProp;
    private SerializedProperty _maxStackSizeProp;
    private SerializedProperty _weightProp;
    private SerializedProperty _unitMeasurementProp;
    private SerializedProperty _costOfUseProp;
    private SerializedProperty _degradeTypeProp;
    private SerializedProperty _degradationValueProp;
    private SerializedProperty _lifeTimeProp;
    private SerializedProperty _deconstructNeadProp;
    private SerializedProperty _repairNeadProp;
    private SerializedProperty _chargeNeadProp;

    // For HeatingItem
    private SerializedProperty _typeHeatingProp;
    private SerializedProperty _maxTemperatureProp;
    private SerializedProperty _chanceHeatingProp;

    private VisualElement root;

    protected virtual void OnEnable()
    {
        _categoryProp = serializedObject.FindProperty("<Category>k__BackingField");
        _useTypeProp = serializedObject.FindProperty("<UseType>k__BackingField");
        _useStrategyProp = serializedObject.FindProperty("<UseStrategy>k__BackingField");
        _actionsProp = serializedObject.FindProperty("<Actions>k__BackingField");
        _nameProp = serializedObject.FindProperty("<Name>k__BackingField");
        _descriptionProp = serializedObject.FindProperty("<Description>k__BackingField");
        _iconProp = serializedObject.FindProperty("<Icon>k__BackingField");
        _objectItemProp = serializedObject.FindProperty("<ItemPrefab>k__BackingField");
        _isStackableProp = serializedObject.FindProperty("<IsStackable>k__BackingField");
        _measuredAsIntegerProp = serializedObject.FindProperty("<MeasuredAsInteger>k__BackingField");
        _maxStackSizeProp = serializedObject.FindProperty("<MaxCapacity>k__BackingField");
        _weightProp = serializedObject.FindProperty("<Weight>k__BackingField");
        _unitMeasurementProp = serializedObject.FindProperty("<UnitMeasurement>k__BackingField");
        _costOfUseProp = serializedObject.FindProperty("<CostOfUse>k__BackingField");
        _degradeTypeProp = serializedObject.FindProperty("<DegradeType>k__BackingField");
        _degradationValueProp = serializedObject.FindProperty("<DegradationValue>k__BackingField");
        _deconstructNeadProp = serializedObject.FindProperty("<DeconstructRecipe>k__BackingField");
        _repairNeadProp = serializedObject.FindProperty("<RepairRecipe>k__BackingField");
        _chargeNeadProp = serializedObject.FindProperty("<ChargeRecipe>k__BackingField");

        if (target is HeatingItem)
        {
            _typeHeatingProp = serializedObject.FindProperty("<TypeHeating>k__BackingField");
            _maxTemperatureProp = serializedObject.FindProperty("<MaxTemperature>k__BackingField");
            _chanceHeatingProp = serializedObject.FindProperty("<ChanceHeating>k__BackingField");
        }
    }

    public override VisualElement CreateInspectorGUI()
    {
        root = new VisualElement();

        string typeName = serializedObject.targetObject.GetType().Name;

        DrawCategoryProp(typeName);

        DrawUseTypeProp(typeName);

        DrawUseStrategyProp();



        root.Add(new PropertyField(_nameProp));

        root.Add(new PropertyField(_descriptionProp));

        root.Add(new PropertyField(_iconProp));

        root.Add(new PropertyField(_objectItemProp));

        root.Add(new PropertyField(_weightProp));

        DrawUnitMeasurement(typeName);

        DrawIsStackableProp(typeName);

        DrawMeasuredAsIntegerProp(typeName);

        DrawMaxCapacityProp(typeName);

        DrawCostOfUseProp();

        DrawDegradeTypeAndValue(typeName);

        DrawActionsAndValues();

        if (target is HeatingItem)
        {
            DrawHeatingItemFields();
        }
        else
        {
            var fieldIterator = serializedObject.GetIterator();
            var last = typeof(InventoryItem).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Last();

            // Пропустить все поля базового класса
            while (fieldIterator.NextVisible(true))
            {
                if (fieldIterator.name == last.Name)
                    break;
            }

            // Отрисовать все поля дочернего класса
            while (fieldIterator.NextVisible(false))
            {
                root.Add(new PropertyField(fieldIterator));
            }
        }
        return root;
    }

    private void DrawHeatingItemFields()
    {
        var typeHeatingField = new PropertyField(_typeHeatingProp);
        root.Add(typeHeatingField);

        var maxTemperatureField = new PropertyField(_maxTemperatureProp);
        root.Add(maxTemperatureField);

        var chanceHeatingField = new PropertyField(_chanceHeatingProp);
        root.Add(chanceHeatingField);

        typeHeatingField.RegisterValueChangeCallback(value =>
        {
            switch ((HeatingItem.HeatingType)_typeHeatingProp.enumValueIndex)
            {
                case HeatingItem.HeatingType.HeatingElement:
                    maxTemperatureField.style.display = DisplayStyle.Flex;
                    chanceHeatingField.style.display = DisplayStyle.Flex;
                    break;
                case HeatingItem.HeatingType.ThermalInsulationPaste:
                    maxTemperatureField.style.display = DisplayStyle.None;
                    chanceHeatingField.style.display = DisplayStyle.Flex;
                    break;
                default:
                    maxTemperatureField.style.display = DisplayStyle.None;
                    chanceHeatingField.style.display = DisplayStyle.None;
                    break;
            }
        });
    }

    private void DrawUseStrategyProp()
    {
        var field = new PropertyField(_useStrategyProp);

        field.RegisterValueChangeCallback(value => OnUseTypeChanged((MethodOfUse)_useTypeProp.enumValueIndex));

        root.Add(field);
    }

    private void DrawCategoryProp(string typeName)
    {
        bool isEnable = typeName switch
        {
            nameof(ClothingItem) => false,
            nameof(MedicineItem) => false,
            nameof(HeatingItem) => false,
            _ => true,
        };
        var field = new PropertyField(_categoryProp);
        field.SetEnabled(isEnable);
        root.Add(field);
    }

    private void DrawUseTypeProp(string typeName)
    {
        bool isEnable = typeName switch
        {
            nameof(ConsumablesItem) => false,
            nameof(ClothingItem) => false,
            nameof(MaterialItem) => false,
            nameof(MedicineItem) => false,
            _ => true,
        };

        var field = new PropertyField(_useTypeProp);
        field.RegisterValueChangeCallback(value => OnUseTypeChanged((MethodOfUse)value.changedProperty.enumValueIndex));

        field.SetEnabled(isEnable);
        root.Add(field);
    }

    private void OnUseTypeChanged(MethodOfUse methodOfUse)
    {
        // Устанавливаем соответствующую стратегию
        switch (methodOfUse)
        {
            case MethodOfUse.OnSelf:
                SetUseStrategy(typeof(UseOnSelfStrategy),
                    _useOnSelfStrategy);
                break;
            case MethodOfUse.EquipHand:
                SetUseStrategy(typeof(EquipHandStrategy),
                    _equipHandStrategy);
                break;
            case MethodOfUse.Wear:
                SetUseStrategy(typeof(WearStrategy),
                    _wearStrategy);
                break;
            default:
                _useStrategyProp.objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
                break;
        }
    }

    private void SetUseStrategy(Type requiredType, UnityEngine.Object defaultStrategy)
    {
        if (_useStrategyProp.objectReferenceValue == null)
        {
            _useStrategyProp.objectReferenceValue = defaultStrategy;
        }
        else if (!_useStrategyProp.objectReferenceValue.GetType().IsSubclassOf(requiredType))
        {
            _useStrategyProp.objectReferenceValue = defaultStrategy;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDegradeTypeAndValue(string typeName)
    {
        bool isEnable = typeName switch
        {
            nameof(ToolItem) => false,
            nameof(HeatingItem) => false,
            _ => true,
        };

        var degradeTypeField = new PropertyField(_degradeTypeProp);
        degradeTypeField.SetEnabled(isEnable);
        root.Add(degradeTypeField);

        Func<double, double> convert = value => 100.0 / (value * 60 * 24); // Преобразование времени жизни в скорость деградации и наоборот
        (double min, double max) degradationRate = (convert(90), 100);
        (double min, double max) lifeTime = (convert(degradationRate.max), convert(degradationRate.min));

        var lifeTimeField = new DoubleField("Life Time [days]");
        lifeTimeField.value = convert(_degradationValueProp.doubleValue);
        lifeTimeField.AddToClassList("unity-base-field__aligned");
        lifeTimeField.RegisterValueChangedCallback(value =>
        {
            Undo.RecordObject(serializedObject.targetObject, "Change Degradation Rate"); // Исправлено
            lifeTimeField.value = Math.Clamp(value.newValue, lifeTime.min, lifeTime.max);
            _degradationValueProp.doubleValue = convert(lifeTimeField.value);
            serializedObject.ApplyModifiedProperties();
        });

        var degradeValueField = new DoubleField("Degradation Rate [unit/min]");
        degradeValueField.value = _degradationValueProp.doubleValue;
        degradeValueField.AddToClassList("unity-base-field__aligned");
        degradeValueField.RegisterValueChangedCallback(value =>
        {
            Undo.RecordObject(serializedObject.targetObject, "Change Degradation Rate"); // Исправлено
            degradeValueField.value = Math.Clamp(value.newValue, degradationRate.min, degradationRate.max);
            _degradationValueProp.doubleValue = degradeValueField.value;
            serializedObject.ApplyModifiedProperties();
        });

        root.TrackPropertyValue(_degradationValueProp, prop =>
        {
            DegradationType currentType = (DegradationType)_degradeTypeProp.enumValueIndex;
            if (currentType == DegradationType.Rate)
                lifeTimeField.value = convert(prop.doubleValue);
            else if (currentType == DegradationType.Used)
                degradeValueField.value = prop.doubleValue;
        });


        root.Add(lifeTimeField);
        root.Add(degradeValueField);

        UpdateDegradationValueField((DegradationType)_degradeTypeProp.enumValueIndex);
        degradeTypeField.RegisterValueChangeCallback(value => UpdateDegradationValueField((DegradationType)value.changedProperty.enumValueIndex));

        void UpdateDegradationValueField(DegradationType type)
        {
            switch (type)
            {
                case DegradationType.None:
                    lifeTimeField.style.display = DisplayStyle.None;
                    degradeValueField.style.display = DisplayStyle.None;
                    break;
                case DegradationType.Used:
                    lifeTimeField.style.display = DisplayStyle.None;
                    degradeValueField.style.display = DisplayStyle.Flex;
                    degradeValueField.value = _degradationValueProp.doubleValue;
                    break;
                case DegradationType.Rate:
                    lifeTimeField.style.display = DisplayStyle.Flex;
                    degradeValueField.style.display = DisplayStyle.None;
                    lifeTimeField.value = convert(_degradationValueProp.doubleValue);
                    break;
            }
        }
    }

    private void DrawIsStackableProp(string typeName)
    {
        bool isEnable = typeName switch
        {
            nameof(ToolItem) => false,
            nameof(ClothingItem) => false,
            _ => true,
        };

        var field = new PropertyField(_isStackableProp);
        field.SetEnabled(isEnable);
        root.Add(field);
    }

    private void DrawMeasuredAsIntegerProp(string typeName)
    {
        bool isEnable = typeName switch
        {
            nameof(ToolItem) => false,
            nameof(ClothingItem) => false,
            _ => true,
        };

        var measuredField = new PropertyField(_measuredAsIntegerProp);
        measuredField.SetEnabled(isEnable);
        root.Add(measuredField);
    }

    private void DrawMaxCapacityProp(string typeName)
    {
        var container = new BindableElement();
        container.SetEnabled(typeName != nameof(ToolItem) && typeName != nameof(ClothingItem));

        // Создаем оба варианта полей
        var integerField = new IntegerField("Max Capacity [integer]");
        integerField.AddToClassList("unity-base-field__aligned");

        root.TrackPropertyValue(_maxStackSizeProp, prop =>
        {
            if (_measuredAsIntegerProp.boolValue)
                integerField.value = (int)prop.floatValue;
        });

        var floatField = new PropertyField(_maxStackSizeProp, "Max Capacity [float]");

        // Настраиваем начальное состояние
        UpdateCapacityFields(_measuredAsIntegerProp.boolValue);

        // Подписываемся на изменения MeasuredAsInteger
        container.TrackPropertyValue(_measuredAsIntegerProp, prop =>
        {
            bool useInteger = prop.boolValue;
            UpdateCapacityFields(useInteger);

            // Конвертация значений при переключении
            if (useInteger)
                _maxStackSizeProp.floatValue = Mathf.Max(1, (int)_maxStackSizeProp.floatValue);
        });

        // Обработчик изменения integer поля
        integerField.RegisterValueChangedCallback(evt =>
        {
            int newValue = Mathf.Max(1, evt.newValue);
            _maxStackSizeProp.floatValue = newValue;
            integerField.value = newValue;
            _maxStackSizeProp.serializedObject.ApplyModifiedProperties();
        });

        // Добавляем элементы
        container.Add(integerField);
        container.Add(floatField);

        root.Add(container);

        void UpdateCapacityFields(bool showInteger)
        {
            integerField.style.display = showInteger ? DisplayStyle.Flex : DisplayStyle.None;
            floatField.style.display = showInteger ? DisplayStyle.None : DisplayStyle.Flex;

            if (showInteger)
                integerField.value = Mathf.Max(1, (int)_maxStackSizeProp.floatValue);
        }
    }

    private void DrawCostOfUseProp()
    {
        var field = new PropertyField(_costOfUseProp);

        UpdateCostOfUseFields(_measuredAsIntegerProp);
        field.TrackPropertyValue(_measuredAsIntegerProp, UpdateCostOfUseFields);

        root.Add(field);

        void UpdateCostOfUseFields(SerializedProperty prop)
        {
            field.SetEnabled(!prop.boolValue);
            if (prop.boolValue == true)
                _costOfUseProp.floatValue = 1;
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void DrawActionsAndValues()
    {
        var actionsField = new PropertyField(_actionsProp);
        root.Add(actionsField);

        var deconstructNeadField = new PropertyField(_deconstructNeadProp);
        root.Add(deconstructNeadField);

        var repairNeadField = new PropertyField(_repairNeadProp);
        root.Add(repairNeadField);

        var chargeNeadField = new PropertyField(_chargeNeadProp);
        root.Add(chargeNeadField);

        UpdateValuesField((ActionType)_actionsProp.enumValueFlag);
        actionsField.RegisterValueChangeCallback(value => { UpdateValuesField((ActionType)value.changedProperty.enumValueFlag); });

        void UpdateValuesField(ActionType type)
        {
            deconstructNeadField.style.display = type.HasFlag(ActionType.Deconstruct) ? DisplayStyle.Flex : DisplayStyle.None;
            repairNeadField.style.display = type.HasFlag(ActionType.Repair) ? DisplayStyle.Flex : DisplayStyle.None;
            chargeNeadField.style.display = (type.HasFlag(ActionType.Charge) || type.HasFlag(ActionType.Discharge)) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void DrawUnitMeasurement(string typeName)
    {
        bool isEnable = typeName switch
        {
            nameof(ToolItem) => false,
            nameof(ClothingItem) => false,
            _ => true,
        };

        var unitMeasurementField = new PropertyField(_unitMeasurementProp);
        unitMeasurementField.SetEnabled(isEnable);
        root.Add(unitMeasurementField);
    }
}
