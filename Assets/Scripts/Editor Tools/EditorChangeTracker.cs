#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EditorChangeTracker
{
    #region SET DIRTY|UNDO METHODS
    /// <summary>
    /// ����������� Undo: ��� ������ �������
    /// </summary>
    public static void RegisterUndo(Object target, string undoMessage = "Editor Change Tracker Undo")
    {
        if (target == null) return;

        Undo.RecordObject(target, undoMessage);
    }

    /// <summary>
    /// ����� �������� �������: ��� ������ �������
    /// </summary>
    public static void SetDirty(Object target)
    {
        if (target == null) return;

        // Prefab-Instance
        if (IsPrefabInstance(target))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            return;
        }

        // Prefab-Asset, SO, ... 
        if (EditorUtility.IsPersistent(target))
        {
            EditorUtility.SetDirty(target);
            return;
        }

        // Object, Component, other in Scene... 
        Scene scene = GetSceneFromTarget(target);
        if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);
    }
    #endregion

    #region UTILITY METHODS
    private static Scene GetSceneFromTarget(Object target)
    {
        if (target is GameObject go) return go.scene;
        if (target is Component comp) return comp.gameObject.scene;
        return default;
    }
    #endregion

    #region OBJECT CONTEXT
    /// <summary>
    /// ��������: �������� �� ������ �������� � Project View?
    /// </summary>
    public static bool IsPrefabAsset(Object target)
    {
        if (target == null) return false;

        return PrefabUtility.IsPartOfPrefabAsset(target) && !PrefabUtility.IsPartOfNonAssetPrefabInstance(target);
    }

    /// <summary>
    /// ��������: �������� �� ������ ����������� ������� � �����?
    /// </summary>
    public static bool IsPrefabInstance(Object target)
    {
        if (target == null) return false;

        GameObject gameObject = target switch
        {
            Component component => component.gameObject,
            GameObject gameObj => gameObj,
            _ => null
        };

        return PrefabUtility.IsPartOfPrefabInstance(gameObject) && PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null && !PrefabUtility.IsPrefabAssetMissing(gameObject);
    }

    /// <summary>
    /// ��������: �������� �� ������ ������ �������������� �������?
    /// </summary>
    public static bool IsPrefabRoot(Object target)
    {
        if (target == null) return false;

        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        return stage != null && stage.prefabContentsRoot == target;
    }

    /// <summary>
    /// ��������: ��������� �� ������ ������ �������������� �������?
    /// </summary>
    public static bool IsPrefabInEditMode(Object target)
    {
        if (target == null) return false;

        if (target is Component c) target = c.gameObject;
        if (target is GameObject go)
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            return stage != null && stage.IsPartOfPrefabContents(go);
        }
        return false;
    }

    /// <summary>
    /// ��������: �������� �� ������ ������ ����� (�� ��������)
    /// </summary>
    public static bool IsSceneObject(Object target)
    {
        if (target == null) return false;

        // �������� �������������� �����
        bool inValidScene = target switch
        {
            GameObject go => go.scene.IsValid(),
            Component c => c.gameObject.scene.IsValid(),
            _ => false
        };

        return inValidScene && !PrefabUtility.IsPartOfPrefabAsset(target) && !PrefabUtility.IsPartOfPrefabInstance(target) && !IsPrefabInEditMode(target);
    }

    /// <summary>
    /// ��������: �������� �� ������ ����������� �� ������� ������� �����?
    /// </summary>
    public static bool IsPrefabAssetComponent(Object target)
    {
        if (target == null) return false;

        return target is Component component && (IsPrefabAsset(component) || IsPrefabInEditMode(component));
    }

    /// <summary>
    /// ��������: �������� �� ������ ����������� �� ������� ������� �����?
    /// </summary>
    public static bool IsSceneComponent(Object target)
    {
        if (target == null) return false;

        return target is Component component && component.gameObject.scene.IsValid() && IsSceneObject(component);
    }


    /// <summary>
    /// �������� ��� ��������� ������� � ���� ������
    /// </summary>
    public static void GetContextInfo(Object target)
    {
        if (target == null) Debug.Log("NULL");

        if (IsPrefabAsset(target)) Debug.Log($"Object (<color=cyan>{target.name}</color>) is Prefab Asset");
        if (IsPrefabInstance(target)) Debug.Log($"Object (<color=cyan>{target.name}</color>) is Prefab Instance");
        if (IsPrefabInEditMode(target)) Debug.Log($"Object (<color=cyan>{target.name}</color>) is Prefab In Edit Mode");

        if (IsPrefabRoot(target)) Debug.Log($"Object (<color=cyan>{target.name}</color>) is Prefab Root");

        if (IsSceneObject(target)) Debug.Log($"Object (<color=cyan>{target.name}</color>) is Scene Object");

        if (IsPrefabAssetComponent(target)) Debug.Log($"Object (<color=cyan>{target.name}</color>) is Prefab Asset Component");
        if (IsSceneComponent(target)) Debug.Log($"Object (<color=cyan>{target.name}</color>) is Scene Component");
    }
    #endregion
}
#endif