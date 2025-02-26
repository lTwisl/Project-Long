using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (ShouldHide(property))
            return;

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return ShouldHide(property)
            ? -EditorGUIUtility.standardVerticalSpacing
            : EditorGUI.GetPropertyHeight(property, label, true);
    }

    private bool ShouldHide(SerializedProperty property)
    {
        HideIfAttribute hideAttribute = (HideIfAttribute)attribute;
        SerializedProperty conditionProperty = FindConditionProperty(property, hideAttribute.ConditionPath);

        if (conditionProperty is null)
        {
            conditionProperty = FindConditionProperty(property, $"<{hideAttribute.ConditionPath}>k__BackingField");

            if (conditionProperty is null)
                return false;
        }

        return CompareValues(conditionProperty, hideAttribute.ComparisonValue, hideAttribute.Invert);
    }

    private SerializedProperty FindConditionProperty(SerializedProperty property, string conditionPath)
    {
        string parentPath = property.propertyPath.Contains(".")
            ? property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.'))
            : "";

        string fullPath = string.IsNullOrEmpty(parentPath)
            ? conditionPath
            : $"{parentPath}.{conditionPath}";

        return property.serializedObject.FindProperty(fullPath);
    }

    private bool CompareValues(SerializedProperty property, object b, bool invert)
    {
        bool rezult = property.propertyType switch
        {
            SerializedPropertyType.Integer => property.intValue == (int)b,
            SerializedPropertyType.Boolean => property.boolValue == (bool)b,
            SerializedPropertyType.Float => Mathf.Approximately(property.floatValue, (float)b),
            SerializedPropertyType.String => property.stringValue == (string)b,
            SerializedPropertyType.Enum => property.enumValueIndex == (int)b,
            _ => false,
        };

        return invert ? !rezult : rezult;
    }
}
