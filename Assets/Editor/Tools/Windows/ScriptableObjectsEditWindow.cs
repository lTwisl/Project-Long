using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectsEditWindow : EditorWindow
{
    private const float CONTROL_PANEL_WIDTH = 300f;
    private const float MIN_PANEL_WIDTH = 200f;
    private const float MAX_PANEL_WIDTH = 600f;

    private enum SortMode { Name, Type, Date }
    private enum LoadMode { Replace, Add }

    private List<ScriptableObject> _SOPool = new();
    private List<ScriptableObject> _deletePool = new();
    private ScriptableObjectsWindowPreset _presetSO;
    private TextAsset _jsonPreset;
    private LoadMode _loadMode = LoadMode.Replace;

    private Dictionary<ScriptableObject, bool> _foldoutStates = new();
    private Dictionary<ScriptableObject, Editor> _editorCache = new();
    private Dictionary<ScriptableObject, Vector2> _panelScrollPositions = new();

    private Vector2 _horizontalScrollPosition;
    private Vector2 _verticalScrollPosition;
    private float _panelWidth = 450f;

    private SortMode _currentSortMode;
    private bool _needsSort;
    private bool _useDragAndDrop = true;

    private GUIStyle _headerStyle;
    private GUIStyle _closeButtonStyle;
    private GUIStyle _visibleStyle;
    private GUIStyle _hideStyle;
    private GUIStyle _descriptionStyle;

    [MenuItem("Tools/Open SO Edit Window")]
    public static void ShowWindow()
    {
        GetWindow<ScriptableObjectsEditWindow>("SO Edit Window");
    }

    private void OnEnable()
    {
        InitializeStyles();
    }

    private void InitializeStyles()
    {
        _headerStyle = new GUIStyle(EditorStyles.largeLabel)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.cyan },
            alignment = TextAnchor.MiddleCenter
        };

        _closeButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            fontSize = 14,
            normal = { textColor = Color.red }
        };

        _visibleStyle = new GUIStyle(EditorStyles.miniButton)
        {
            fontSize = 12,
            normal = { textColor = Color.cyan },
            alignment = TextAnchor.MiddleLeft
        };

        _hideStyle = new GUIStyle(EditorStyles.miniButton)
        {
            fontSize = 12,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f, 1) },
            alignment = TextAnchor.MiddleLeft
        };

        _descriptionStyle = new GUIStyle(EditorStyles.helpBox)
        {
            wordWrap = true,
            fontSize = 11,
            padding = new RectOffset(8, 8, 8, 8)
        };
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawDragAndDrop();
        SortObjects();
        DrawObjectsPanel();
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            GUILayout.Label("Sorting by:");

            if (GUILayout.Toggle(_currentSortMode == SortMode.Name, "Name", EditorStyles.toolbarButton))
                SetSortMode(SortMode.Name);

            if (GUILayout.Toggle(_currentSortMode == SortMode.Type, "Type", EditorStyles.toolbarButton))
                SetSortMode(SortMode.Type);

            if (GUILayout.Toggle(_currentSortMode == SortMode.Date, "Date", EditorStyles.toolbarButton))
                SetSortMode(SortMode.Date);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Selected In Project", EditorStyles.toolbarButton))
                AddSelectedObjects();

            if (GUILayout.Button("Clear All Tracked", EditorStyles.toolbarButton))
                ClearWindow();
        }
        GUILayout.EndHorizontal();
    }

    private void DrawDragAndDrop()
    {
        if (!_useDragAndDrop) return;

        if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
        {
            bool hasValidObjects = false;
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is ScriptableObject)
                {
                    hasValidObjects = true;
                    break;
                }
            }

            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = hasValidObjects ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.DragPerform && hasValidObjects)
            {
                DragAndDrop.AcceptDrag();
                foreach (var obj in DragAndDrop.objectReferences)
                    TryAddScriptableObject(obj as ScriptableObject);

                _needsSort = true;
                Event.current.Use();
            }
        }
    }

    private void SortObjects()
    {
        if (!_needsSort) return;

        switch (_currentSortMode)
        {
            case SortMode.Name: _SOPool.Sort((a, b) => a.name.CompareTo(b.name)); break;
            case SortMode.Type: _SOPool.Sort((a, b) => a.GetType().Name.CompareTo(b.GetType().Name)); break;
            case SortMode.Date: _SOPool.Sort((a, b) => File.GetLastWriteTime(AssetDatabase.GetAssetPath(a)).CompareTo(File.GetLastWriteTime(AssetDatabase.GetAssetPath(b)))); break;
        }
        _needsSort = false;
    }

    private void DrawObjectsPanel()
    {
        ValidateScriptableObjectsPool();
        DeferredDeletion();

        if (_SOPool.Count == 0)
        {
            EditorGUILayout.HelpBox("The tracked ScriptableObjects are not selected.\n" + "Use (Drag and Drop) or (Add Selected In Project) to add SO to the window.", MessageType.Warning);
            if (_useDragAndDrop != true) _useDragAndDrop = true;
        }

        GUILayout.BeginHorizontal();
        {
            DrawMainContentPanel();
            DrawControlSidePanel();
        }
        GUILayout.EndHorizontal();
    }

    private void DrawMainContentPanel()
    {
        GUILayout.BeginVertical(GUILayout.Width(position.width - CONTROL_PANEL_WIDTH));
        _horizontalScrollPosition = EditorGUILayout.BeginScrollView(_horizontalScrollPosition, GUILayout.Height(position.height - EditorGUIUtility.singleLineHeight * 1.5f));

        GUILayout.BeginHorizontal();
        foreach (var so in _SOPool)
        {
            if (!IsValid(so)) continue;
            if (_foldoutStates.GetValueOrDefault(so))
                DrawExpandedPanel(so);
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawControlSidePanel()
    {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(CONTROL_PANEL_WIDTH));
        _verticalScrollPosition = EditorGUILayout.BeginScrollView(_verticalScrollPosition);

        EditorGUILayout.LabelField("Editing SO Preset:", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        {
            _presetSO = (ScriptableObjectsWindowPreset)EditorGUILayout.ObjectField(_presetSO, typeof(ScriptableObjectsWindowPreset), false);
            if (GUILayout.Button("Load", GUILayout.Width(60)))
            {
                if (_presetSO != null)
                {
                    LoadPresetFromSO(_presetSO);
                }
            }
        }
        GUILayout.EndHorizontal();

        if (_presetSO)
        {
            EditorGUILayout.TextArea($"Selected preset contains: {_presetSO.GetSoCount} Scriptable Objects.", _descriptionStyle);
            if (!string.IsNullOrEmpty(_presetSO.Description))
                EditorGUILayout.TextArea(_presetSO.Description, _descriptionStyle);
        }

        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Toggle(_loadMode == LoadMode.Replace, "Replace", EditorStyles.miniButtonLeft))
                _loadMode = LoadMode.Replace;

            if (GUILayout.Toggle(_loadMode == LoadMode.Add, "Add", EditorStyles.miniButtonRight))
                _loadMode = LoadMode.Add;
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Drag And Drop Mode:", EditorStyles.boldLabel);
        string buttonDragText = _useDragAndDrop ? "Scriptable Objects" : "Propertys / References";
        if (GUILayout.Button(buttonDragText, _useDragAndDrop ? EditorStyles.toolbarButton : EditorStyles.toolbarButton))
            _useDragAndDrop = !_useDragAndDrop;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Tracked Editors Width:", EditorStyles.boldLabel);
        _panelWidth = EditorGUILayout.Slider(_panelWidth, MIN_PANEL_WIDTH, MAX_PANEL_WIDTH);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField($"Tracked Scriptable Objects ({_SOPool.Count}):", EditorStyles.boldLabel);

        foreach (var so in _SOPool)
        {
            if (!IsValid(so)) continue;

            GUILayout.BeginHorizontal();
            {
                bool isExpanded = _foldoutStates.GetValueOrDefault(so);
                string buttonText = isExpanded ? $"▶ {so.name}" : $"▼ {so.name}";

                if (GUILayout.Button(buttonText, isExpanded ? _visibleStyle : _hideStyle))
                    _foldoutStates[so] = !isExpanded;

                if (GUILayout.Button("×", _closeButtonStyle, GUILayout.Width(30)))
                    _deletePool.Add(so);
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(1);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawExpandedPanel(ScriptableObject so)
    {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(_panelWidth), GUILayout.ExpandHeight(true));
        {
            EditorGUILayout.LabelField(so.name, _headerStyle);

            if (!_editorCache.TryGetValue(so, out Editor editor))
            {
                editor = Editor.CreateEditor(so);
                _editorCache[so] = editor;
            }

            if (!_panelScrollPositions.ContainsKey(so))
                _panelScrollPositions[so] = Vector2.zero;

            _panelScrollPositions[so] = EditorGUILayout.BeginScrollView(_panelScrollPositions[so], GUILayout.ExpandHeight(true));
            EditorGUI.BeginChangeCheck();
            editor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(so);
            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
    }

    private void LoadPresetFromSO(ScriptableObjectsWindowPreset preset)
    {
        if (preset == null) return;

        if (_loadMode == LoadMode.Replace)
        {
            ClearWindow();
        }

        foreach (ScriptableObject SO in preset.ScriptableObjects)
        {
            TryAddScriptableObject(SO);
        }
        _needsSort = true;
    }

    private void ValidateScriptableObjectsPool()
    {
        for (int i = _SOPool.Count - 1; i >= 0; i--)
        {
            if (!IsValid(_SOPool[i]) && !_deletePool.Contains(_SOPool[i]))
            {
                _deletePool.Add(_SOPool[i]);
            }
        }
    }

    private void AddSelectedObjects()
    {
        foreach (var obj in Selection.objects)
            TryAddScriptableObject(obj as ScriptableObject);
        _needsSort = true;
    }

    private void TryAddScriptableObject(ScriptableObject so)
    {
        if (IsValid(so) && !_SOPool.Contains(so))
            _SOPool.Add(so);
    }

    private bool IsValid(ScriptableObject so) => so != null && AssetDatabase.Contains(so);

    private void SetSortMode(SortMode mode)
    {
        _currentSortMode = mode;
        _needsSort = true;
    }

    private void DeferredDeletion()
    {
        foreach (var so in _deletePool)
        {
            if (_SOPool.Contains(so))
            {
                _SOPool.Remove(so);
                if (_editorCache.TryGetValue(so, out Editor editor))
                {
                    DestroyImmediate(editor);
                    _editorCache.Remove(so);
                }
                _foldoutStates.Remove(so);
                _panelScrollPositions.Remove(so);
            }
        }
        _deletePool.Clear();
    }

    private void ClearWindow()
    {
        foreach (var editor in _editorCache.Values)
            DestroyImmediate(editor);

        _SOPool.Clear();
        _deletePool.Clear();
        _editorCache.Clear();
        _foldoutStates.Clear();
        _panelScrollPositions.Clear();
    }

    private void OnDisable()
    {
        ClearWindow();
    }
}