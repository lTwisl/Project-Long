using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptableObjectsEditWindowUITK : EditorWindow
{
    private const float CONTROL_PANEL_WIDTH = 300f;
    private const float MIN_PANEL_WIDTH = 200f;
    private const float MAX_PANEL_WIDTH = 600f;

    private enum SortMode { Name, Type, Date }
    private enum LoadMode { Replace, Add }

    private List<ScriptableObject> _SOPool = new();
    private List<ScriptableObject> _deletePool = new();
    private ScriptableObjectsWindowPreset _presetSO;
    private LoadMode _loadMode = LoadMode.Replace;

    private Dictionary<ScriptableObject, bool> _foldoutStates = new();
    private Dictionary<ScriptableObject, Editor> _editorCache = new();
    private Dictionary<ScriptableObject, VisualElement> _editorContainers = new();

    private float _panelWidth = 450f;
    private SortMode _currentSortMode;
    private bool _needsSort;
    private bool _useDragAndDrop = true;

    private VisualElement _root;
    private ScrollView _mainContentScroll;
    private VisualElement _contentContainer;
    private ScrollView _controlPanelScroll;
    private VisualElement _soListContainer;
    private ObjectField _presetField;
    private TextField _descriptionField;
    private FloatField _widthField;

    [MenuItem("Tools/Open SO Edit Window (UITK)")]
    public static void ShowWindow()
    {
        var window = GetWindow<ScriptableObjectsEditWindowUITK>();
        window.titleContent = new GUIContent("SO Edit Window (UITK)");
        window.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        _root = rootVisualElement;
        _root.style.flexDirection = FlexDirection.Column;

        CreateToolbar();
        CreateMainLayout();
    }

    private void CreateToolbar()
    {
        var toolbar = new Toolbar();
        _root.Add(toolbar);

        toolbar.Add(new Label("Sorting by:"));

        var nameButton = new ToolbarToggle { text = "Name" };
        nameButton.RegisterValueChangedCallback(evt => {
            if (evt.newValue) SetSortMode(SortMode.Name);
        });
        toolbar.Add(nameButton);

        var typeButton = new ToolbarToggle { text = "Type" };
        typeButton.RegisterValueChangedCallback(evt => {
            if (evt.newValue) SetSortMode(SortMode.Type);
        });
        toolbar.Add(typeButton);

        var dateButton = new ToolbarToggle { text = "Date" };
        dateButton.RegisterValueChangedCallback(evt => {
            if (evt.newValue) SetSortMode(SortMode.Date);
        });
        toolbar.Add(dateButton);

        // Set initial state
        nameButton.value = _currentSortMode == SortMode.Name;
        typeButton.value = _currentSortMode == SortMode.Type;
        dateButton.value = _currentSortMode == SortMode.Date;

        toolbar.Add(new ToolbarSpacer());

        var addSelectedButton = new ToolbarButton(AddSelectedObjects) { text = "Add Selected In Project" };
        toolbar.Add(addSelectedButton);

        var clearButton = new ToolbarButton(ClearWindow) { text = "Clear All Tracked" };
        toolbar.Add(clearButton);
    }

    private void CreateMainLayout()
    {
        var mainContainer = new VisualElement();
        mainContainer.style.flexDirection = FlexDirection.Row;
        mainContainer.style.flexGrow = 1;
        _root.Add(mainContainer);

        // Main content area
        _mainContentScroll = new ScrollView(ScrollViewMode.Horizontal);
        _mainContentScroll.style.flexGrow = 1;
        mainContainer.Add(_mainContentScroll);

        _contentContainer = new VisualElement();
        _contentContainer.style.flexDirection = FlexDirection.Row;
        _mainContentScroll.Add(_contentContainer);

        // Control panel
        var controlPanel = new VisualElement();
        controlPanel.style.width = CONTROL_PANEL_WIDTH;
        controlPanel.style.borderLeftWidth = 1;
        controlPanel.style.borderRightWidth = 1;
        controlPanel.style.borderTopWidth = 1;
        controlPanel.style.borderBottomWidth = 1;
        controlPanel.style.borderLeftColor = Color.gray;
        controlPanel.style.borderRightColor = Color.gray;
        controlPanel.style.borderTopColor = Color.gray;
        controlPanel.style.borderBottomColor = Color.gray;
        mainContainer.Add(controlPanel);

        _controlPanelScroll = new ScrollView();
        _controlPanelScroll.style.flexGrow = 1;
        controlPanel.Add(_controlPanelScroll);

        // Preset section
        var presetSection = new Foldout { text = "SO Preset", value = true };
        _controlPanelScroll.Add(presetSection);

        _presetField = new ObjectField("Preset");
        _presetField.objectType = typeof(ScriptableObjectsWindowPreset);
        _presetField.RegisterValueChangedCallback(evt => {
            _presetSO = (ScriptableObjectsWindowPreset)evt.newValue;
            UpdatePresetDescription();
        });
        presetSection.Add(_presetField);

        var loadButton = new Button(() => {
            if (_presetSO != null) LoadPresetFromSO(_presetSO);
        })
        { text = "Load Preset" };
        presetSection.Add(loadButton);

        _descriptionField = new TextField("Description");
        _descriptionField.multiline = true;
        _descriptionField.style.whiteSpace = WhiteSpace.Normal;
        _descriptionField.SetEnabled(false);
        presetSection.Add(_descriptionField);

        // Load mode
        var loadModeSection = new Foldout { text = "Load Mode", value = true };
        _controlPanelScroll.Add(loadModeSection);

        var loadModeRadio = new RadioButtonGroup("Mode", new List<string> { "Replace", "Add" });
        loadModeRadio.value = (int)_loadMode;
        loadModeRadio.RegisterValueChangedCallback(evt => {
            _loadMode = (LoadMode)evt.newValue;
        });
        loadModeSection.Add(loadModeRadio);

        // Settings section
        var settingsSection = new Foldout { text = "Settings", value = true };
        _controlPanelScroll.Add(settingsSection);

        var dragDropToggle = new Toggle("Drag and Drop Mode");
        dragDropToggle.value = _useDragAndDrop;
        dragDropToggle.RegisterValueChangedCallback(evt => {
            _useDragAndDrop = evt.newValue;
        });
        settingsSection.Add(dragDropToggle);

        _widthField = new FloatField("Editor Width");
        _widthField.value = _panelWidth;
        _widthField.RegisterValueChangedCallback(evt => {
            _panelWidth = Mathf.Clamp(evt.newValue, MIN_PANEL_WIDTH, MAX_PANEL_WIDTH);
            UpdateEditorWidths();
        });
        settingsSection.Add(_widthField);

        // SO List
        var soListSection = new Foldout { text = $"Tracked Scriptable Objects (0)", value = true };
        _controlPanelScroll.Add(soListSection);

        _soListContainer = new VisualElement();
        soListSection.Add(_soListContainer);

        // Register drag and drop
        _mainContentScroll.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
        _mainContentScroll.RegisterCallback<DragPerformEvent>(OnDragPerform);
    }

    private void UpdatePresetDescription()
    {
        if (_presetSO != null)
        {
            _descriptionField.value = _presetSO.Description;
        }
        else
        {
            _descriptionField.value = string.Empty;
        }
    }

    private void OnDragUpdated(DragUpdatedEvent evt)
    {
        if (!_useDragAndDrop) return;

        bool hasValidObjects = DragAndDrop.objectReferences.Any(obj => obj is ScriptableObject);
        DragAndDrop.visualMode = hasValidObjects ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
        evt.StopPropagation();
    }

    private void OnDragPerform(DragPerformEvent evt)
    {
        if (!_useDragAndDrop) return;

        foreach (var obj in DragAndDrop.objectReferences)
        {
            if (obj is ScriptableObject so)
            {
                TryAddScriptableObject(so);
            }
        }
        _needsSort = true;
        UpdateUI();
        evt.StopPropagation();
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

    private void UpdateUI()
    {
        SortObjects();
        ValidateScriptableObjectsPool();
        DeferredDeletion();

        // Update SO list
        _soListContainer.Clear();

        var soListSection = _controlPanelScroll.Q<Foldout>(null, "unity-foldout");
        soListSection.text = $"Tracked Scriptable Objects ({_SOPool.Count})";

        if (_SOPool.Count == 0)
        {
            var helpBox = new HelpBox("The tracked ScriptableObjects are not selected.\nUse (Drag and Drop) or (Add Selected In Project) to add SO to the window.", HelpBoxMessageType.Warning);
            _soListContainer.Add(helpBox);
            return;
        }

        foreach (var so in _SOPool)
        {
            if (!IsValid(so)) continue;

            var soItem = new VisualElement();
            soItem.style.flexDirection = FlexDirection.Row;
            soItem.style.marginBottom = 2;

            var toggleButton = new Button(() => {
                _foldoutStates[so] = !_foldoutStates.GetValueOrDefault(so);
                UpdateEditorVisibility(so);
            })
            { text = _foldoutStates.GetValueOrDefault(so) ? $"▶ {so.name}" : $"▼ {so.name}" };
            toggleButton.style.flexGrow = 1;
            toggleButton.style.unityTextAlign = TextAnchor.MiddleLeft;
            soItem.Add(toggleButton);

            var deleteButton = new Button(() => {
                _deletePool.Add(so);
                UpdateUI();
            })
            { text = "×" };
            deleteButton.style.color = Color.red;
            deleteButton.style.width = 30;
            soItem.Add(deleteButton);

            _soListContainer.Add(soItem);
        }

        // Update editors
        _contentContainer.Clear();
        _editorContainers.Clear();

        foreach (var so in _SOPool)
        {
            if (!IsValid(so)) continue;

            var editorContainer = new VisualElement();
            editorContainer.style.width = _panelWidth;
            editorContainer.style.flexShrink = 0;
            editorContainer.style.marginRight = 5;
            editorContainer.style.borderLeftWidth = 1;
            editorContainer.style.borderRightWidth = 1;
            editorContainer.style.borderTopWidth = 1;
            editorContainer.style.borderBottomWidth = 1;
            editorContainer.style.borderLeftColor = Color.gray;
            editorContainer.style.borderRightColor = Color.gray;
            editorContainer.style.borderTopColor = Color.gray;
            editorContainer.style.borderBottomColor = Color.gray;
            editorContainer.style.paddingLeft = 5;
            editorContainer.style.paddingRight = 5;
            editorContainer.style.paddingTop = 5;
            editorContainer.style.paddingBottom = 5;

            var header = new Label(so.name);
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.fontSize = 14;
            header.style.color = Color.cyan;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            editorContainer.Add(header);

            if (_foldoutStates.GetValueOrDefault(so))
            {
                if (!_editorCache.TryGetValue(so, out Editor editor))
                {
                    editor = Editor.CreateEditor(so);
                    _editorCache[so] = editor;
                }

                var editorScroll = new ScrollView();
                var editorElement = editor.CreateInspectorGUI();
                if (editorElement == null)
                {
                    // Fallback for editors that don't support UITK
                    editorScroll.Add(new IMGUIContainer(() => {
                        EditorGUI.BeginChangeCheck();
                        editor.OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck())
                            EditorUtility.SetDirty(so);
                    }));
                }
                else
                {
                    editorScroll.Add(editorElement);
                }
                editorContainer.Add(editorScroll);
            }

            _contentContainer.Add(editorContainer);
            _editorContainers[so] = editorContainer;
        }
    }

    private void UpdateEditorVisibility(ScriptableObject so)
    {
        if (_editorContainers.TryGetValue(so, out var container))
        {
            container.Clear();

            if (_foldoutStates.GetValueOrDefault(so))
            {
                var header = new Label(so.name);
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.fontSize = 14;
                header.style.color = Color.cyan;
                header.style.unityTextAlign = TextAnchor.MiddleCenter;
                container.Add(header);

                if (!_editorCache.TryGetValue(so, out Editor editor))
                {
                    editor = Editor.CreateEditor(so);
                    _editorCache[so] = editor;
                }

                var editorScroll = new ScrollView();
                var editorElement = editor.CreateInspectorGUI();
                if (editorElement == null)
                {
                    editorScroll.Add(new IMGUIContainer(() => {
                        EditorGUI.BeginChangeCheck();
                        editor.OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck())
                            EditorUtility.SetDirty(so);
                    }));
                }
                else
                {
                    editorScroll.Add(editorElement);
                }
                container.Add(editorScroll);
            }
        }
    }

    private void UpdateEditorWidths()
    {
        foreach (var container in _editorContainers.Values)
        {
            container.style.width = _panelWidth;
        }
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
        UpdateUI();
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
        UpdateUI();
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
        UpdateUI();
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
        _editorContainers.Clear();

        UpdateUI();
    }

    private void OnDisable()
    {
        ClearWindow();
    }
}