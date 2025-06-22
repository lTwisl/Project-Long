#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Универсальный инструмент для работы с изменениями объектов в Unity Editor
/// </summary>
public static class EditorChangeTracker
{
    /// <summary>
    /// Универсальная логика сохранения изменений для любого типа и контекста объекта
    /// </summary>
    /// <param name="obj">Объект, который необходимо сохранить</param>
    /// <param name="description">Описание изменения для системы Undo</param>
    /// <param name="immediateSave">Нужно ли сразу сохранять изменения на диск</param>
    public static void MarkAsDirty(Object obj, string description, bool immediateSave = false)
    {
        if (!obj)
        {
            Debug.LogWarning("EditorSaver: Попытка сохранить null объект");
            return;
        }

        // Заносим изменение в систему Undo
        UndoRegisterChange(obj, description);

        // Обрабатываем разные контексты объектов
        if (PrefabUtility.IsPartOfPrefabAsset(obj)) // Для обьектов, которые являются частями префабов в режиме редакторования префаба
        {
            HandlePrefabAsset(obj, immediateSave);
        }
        else if (PrefabUtility.IsPartOfPrefabInstance(obj)) // Для обьектов, которые являются частями префабов в экземпляре префаба сцены
        {
            HandlePrefabInstance(obj);
        }
        else // Для обьектов, которые находятся в сцене и не являются частями префабов
        {
            HandleSceneObject(obj);
        }

        // Принудительное обновление инспектора
        EditorApplication.RepaintHierarchyWindow();
        EditorApplication.RepaintProjectWindow();
    }

    /// <summary>
    /// Сохраняет несколько объектов одновременно
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

    #region Функциональные методы

    private static void UndoRegisterChange(Object obj, string description)
    {
        // Если это компонент
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

            // Для вложенных префабов
            if (PrefabUtility.IsPartOfVariantPrefab(root) || PrefabUtility.IsPartOfRegularPrefab(root))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(root);
            }
        }
    }

    private static void HandleSceneObject(Object obj)
    {
        EditorUtility.SetDirty(obj);

        // Для GameObject также помечаем сцену как изменённую
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