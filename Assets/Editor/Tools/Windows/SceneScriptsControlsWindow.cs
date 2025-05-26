using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneScriptsControlsWindow : EditorWindow
{
    private Vector2 _scrollPosition;

    [Serializable]
    private class ScriptGroup<T> where T : MonoBehaviour
    {
        public List<T> scripts = new();
        public bool showFoldout = true;
        public bool showAll = true;
        public string groupName;

        public ScriptGroup(string label)
        {
            this.groupName = label;
        }
    }

    // ГРУППЫ СКРИПТОВ:
    private ScriptGroup<Shelter> _shelters = new("Shelters");
    private ScriptGroup<ToxicityZone> _toxicityZones = new("Toxicity Zones");
    private ScriptGroup<Storage> _storages = new("Storages");


    [MenuItem("Tools/Open Scene Scripts Controls Window")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneScriptsControlsWindow>();
        window.titleContent = new GUIContent("Scene Scripts Controls Window");
        window.minSize = new Vector2(350, 300);
        window.maxSize = new Vector2(350, 1440);
    }

    private void OnEnable()
    {
        // Подписываемся на событие загрузки сцены
        EditorSceneManager.sceneOpened += OnSceneOpened;
        RefreshScriptsLists();
    }

    private void OnDisable()
    {
        // Отписываемся от события загрузки сцены
        EditorSceneManager.sceneOpened -= OnSceneOpened;
    }

    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        RefreshScriptsLists();

        // Если окно открыто - перерисовываем его
        if (this != null) Repaint();
    }

    private void RefreshScriptsLists()
    {
        // ПОИСК ВСЕХ СКРИПТОВ В СЦЕНЕ:
        _shelters.scripts = FindObjectsByType<Shelter>(FindObjectsSortMode.None).ToList();
        _toxicityZones.scripts = FindObjectsByType<ToxicityZone>(FindObjectsSortMode.None).ToList();
        _storages.scripts = FindObjectsByType<Storage>(FindObjectsSortMode.None).ToList();

        EditorUtility.DisplayProgressBar("Refreshing", "Updating script references...", 1f);
        EditorUtility.ClearProgressBar();
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawHelpBox();
        DrawControls();
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton)) RefreshScriptsLists();
            GUILayout.FlexibleSpace();

            // СПРЯТАТЬ ВСЕ ГРУППЫ СКРИПТОВ:
            if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton))
            {
                _shelters.showFoldout = false;
                _toxicityZones.showFoldout = false;
                _storages.showFoldout = false;
            }
            // ОТОБРАЗИТЬ ВСЕ ГРУППЫ СКРИПТОВ:
            if (GUILayout.Button("Expand All", EditorStyles.toolbarButton))
            {
                _shelters.showFoldout = true;
                _toxicityZones.showFoldout = true;
                _storages.showFoldout = true;
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawHelpBox()
    {
        EditorGUILayout.HelpBox(
            "1. Click on object name to select it in hierarchy\n" +
            "2. Use toggles to control script visualization",
            MessageType.Info);
    }

    private void DrawControls()
    {
        // РИСУЕМ ВСЕ ГРУППЫ СКРИПТОВ:
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        {
            DrawScriptGroup(_shelters);
            DrawScriptGroup(_toxicityZones);
            DrawScriptGroup(_storages);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawScriptGroup<T>(ScriptGroup<T> group) where T : MonoBehaviour, IShowable
    {
        // Делаем раскрывающийся список
        group.showFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(group.showFoldout, $"{group.groupName} ({group.scripts.Count})");

        if (group.showFoldout)
        {
            var (anyShown, anyHidden) = CalculateVisibilityStates(group.scripts);
            bool mixedState = anyShown && anyHidden;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = mixedState;
            group.showAll = EditorGUILayout.Toggle("Show All", mixedState ? false : group.showAll);
            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                bool newValue = !(anyShown && !anyHidden);
                SetAllVisibility(group.scripts, newValue, $"Toggle All {group.groupName} Visibility");
                group.showAll = newValue;
            }

            EditorGUI.indentLevel++;
            DrawElementsList(group);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(10);
    }

    private (bool anyShown, bool anyHidden) CalculateVisibilityStates<T>(List<T> elements) where T : IShowable
    {
        bool anyShown = false, anyHidden = false;
        foreach (var element in elements.Where(e => e != null))
        {
            if (element.ShowScriptInfo) anyShown = true;
            else anyHidden = true;
        }
        return (anyShown, anyHidden);
    }

    private void SetAllVisibility<T>(List<T> elements, bool value, string undoMessage) where T : IShowable
    {
        foreach (var element in elements.Where(e => e != null))
        {
            Undo.RecordObject(element as UnityEngine.Object, undoMessage);
            element.ShowScriptInfo = value;
            EditorUtility.SetDirty(element as UnityEngine.Object);
        }
    }

    private void DrawElementsList<T>(ScriptGroup<T> group) where T : MonoBehaviour, IShowable
    {
        foreach (var element in group.scripts)
        {
            if (element == null) continue;

            EditorGUILayout.BeginHorizontal();
            {
                SelectObjectButton(element.gameObject);

                EditorGUI.BeginChangeCheck();
                bool newValue = EditorGUILayout.Toggle(element.ShowScriptInfo);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(element, $"Toggle {group.groupName} Visibility");
                    element.ShowScriptInfo = newValue;
                    EditorUtility.SetDirty(element);
                    group.showAll = group.scripts.All(e => e != null && e.ShowScriptInfo);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void SelectObjectButton(GameObject obj)
    {
        var style = new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleLeft,
            fixedWidth = 300
        };

        if (GUILayout.Button(obj.name, style))
        {
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }
}