using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InventorySlot))]
public class InventorySlotDrawer : PropertyDrawer
{
    private readonly float _iconSize = EditorGUIUtility.singleLineHeight * 4;

    private static Dictionary<string, bool> _foldoutStates = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // ���������� ���������� ���� ��� ������� ��������
        string key = $"{property.propertyPath}";
        if (!_foldoutStates.ContainsKey(key)) _foldoutStates[key] = false;

        // Foldout ���������
        Rect foldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        _foldoutStates[key] = EditorGUI.Foldout(foldRect, _foldoutStates[key], GetHeaderLabel(property), true);

        EditorGUI.indentLevel++;

        if (_foldoutStates[key])
        {
            SerializedProperty itemProp = property.FindPropertyRelative("<Item>k__BackingField");
            SerializedProperty capacityProp = property.FindPropertyRelative("_capacity");
            SerializedProperty conditionProp = property.FindPropertyRelative("_condition");

            // ������ ��� �����������
            float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // ���� Item

            Texture textureIcon = (itemProp.objectReferenceValue as InventoryItem)?.Icon.texture;
            if (textureIcon != null)
            {
                Rect iconRect = new Rect(position.x, y, _iconSize, _iconSize);
                EditorGUI.DrawTextureTransparent(iconRect, textureIcon);
            }

            Rect itemRect = new Rect(position.x + _iconSize, y, position.width - _iconSize, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(itemRect, itemProp);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // ���� Count
            Rect capacityRect = new Rect(position.x + _iconSize, y, position.width - _iconSize, EditorGUIUtility.singleLineHeight);
            DrawCapacityField(itemProp, capacityProp, capacityRect);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (Mathf.Approximately(capacityProp.floatValue, 0))
            {
                if (itemProp.objectReferenceValue is InventoryItem item)
                    capacityProp.floatValue = item.MaxCapacity;
            }

            // ���� Condition
            Rect conditionRect = new Rect(position.x + _iconSize, y, position.width - _iconSize, EditorGUIUtility.singleLineHeight);
            EditorGUI.Slider(conditionRect, conditionProp, 0.001f, 1f, "Condition");
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (Mathf.Approximately(conditionProp.floatValue, 0))
                conditionProp.floatValue = 1;

            // Weight
            Rect weightRect = new Rect(position.x + _iconSize, y, position.width - _iconSize, EditorGUIUtility.singleLineHeight);
            DrawWeightField(itemProp, capacityProp, weightRect);
        }
        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string key = $"{property.propertyPath}";
        if (!_foldoutStates.TryGetValue(key, out bool isExpanded)) isExpanded = false;

        return isExpanded
            ? 5 * EditorGUIUtility.singleLineHeight + 4 * EditorGUIUtility.standardVerticalSpacing
            : EditorGUIUtility.singleLineHeight;
    }

    private GUIContent GetHeaderLabel(SerializedProperty property)
    {
        var itemProp = property.FindPropertyRelative("<Item>k__BackingField");
        string capacity = property.FindPropertyRelative("_capacity").floatValue.ToString("0.##");
        string condition = (property.FindPropertyRelative("_condition").floatValue * 100).ToString("0.###");
        string itemName = itemProp.objectReferenceValue?.name ?? "Empty";
        return new GUIContent($"{itemName} ({capacity}) ({condition}%)");
    }

    private void DrawCapacityField(SerializedProperty itemProp, SerializedProperty capacityProp, Rect rect)
    {
        if (itemProp.objectReferenceValue == null)
        {
            EditorGUI.PropertyField(rect, capacityProp);
            return;
        }

        SerializedObject item = new SerializedObject(itemProp.objectReferenceValue);

        if (item.targetObject is ClothingItem || item.targetObject is ToolItem)
        {
            GUI.enabled = false;
            capacityProp.floatValue = 1f;
            EditorGUI.PropertyField(rect, capacityProp);
            GUI.enabled = true;
            return;
        }

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

    private void DrawWeightField(SerializedProperty itemProp, SerializedProperty countProp, Rect rect)
    {
        if (itemProp.objectReferenceValue == null)
        {
            EditorGUI.LabelField(rect, "Weight", "N/A");
            return;
        }

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
}
