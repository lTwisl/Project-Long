using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryItem), true)]
public class InventoryItemEditor : Editor
{
    protected SerializedProperty _categoryProp;
    protected SerializedProperty _useTypeProp;
    protected SerializedProperty _actionsProp;
    protected SerializedProperty _nameProp;
    protected SerializedProperty _descriptionProp;
    protected SerializedProperty _iconProp;
    protected SerializedProperty _objectItemProp;
    protected SerializedProperty _isStackableProp;
    protected SerializedProperty _measuredAsIntegerProp;
    protected SerializedProperty _maxStackSizeProp;
    protected SerializedProperty _weightProp;
    protected SerializedProperty _unitMeasurementProp;
    protected SerializedProperty _degradeTypeProp;
    protected SerializedProperty _degradationValueProp;
    protected SerializedProperty _receivedItemsAfterDeconstructProp;

    protected virtual void OnEnable()
    {
        _categoryProp = serializedObject.FindProperty("<Category>k__BackingField");
        _useTypeProp = serializedObject.FindProperty("<UseType>k__BackingField");
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
        _degradeTypeProp = serializedObject.FindProperty("<DegradeType>k__BackingField");
        _degradationValueProp = serializedObject.FindProperty("<DegradationValue>k__BackingField");
        _receivedItemsAfterDeconstructProp = serializedObject.FindProperty("<ReceivedItemsAfterDeconstruct>k__BackingField");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        string typeName = serializedObject.targetObject.GetType().Name;

        DrawCategoryProp(typeName);
        DrawUseTypeProp(typeName);

        EditorGUILayout.PropertyField(_actionsProp);
        EditorGUILayout.PropertyField(_nameProp);
        EditorGUILayout.PropertyField(_descriptionProp);

        DrawIconPreview();

        EditorGUILayout.PropertyField(_objectItemProp);
        EditorGUILayout.PropertyField(_weightProp);

        EditorGUILayout.Space(10);

        DrawIsStackableProp(typeName);
        DrawMeasuredAsIntegerProp(typeName);
        DrawMaxCapacityProp(typeName);

        DrawUnitMeasurement(typeName);


        EditorGUILayout.Space(10);

        DrawDegradeType(typeName);

        EditorGUI.indentLevel++;
        if (_degradeTypeProp.enumValueIndex == 1)
            EditorGUILayout.PropertyField(_degradationValueProp, new GUIContent("Degradation Used"));
        else if (_degradeTypeProp.enumValueIndex == 2)
            EditorGUILayout.PropertyField(_degradationValueProp, new GUIContent("Degradation Rate"));
        EditorGUI.indentLevel--;

        if (((InventoryItem.ActionType)_actionsProp.enumValueFlag).HasFlag(InventoryItem.ActionType.Deconstruct))
            EditorGUILayout.PropertyField(_receivedItemsAfterDeconstructProp);

        EditorGUILayout.Space(20);

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
            EditorGUILayout.PropertyField(fieldIterator);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawIconPreview()
    {
        EditorGUILayout.BeginHorizontal();

        // Лейбл "Icon"
        EditorGUILayout.PrefixLabel("Icon");

        // Получаем текущий спрайт
        Sprite currentSprite = _iconProp.objectReferenceValue as Sprite;

        // Отображаем ObjectField с превью
        Sprite newSprite = (Sprite)EditorGUILayout.ObjectField(
            currentSprite,
            typeof(Sprite),
            false, // Не разрешаем объекты из сцены
            GUILayout.Width(256), // Ширина превью
            GUILayout.Height(256) // Высота превью
        );

        // Обновляем значение, если спрайт изменился
        if (newSprite != currentSprite)
        {
            _iconProp.objectReferenceValue = newSprite;
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawCategoryProp(string typeName)
    {
        GUI.enabled = typeName switch
        {
            nameof(ClothesItem) => false,
            nameof(MedicineItem) => false,
            nameof(HeatingItem) => false,
            _ => true,
        };

        EditorGUILayout.PropertyField(_categoryProp);

        GUI.enabled = true;
    }

    private void DrawUseTypeProp(string typeName)
    {
        GUI.enabled = typeName switch
        {
            nameof(Consumables) => false,
            nameof(ClothesItem) => false,
            nameof(MaterialItem) => false,
            nameof(MedicineItem) => false,
            _ => true,
        };

        EditorGUILayout.PropertyField(_useTypeProp);

        GUI.enabled = true;
    }

    private void DrawDegradeType(string typeName)
    {
        GUI.enabled = typeName switch
        {
            nameof(ToolItem) => false,
            nameof(ClothesItem) => false,
            _ => true,
        };

        EditorGUILayout.PropertyField(_degradeTypeProp);

        GUI.enabled = true;
    }

    private void DrawIsStackableProp(string typeName)
    {
        GUI.enabled = typeName switch
        {
            nameof(ToolItem) => false,
            nameof(ClothesItem) => false,
            _ => true,
        };

        EditorGUILayout.PropertyField(_isStackableProp);

        GUI.enabled = true;
    }

    private void DrawMeasuredAsIntegerProp(string typeName)
    {
        GUI.enabled = typeName switch
        {
            nameof(ToolItem) => false,
            nameof(ClothesItem) => false,
            _ => true,
        };

        EditorGUILayout.PropertyField(_measuredAsIntegerProp);

        GUI.enabled = true;
    }

    private void DrawMaxCapacityProp(string typeName)
    {
        GUI.enabled = typeName switch
        {
            nameof(ToolItem) => false,
            nameof(ClothesItem) => false,
            _ => true,
        };

        if (_measuredAsIntegerProp.boolValue)
        {
            int currentValue = (int)_maxStackSizeProp.floatValue;
            int newValue = EditorGUILayout.IntField("Max Capacity [integer]", currentValue);

            newValue = Mathf.Max(1, newValue);

            _maxStackSizeProp.floatValue = newValue;
        }
        else
        {
            EditorGUILayout.PropertyField(_maxStackSizeProp, new GUIContent("Max Capacity [float]"));
        }

        GUI.enabled = true;
    }

    private void DrawUnitMeasurement(string typeName)
    {
        GUI.enabled = typeName switch
        {
            nameof(ToolItem) => false,
            nameof(ClothesItem) => false,
            _ => true,
        };

        EditorGUILayout.PropertyField(_unitMeasurementProp);

        GUI.enabled = true;
    }
}
