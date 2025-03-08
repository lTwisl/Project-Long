using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(InventorySlot))]
public class InventorySlotDrawer : PropertyDrawer
{
    private static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Генерируем уникальный ключ для каждого элемента
        string key = $"{property.propertyPath}";
        if (!foldoutStates.ContainsKey(key)) foldoutStates[key] = false;

        // Foldout заголовок
        Rect foldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        foldoutStates[key] = EditorGUI.Foldout(foldRect, foldoutStates[key], GetHeaderLabel(property), true);

        EditorGUI.indentLevel++;

        if (foldoutStates[key])
        {
            SerializedProperty itemProp = property.FindPropertyRelative("<Item>k__BackingField");
            SerializedProperty capacityProp = property.FindPropertyRelative("_capacity");
            SerializedProperty conditionProp = property.FindPropertyRelative("_condition");

            // Отступ для содержимого
            float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Поле Item
            Rect itemRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(itemRect, itemProp);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Поле Count
            Rect capacityRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            DrawCapacityField(itemProp, capacityProp, capacityRect);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Поле Condition
            Rect conditionRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.Slider(conditionRect, conditionProp, 0.001f, 100f, "Condition");
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Weight
            Rect weightRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            DrawWeightField(itemProp, capacityProp, weightRect);
        }
        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string key = $"{property.propertyPath}";
        if (!foldoutStates.TryGetValue(key, out bool isExpanded)) isExpanded = false;

        return isExpanded
            ? 5 * EditorGUIUtility.singleLineHeight + 4 * EditorGUIUtility.standardVerticalSpacing
            : EditorGUIUtility.singleLineHeight;
    }

    private GUIContent GetHeaderLabel(SerializedProperty property)
    {
        var itemProp = property.FindPropertyRelative("<Item>k__BackingField");
        string capacity = property.FindPropertyRelative("_capacity").floatValue.ToString("0.##");
        string condition = property.FindPropertyRelative("_condition").floatValue.ToString("0.###");
        string itemName = itemProp.objectReferenceValue?.name ?? "Empty";
        return new GUIContent($"{itemName} ({capacity}) ({condition}%)");
    }

    private void DrawCapacityField(SerializedProperty itemProp, SerializedProperty capacityProp, Rect rect)
    {
        if (itemProp.objectReferenceValue != null)
        {
            SerializedObject item = new SerializedObject(itemProp.objectReferenceValue);
            bool useInt = item.FindProperty("<MeasuredAsInteger>k__BackingField").boolValue;
            float maxCapacity = item.FindProperty("<MaxCapacity>k__BackingField").floatValue;

            if (useInt)
            {
                capacityProp.floatValue = EditorGUI.Slider(
                    rect,
                    "Capacity",
                    (int)capacityProp.floatValue,
                    1,
                    (int)maxCapacity
                );
            }
            else
            {
                capacityProp.floatValue = EditorGUI.Slider(
                    rect,
                    "Capacity",
                    capacityProp.floatValue,
                    0.001f,
                    maxCapacity
                );
            }
        }
        else
        {
            EditorGUI.PropertyField(rect, capacityProp);
        }
    }

    private void DrawWeightField(SerializedProperty itemProp, SerializedProperty countProp, Rect rect)
    {
        if (itemProp.objectReferenceValue != null)
        {
            var item = new SerializedObject(itemProp.objectReferenceValue);
            SerializedProperty weightProp = item.FindProperty("<Weight>k__BackingField");

            if (weightProp != null)
            {
                float totalWeight = weightProp.floatValue * countProp.floatValue;
                EditorGUI.LabelField(rect, "Total Weight", $"{totalWeight:0.##} kg");
            }
            else
            {
                EditorGUI.LabelField(rect, "Weight", "N/A");
            }
        }
        else
        {
            EditorGUI.LabelField(rect, "Weight", "0 kg");
        }
    }
}
