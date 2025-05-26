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

//        // --- ��������� ��������� ---
//        showInventorySettings = EditorGUILayout.BeginFoldoutHeaderGroup(showInventorySettings, "��������� ���������");
//        if (showInventorySettings)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("Inventory"), true);
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("_storageId"), true);
//        }
//        EditorGUILayout.EndFoldoutHeaderGroup();

//        // --- ����������������� �������� ---
//        showInitSlots = EditorGUILayout.BeginFoldoutHeaderGroup(showInitSlots, "����������������� ��������");
//        if (showInitSlots)
//        {
//            EditorGUILayout.HelpBox("�������� ���� ��������, ������� ����� ��������� ��� ������.", MessageType.Info);

//            // ���������� ���� _initItems
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("_initItems"), true);

//            // ������ ���������� ��������� � InitSlots
//            if (GUILayout.Button("�������� �������� � ���������"))
//            {
//                storage.ItemsToInventory();
//            }
//        }
//        EditorGUILayout.EndFoldoutHeaderGroup();

//        // --- ������� ����� ---
//        if (storage.Inventory != null && storage.Inventory.Slots != null && storage.Inventory.Slots.Count > 0)
//        {
//            EditorGUILayout.LabelField("������� ����� (���������):", EditorStyles.boldLabel);
//            foreach (var slot in storage.Inventory.Slots)
//            {
//                GUILayout.BeginHorizontal(EditorStyles.helpBox);
//                EditorGUILayout.LabelField($"{slot.Item?.Name ?? "�� �����"} x{slot.Capacity:F1} | ���������: {slot.Condition:F0}%", GUILayout.ExpandWidth(true));
//                GUILayout.EndHorizontal();
//            }
//        }
//        else
//        {
//            EditorGUILayout.HelpBox("��������� ���� ��� �� ���������������.", MessageType.Info);
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}