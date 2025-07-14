using UnityEditor;
using UnityEngine;

namespace Test
{
    

    /*[CustomEditor(typeof(InventoryManager))]
    public class InventoryManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            InventoryManager manager = (InventoryManager)target;

            // Стандартный инспектор для остальных полей
            DrawDefaultInspector();

            GUILayout.Space(10);

            // Область для Drag & Drop
            Rect dropArea = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Перетащите InventoryItem сюда", EditorStyles.helpBox);

            // Обработка события Drag & Drop
            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (currentEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            if (obj is InventoryItem item)
                            {
                                manager.inventorySlots.Add(new InventorySlot(item, 1));
                                EditorUtility.SetDirty(manager); // Сохранить изменения
                            }
                        }

                        currentEvent.Use();
                        GUI.changed = true;
                    }
                }
            }

            Repaint(); // Обновить инспектор
        }
    }*/
}


