using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static PlayerProbeData;

/// <summary>
/// Редактор инструмента проверки проходимости персонажа.
/// </summary>
public class PlayerProbeEditorWindow : EditorWindow
{
    [SerializeField] private PlayerDimensions _playerDimensions = new PlayerDimensions();
    [SerializeField] private LayerMask _obstacleLayers = Physics.AllLayers;
    [SerializeField] private List<PlayerProbeData> _probes = new List<PlayerProbeData>();
    [SerializeField] private int _selectedProbeIndex = -1;
    [SerializeField] private ProbeState _defaultProbeState = ProbeState.Standing;

    [SerializeField] private Material _normalsValidatorMaterial;

    [MenuItem("Tools/Open Player Movement Checker")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlayerProbeEditorWindow>("Player Movement Checker");
        window.minSize = new Vector2(450, 400);
        window.maxSize = new Vector2(450, 1440);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SetMaterialFloatValue(_normalsValidatorMaterial, "_Alpha", 0);
    }

    private void OnGUI()
    {
        DrawPlayerDimensions();
        DrawObstacleSettings();
        DrawProbeListSettings();
        DrawProbeSelection();
        DrawShadersControls();
        DrawSlidingInfo();
        DrawHotkeys();
    }

    #region Player Dimensions UI

    private void DrawPlayerDimensions()
    {
        GUILayout.Label("- - Player Dimensions:", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        _playerDimensions.heightStanding = EditorGUILayout.FloatField("Standing Height", Mathf.Max(_playerDimensions.heightStanding, _playerDimensions.capsuleRadius));
        _playerDimensions.heightCrouching = EditorGUILayout.FloatField("Crouching Height", Mathf.Max(_playerDimensions.heightCrouching, _playerDimensions.capsuleRadius));
        _playerDimensions.capsuleRadius = EditorGUILayout.FloatField("Player Radius", _playerDimensions.capsuleRadius);
        _playerDimensions.heightJumping = EditorGUILayout.FloatField("Jump Height", Mathf.Max(_playerDimensions.heightJumping, _playerDimensions.capsuleRadius));
        _playerDimensions.groundOffset = EditorGUILayout.FloatField("Ground Offset", _playerDimensions.groundOffset);

        GUILayout.Space(5);

        if (GUILayout.Button("Use Scene Player Dimensions"))
        {
            UsePlayerReference();
        }

        if (GUILayout.Button("Update Dimensions To All Probes"))
        {
            foreach (var probe in _probes)
                probe.UpdateProbe(probe.Position, probe.State, _playerDimensions);
        }

        GUILayout.Space(10);
    }

    private void UsePlayerReference()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!player)
        {
            EditorUtility.DisplayDialog("Player Not Found", "No GameObject with tag 'Player' found in the scene.", "OK");
            return;
        }

        CapsuleCollider capsule = player.GetComponentInChildren<CapsuleCollider>();
        if (!capsule)
        {
            EditorUtility.DisplayDialog("CapsuleCollider Missing", "Selected Player does not have a CapsuleCollider component.", "OK");
            return;
        }

        _playerDimensions = new PlayerDimensions
        {
            heightStanding = capsule.height,
            capsuleRadius = capsule.radius,
            heightCrouching = _playerDimensions.heightCrouching,
            heightJumping = _playerDimensions.heightJumping,
            groundOffset = _playerDimensions.groundOffset
        };

