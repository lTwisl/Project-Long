using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(TimeConversionAttribute))]
public class TimeConversionDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var attribute = this.attribute as TimeConversionAttribute;

        // Создаем контейнер
        var container = new VisualElement();

        // Создаем стандартное поле для float
        var floatField = new FloatField(property.displayName)
        {
            bindingPath = property.propertyPath
        };
        floatField.AddToClassList("unity-base-field__aligned");

        // Добавляем контекстное меню
        floatField.AddManipulator(new ContextualMenuManipulator((evt) =>
        {
            evt.menu.AppendAction(attribute.ConvertToMinutesLabel,
                _ => ConvertDaysToMinutes(property));
            evt.menu.AppendAction(attribute.ConvertToDaysLabel,
                _ => ConvertMinutesToDays(property));
        }));

        container.Add(floatField);
        return container;
    }

    private void ConvertDaysToMinutes(SerializedProperty property)
    {
        property.serializedObject.Update();
        property.floatValue *= 1440;
        property.serializedObject.ApplyModifiedProperties();
    }

    private void ConvertMinutesToDays(SerializedProperty property)
    {
        property.serializedObject.Update();
        property.floatValue /= 1440;
        property.serializedObject.ApplyModifiedProperties();
    }
}