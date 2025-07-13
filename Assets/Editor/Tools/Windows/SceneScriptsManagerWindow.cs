using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneScriptsManagerWindow : EditorWindow
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
            groupName = label;
        }
    }
    // ГРУППЫ СКРИПТОВ:
    private ScriptGroup<Shelter> _shelters = new("Shelters");
    private ScriptGroup<ToxicityZone> _toxicityZones = new("Toxicity Zones");
    private ScriptGroup<Storage> _storages = new("Storages");


    [MenuItem("Tools/Open Scripts Manager Window")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneScriptsManagerWindow>();
        window.titleContent = new GUIContent("Scripts Manager Window");
        window.minSize = new Vector2(370, 300);
        window.maxSize = new Vector2(370, 2160);
    }

    private void OnEnable()
    {
        // Обновляем список доступных для контроля интерфейсов
        RefreshScriptsLists();

        // Подписываемся на событие загрузки сцены
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private void OnDisable()
    {
        // Отписываемся от события загрузки сцены
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        RefreshScriptsLists();
        if (this) Repaint();
    }

    private void OnHierarchyChanged()
    {
        // Обновляем списки с небольшой задержкой, чтобы избежать множественных обновлений
        EditorApplication.delayCall += () => {
            if (this) RefreshScriptsLists();
        };
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
        DrawGizmosWarning();
        DrawScriptGroups();
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            // ОБНОВИТЬ ВСЕ ГРУППЫ СКРИПТОВ:
            if (GUILayout.Button("Refresh Groups", EditorStyles.toolbarButton))
            {
                RefreshScriptsLists();
            }

            // СПРЯТАТЬ ВСЕ ГРУППЫ СКРИПТОВ:
            if (GUILayout.Button("Collapse Groups", EditorStyles.toolbarButton))
            {
                _shelters.showFoldout = false;
                _toxicityZones.showFoldout = false;
                _storages.showFoldout = false;
            }

            // ОТОБРАЗИТЬ ВСЕ ГРУППЫ СКРИПТОВ:
            if (GUILayout.Button("Expand Groups", EditorStyles.toolbarButton))
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
                "1. Click object name to select\n" +
                "2. [Focus] - frame object in Scene\n" +
                "3. [X] - delete object\n" +
                "4. Toggles control visualization",
                MessageType.Info);
    }

    // Новый метод для проверки и отображения предупреждения о Gizmos
    private void DrawGizmosWarning()
    {
        // Проверяем, включены ли Gizmos в сцене
        if (!SceneView.lastActiveSceneView?.drawGizmos ?? false)
        {
            EditorGUILayout.HelpBox(
                "Gizmos are currently disabled in the Scene view!\n" +
                "Some script visualizations may not be visible.\n" +
                "Please enable Gizmos in the Scene view toolbar.",
                MessageType.Warning);

            // Добавляем кнопку для быстрого включения Gizmos
            if (GUILayout.Button("Enable Gizmos In Scene", EditorStyles.miniButton))
            {
                ToggleGizmos();
            }
        }
        else
        {
            // Добавляем кнопку для быстрого выключения Gizmos
            if (GUILayout.Button("Disable Gizmos In Scene", EditorStyles.miniButton))
            {
                ToggleGizmos();
            }
        }
    }

    // Метод для переключения состояния Gizmos
    private void ToggleGizmos()
    {
        var sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            sceneView.drawGizmos = !sceneView.drawGizmos;
            sceneView.Repaint();
        }
    }

    private void DrawScriptGroups()
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
            bool currentToggleState = !anyHidden;

            // Создаем горизонтальную группу для лучшего выравнивания
            EditorGUILayout.BeginHorizontal();
            {
                // Делаем тоггл более заметным
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = mixedState;

                // Используем более широкий и заметный стиль для тоггла
                bool newToggleState = EditorGUILayout.Toggle(
                    new GUIContent("Show All Objects", "Toggle visibility for all items in this group"),
                    mixedState ? false : currentToggleState
                );

                EditorGUI.showMixedValue = false;

                if (EditorGUI.EndChangeCheck())
                {
                    bool newValue = !currentToggleState;
                    SetAllVisibility(group.scripts, newValue, $"Toggle All {group.groupName} Visibility");
                    group.showAll = newValue;
                }
            }
            EditorGUILayout.EndHorizontal();

            // Рисуем список элементов с увеличенным отступом
            EditorGUI.indentLevel++;
            DrawElementsList(group);
            EditorGUI.indentLevel--;

            // Добавляем разделительную линию между тогглом группы и элементами
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(5); // Уменьшаем промежуток между группами
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
                // Подсветка выбранного объекта
                var isSelected = Selection.activeObject == element.gameObject;
                var bgColor = GUI.backgroundColor;
                if (isSelected) GUI.backgroundColor = Color.cyan;

                // Кнопка пинга обьекта с его именем
                SelectObjectButton(element.gameObject);

                // Кнопка фокуса на объекте в окне сцены
                if (GUILayout.Button("Focus", EditorStyles.miniButtonLeft, GUILayout.Width(50)))
                {
                    EditorApplication.delayCall += () =>
                    {
                        Selection.activeObject = element.gameObject;
                        SceneView.lastActiveSceneView.FrameSelected();
                    };
                }

                // Кнопка удаления объекта со сцены
                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Object", $"Delete ({element.name})?", "Yes", "No"))
                    {
                        Undo.DestroyObjectImmediate(element.gameObject);
                        RefreshScriptsLists();
                    }
                }

                // Тоггл управления видимостью обьекта
                EditorGUI.BeginChangeCheck();
                bool newValue = EditorGUILayout.Toggle(element.ShowScriptInfo);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(element, $"Toggle {group.groupName} Visibility");
                    element.ShowScriptInfo = newValue;
                    EditorUtility.SetDirty(element);
                }

                GUI.backgroundColor = bgColor;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void SelectObjectButton(GameObject obj)
    {
        var style = new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleLeft,
            fixedWidth = 250
        };

        if (GUILayout.Button(obj.name, style))
        {
            EditorApplication.delayCall += () =>
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            };
        }
    }
}