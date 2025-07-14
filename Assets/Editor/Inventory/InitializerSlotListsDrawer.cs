using UnityEditor;
using UnityEngine;


public abstract class InitListDrawer<SelfType, ByType> : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Сохраняем исходную позицию
        Rect originalPosition = position;

        // Прозрачная кнопка над заголовком для Drag & Drop
        Rect dragRect = new Rect(
            originalPosition.x,
            originalPosition.y,
            originalPosition.width,
            EditorGUIUtility.singleLineHeight
        );

        // Обработка Drag & Drop на заголовке
        HandleDragAndDrop(dragRect, property);

        // Смещаем остальные элементы ниже
        float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        EditorGUI.PropertyField(dragRect, property.FindPropertyRelative("Items"), label);

        EditorGUI.EndProperty();
    }

    private void HandleDragAndDrop(Rect dragRect, SerializedProperty property)
    {
        Event currentEvent = Event.current;

        GUI.Box(dragRect, $"Перетащите {typeof(ByType).Name} сюда");

        if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
        {
            if (dragRect.Contains(currentEvent.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (currentEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object obj in DragAndDrop.objectReferences)
                    {
                        if (obj is ByType item)
                        {
                            // Получаем целевой объект InitSlotList
                            var targetList = fieldInfo.GetValue(property.serializedObject.targetObject) as InitializerList<SelfType, ByType>;
                            if (targetList != null)
                            {
                                targetList.Items.Add(CreateObject(item));
                                EditorUtility.SetDirty(property.serializedObject.targetObject);
                            }
                        }
                    }

                    currentEvent.Use();
                    GUI.changed = true;
                }
            }
        }
    }

    public abstract SelfType CreateObject(ByType item);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Items"));
        return height;
    }
}

[CustomPropertyDrawer(typeof(InitializerListSlots))]
public class InitSlotListDrawer : InitListDrawer<InventorySlot, InventoryItem>
{
    public override InventorySlot CreateObject(InventoryItem item)
    {
        return new InventorySlot(item, 1, 1);
    }
}

[CustomPropertyDrawer(typeof(InitializerListRandSlots))]
public class InitRandSlotListDrawer : InitListDrawer<RandItem, InventoryItem>
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);

        var items = property.FindPropertyRelative("Items");

        for (int i = 0; i < items.arraySize; ++i)
        {
            var item = items.GetArrayElementAtIndex(i);
            var v2 = item.FindPropertyRelative("<MinMaxCapacity>k__BackingField");
            var it = item.FindPropertyRelative("<Item>k__BackingField");

            if (it.boxedValue == null)
                continue;

            float max = Mathf.Clamp(v2.vector2Value.y, 0, (it.boxedValue as InventoryItem).MaxCapacity);
            float min = Mathf.Clamp(v2.vector2Value.x, 0, max);

            v2.vector2Value = new Vector2(min, max);

            var v3 = item.FindPropertyRelative("<MinMaxCondition>k__BackingField");

            min = Mathf.Clamp(v3.vector2Value.x, 0, Mathf.Min(1, v3.vector2Value.y));
            max = Mathf.Clamp(v3.vector2Value.y, min, 1);

            v3.vector2Value = new Vector2(min, max);


        }
    }

    public override RandItem CreateObject(InventoryItem item)
    {
        return new RandItem(1, item, new Vector2(0, item.MaxCapacity), new Vector2(0, 1));
    }
}
