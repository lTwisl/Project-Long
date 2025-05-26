//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(Storage))]
//public class StorageEditor : Editor
//{
//    private bool showInitSlots = true;
//    private bool showInventorySettings = true;

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        Storage storage = (Storage)target;

//        // --- Настройки инвентаря ---
//        showInventorySettings = EditorGUILayout.BeginFoldoutHeaderGroup(showInventorySettings, "Настройки Инвентаря");
//        if (showInventorySettings)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("Inventory"), true);
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("_storageId"), true);
//        }
//        EditorGUILayout.EndFoldoutHeaderGroup();

//        // --- Инициализационные предметы ---
//        showInitSlots = EditorGUILayout.BeginFoldoutHeaderGroup(showInitSlots, "Инициализационные Предметы");
//        if (showInitSlots)
//        {
//            EditorGUILayout.HelpBox("Добавьте сюда предметы, которые будут загружены при старте.", MessageType.Info);

//            // Показываем поле _initItems
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("_initItems"), true);

//            // Кнопка добавления предметов в InitSlots
//            if (GUILayout.Button("Добавить предметы в инвентарь"))
//            {
//                storage.ItemsToInventory();
//            }
//        }
//        EditorGUILayout.EndFoldoutHeaderGroup();

//        // --- Текущие слоты ---
//        if (storage.Inventory != null && storage.Inventory.Slots != null && storage.Inventory.Slots.Count > 0)
//        {
//            EditorGUILayout.LabelField("Текущие слоты (инвентарь):", EditorStyles.boldLabel);
//            foreach (var slot in storage.Inventory.Slots)
//            {
//                GUILayout.BeginHorizontal(EditorStyles.helpBox);
//                EditorGUILayout.LabelField($"{slot.Item?.Name ?? "Не задан"} x{slot.Capacity:F1} | Состояние: {slot.Condition:F0}%", GUILayout.ExpandWidth(true));
//                GUILayout.EndHorizontal();
//            }
//        }
//        else
//        {
//            EditorGUILayout.HelpBox("Инвентарь пуст или не инициализирован.", MessageType.Info);
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}