        EditorUtility.DisplayDialog("Success", "Player dimensions loaded from CapsuleCollider.", "OK");
    }

    #endregion

    #region Obstacle Settings UI

    private void DrawObstacleSettings()
    {
        EditorGUI.BeginChangeCheck();
        GUILayout.Label("- - Collision Layers Settings:", EditorStyles.boldLabel);
        _obstacleLayers = EditorGUILayout.MaskField("Obstacle Layers", _obstacleLayers, UnityEditorInternal.InternalEditorUtility.layers);

        if (EditorGUI.EndChangeCheck())
        {
            foreach (var probe in _probes)
            {
                probe.ObstacleLayers = _obstacleLayers;
            }
        }

        GUILayout.Space(10);
    }

    #endregion

    #region Probe Controls UI

    private void DrawProbeListSettings()
    {
        GUILayout.Label("- - Probe Settings:", EditorStyles.boldLabel);

        if (GUILayout.Button("Add New Probe"))
        {
            AddProbe();
        }

        GUILayout.Space(10);

        for (int i = 0; i < _probes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Probe №{i}:");
            GUIStyle statusStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = _probes[i].VisualizeColor }
            };
            GUILayout.Label($"{(_probes[i].IsPassable ? "✔ Pass" : "❌ Blocked")}", statusStyle);

            ProbeState newState = (ProbeState)EditorGUILayout.EnumPopup(_probes[i].State, GUILayout.Width(100));
            if (newState != _probes[i].State)
            {
                _probes[i].SetState(newState);
            }

            if (GUILayout.Button("Update", GUILayout.Width(60)))
            {
                _probes[i].UpdateProbe(_probes[i].Position, _probes[i].State, _playerDimensions);
            }

            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                _probes.RemoveAt(i);
                Repaint();
                EditorGUILayout.EndHorizontal();
                continue;
            }

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
    }

    private void DrawProbeSelection()
    {
        if (_probes.Count == 0)
        {
            GUILayout.Label("No probes available. Add a probe first.", EditorStyles.helpBox);
            return;
        }

        int min = 0;
        int max = _probes.Count - 1;
        _selectedProbeIndex = Mathf.Clamp(_selectedProbeIndex, min, max);
        _selectedProbeIndex = EditorGUILayout.IntSlider("Selected Probe To Place", _selectedProbeIndex, min, max);
    }

    private void AddProbe()
    {
        PlayerProbeData newProbe = new PlayerProbeData(Vector3.zero, _defaultProbeState, _playerDimensions);
        newProbe.ProbeIndex = _probes.Count;
        newProbe.ObstacleLayers = _obstacleLayers;
        _probes.Add(newProbe);
        _selectedProbeIndex = _probes.Count - 1;
    }

    #endregion

    #region Probe Controls Scene

    private void OnSceneGUI(SceneView sceneView)
    {
        HandleSceneInput(sceneView);

        for (int i = 0; i < _probes.Count; i++)
        {
            if (_probes[i] == null) continue;

            if (i == _selectedProbeIndex)
            {
                Vector3 oldPos = _probes[i].Position;
                Vector3 newPos = Handles.PositionHandle(oldPos, Quaternion.identity);

                if (newPos != oldPos)
                {
                    Undo.RecordObject(this, "Move Probe");
                    _probes[i].UpdateProbe(newPos, _probes[i].State, _playerDimensions);
                    _probes[i].ObstacleLayers = _obstacleLayers;
                    EditorUtility.SetDirty(this);
                }
            }

            _probes[i].DrawProbe();
        }
    }

    private void HandleSceneInput(SceneView sceneView)
    {
        Event currentEvent = Event.current;

        // Хоткей для добавления новой пробы (Ctrl + N)
        if (currentEvent.type == EventType.KeyDown && currentEvent.control && currentEvent.keyCode == KeyCode.N)
        {
            Undo.RecordObject(this, "Add Probe via Hotkey");
            AddProbe();
            _probes[^1].UpdateProbe(Vector3.zero, _defaultProbeState, _playerDimensions);
            _probes[^1].ObstacleLayers = _obstacleLayers;
            EditorUtility.SetDirty(this);
            _selectedProbeIndex = _probes.Count - 1;
            SceneView.RepaintAll();
            currentEvent.Use();
        }

        // Хоткей для удаления выбранной пробы (Ctrl + R)
        if (currentEvent.type == EventType.KeyDown && currentEvent.control && currentEvent.keyCode == KeyCode.R && _selectedProbeIndex >= 0 && _selectedProbeIndex < _probes.Count)
        {
            Undo.RecordObject(this, "Remove Probe via Hotkey");
            _probes.RemoveAt(_selectedProbeIndex);
            _selectedProbeIndex = Mathf.Clamp(_selectedProbeIndex, 0, _probes.Count - 1);
            EditorUtility.SetDirty(this);
            Repaint();
            currentEvent.Use();
        }

        // Хоткей для смены типа выбранной пробы (Ctrl + T)
        if (currentEvent.type == EventType.KeyDown && currentEvent.control && currentEvent.keyCode == KeyCode.T && _selectedProbeIndex >= 0 && _selectedProbeIndex < _probes.Count)
        {
            var probe = _probes[_selectedProbeIndex];
            ProbeState[] states = (ProbeState[])System.Enum.GetValues(typeof(ProbeState));
            int currentIndex = System.Array.IndexOf(states, probe.State);
            int nextIndex = (currentIndex + 1) % states.Length;
            probe.SetState(states[nextIndex]);
            currentEvent.Use();
            Repaint();
        }

        // Хоткей для смены выбранной пробы (Ctrl + Scroll)
        if (currentEvent.type == EventType.ScrollWheel && currentEvent.control)
        {
            if (_probes.Count > 0)
            {
                if (_selectedProbeIndex < 0 || _selectedProbeIndex >= _probes.Count)
                {
                    _selectedProbeIndex = 0;
                }
                else
                {
                    _selectedProbeIndex += currentEvent.delta.y > 0 ? -1 : 1;
                    _selectedProbeIndex = Mathf.Clamp(_selectedProbeIndex, 0, _probes.Count - 1);
                }

                currentEvent.Use();
                Repaint();
            }
        }

        // Хоткей для смены позиции выбранной пробы (Ctrl + LBM)
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && EditorGUIUtility.hotControl == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (currentEvent.control || currentEvent.command)
                {
                    PlaceProbeAt(hit.point);
                    currentEvent.Use();
                    SceneView.RepaintAll();
                }
            }
        }
    }

    private void PlaceProbeAt(Vector3 position)
    {
        if (_selectedProbeIndex >= 0 && _selectedProbeIndex < _probes.Count)
        {
            var selectedProbe = _probes[_selectedProbeIndex];
            Undo.RecordObject(this, "Move Probe");
            selectedProbe.UpdateProbe(position, selectedProbe.State, _playerDimensions);
            selectedProbe.ObstacleLayers = _obstacleLayers;
            EditorUtility.SetDirty(this);
        }
        else
        {
            Undo.RecordObject(this, "Add Probe");
            AddProbe();
            _probes[^1].UpdateProbe(position, _defaultProbeState, _playerDimensions);
            _probes[^1].ObstacleLayers = _obstacleLayers;
            EditorUtility.SetDirty(this);
            _selectedProbeIndex = _probes.Count - 1;
        }

        SceneView.RepaintAll();
    }

    #endregion

    #region Shaders Controls

    private void DrawShadersControls()
    {
        GUILayout.Space(10);
        GUILayout.Label("- - Normals Validation:", EditorStyles.boldLabel);

        _normalsValidatorMaterial = EditorGUILayout.ObjectField("Normals Validator Material", _normalsValidatorMaterial, typeof(Material), false) as Material;

        ChangeMaterialFloatValue(_normalsValidatorMaterial, "_Alpha", 0, 1);
        ChangeMaterialFloatValue(_normalsValidatorMaterial, "_MaxWalkAngle", 0, 90);
    }

    public void ChangeMaterialFloatValue(Material material, string paramName, float sliderMin, float sliderMax)
    {
        if (material == null) return;

        if (material.HasProperty(paramName))
            material.SetFloat(paramName, EditorGUILayout.Slider(paramName.Replace("_", ""), _normalsValidatorMaterial.GetFloat(paramName), sliderMin, sliderMax));
    }

    public void SetMaterialFloatValue(Material material, string paramName, float value)
    {
        if (material == null) return;

        if (material.HasProperty(paramName))
            material.SetFloat(paramName, value);
    }

    #endregion

    #region Sliding Info

    private void DrawSlidingInfo()
    {
        GUILayout.Space(10);
        GUILayout.Label("- - Sliding Info:", EditorStyles.boldLabel);;
    }

    #endregion

    private void DrawHotkeys()
    {
        GUILayout.Space(15);
        GUILayout.Label("- - Hotkeys:", EditorStyles.boldLabel);
        GUILayout.Label("Hold (Ctrl) and click (LMB) in the SceneView to place probe.", EditorStyles.helpBox);
        GUILayout.Label("Hold (Ctrl + N) to add a new probe.", EditorStyles.helpBox);
        GUILayout.Label("Hold (Ctrl + T) to change state of selected probe.", EditorStyles.helpBox);
        GUILayout.Label("Hold (Ctrl + R) to remove selected probe.", EditorStyles.helpBox);
        GUILayout.Label("Hold (Ctrl) and scroll (Mouse Wheel) in the SceneView to change selected probe index.", EditorStyles.helpBox);
    }
}