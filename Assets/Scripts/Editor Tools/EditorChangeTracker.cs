#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// ������������� ���������� ��� ������ � ����������� �������� � Unity Editor
/// </summary>
public static class EditorChangeTracker
{
    /// <summary>
    /// ������������� ������ ���������� ��������� ��� ������ ���� � ��������� �������
    /// </summary>
    /// <param name="obj">������, ������� ���������� ���������</param>
    /// <param name="description">�������� ��������� ��� ������� Undo</param>
    /// <param name="immediateSave">����� �� ����� ��������� ��������� �� ����</param>
    public static void MarkAsDirty(Object obj, string description, bool immediateSave = false)
    {
        if (!obj)
        {
            Debug.LogWarning("EditorSaver: ������� ��������� null ������");
            return;
        }

        // ������� ��������� � ������� Undo
        UndoRegisterChange(obj, description);

        // ������������ ������ ��������� ��������
        if (PrefabUtility.IsPartOfPrefabAsset(obj)) // ��� ��������, ������� �������� ������� �������� � ������ �������������� �������
        {
            HandlePrefabAsset(obj, immediateSave);
        }
        else if (PrefabUtility.IsPartOfPrefabInstance(obj)) // ��� ��������, ������� �������� ������� �������� � ���������� ������� �����
        {
            HandlePrefabInstance(obj);
        }
        else // ��� ��������, ������� ��������� � ����� � �� �������� ������� ��������
        {
            HandleSceneObject(obj);
        }

        // �������������� ���������� ����������
        EditorApplication.RepaintHierarchyWindow();
        EditorApplication.RepaintProjectWindow();
    }

    /// <summary>
    /// ��������� ��������� �������� ������������
    /// </summary>
    public static void MarkMultipleAsDirty(IEnumerable<Object> objects, string description, bool immediateSave = false)
    {
        if (objects == null) return;

        Undo.SetCurrentGroupName(description);
        var group = Undo.GetCurrentGroup();

        foreach (var obj in objects)
        {
            MarkAsDirty(obj, description, immediateSave);
        }

        Undo.CollapseUndoOperations(group);
    }

    #region �������������� ������

    private static void UndoRegisterChange(Object obj, string description)
    {
        // ���� ��� ���������
        if (obj is Component component)
        {
            Undo.RecordObject(component.gameObject, description);
        }
        else
        {
            Undo.RecordObject(obj, description);
        }
    }

    private static void HandlePrefabAsset(Object obj, bool immediateSave)
    {
        EditorUtility.SetDirty(obj);
        if (immediateSave)
        {
            AssetDatabase.SaveAssetIfDirty(obj);
        }
    }

    private static void HandlePrefabInstance(Object obj)
    {
        var root = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
        if (root)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
            EditorUtility.SetDirty(root);

            // ��� ��������� ��������
            if (PrefabUtility.IsPartOfVariantPrefab(root) || PrefabUtility.IsPartOfRegularPrefab(root))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(root);
            }
        }
    }

    private static void HandleSceneObject(Object obj)
    {
        EditorUtility.SetDirty(obj);

        // ��� GameObject ����� �������� ����� ��� ���������
        if (obj is GameObject go && !EditorApplication.isPlaying)
        {
            var scene = go.scene;
            if (scene.IsValid() && scene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }
    }

    #endregion
}
#endif