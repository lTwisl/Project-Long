using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SceneScriptsControlsWindow : EditorWindow
{
    private List<Shelter> _shelters = new List<Shelter>();
    private List<ToxicityZone> _toxicityZones = new List<ToxicityZone>();

    private Vector2 _scrollPosition;

    private bool _showSheltersFoldout = true;
    private bool _showAllShelters = true;

    private bool _showZonesFoldout = true;
    private bool _showAllZones = true;

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
        RefreshScriptsLists();
    }

    private void RefreshScriptsLists()
    {
        _shelters = FindObjectsByType<Shelter>(FindObjectsSortMode.None).ToList();
        _toxicityZones = FindObjectsByType<ToxicityZone>(FindObjectsSortMode.None).ToList();
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
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                RefreshScriptsLists();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton))
            {
                _showSheltersFoldout = false;
                _showZonesFoldout = false;
            }

            if (GUILayout.Button("Expand All", EditorStyles.toolbarButton))
            {
                _showSheltersFoldout = true;
                _showZonesFoldout = true;
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
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        {
            DrawSheltersSection();
            DrawToxicityZonesSection();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawSheltersSection()
    {
        _showSheltersFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_showSheltersFoldout, $"Shelters ({_shelters.Count})");
        if (_showSheltersFoldout)
        {
            EditorGUI.BeginChangeCheck();
            _showAllShelters = EditorGUILayout.Toggle("Show All", _showAllShelters);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var shelter in _shelters.Where(s => s != null))
                {
                    shelter.ShowInfo = _showAllShelters;
                }
            }

            EditorGUI.indentLevel++;
            foreach (var shelter in _shelters)
            {
                if (shelter == null) continue;

                EditorGUILayout.BeginHorizontal();
                {
                    SelectObjectButton(shelter.gameObject);
                    shelter.ShowInfo = EditorGUILayout.Toggle(shelter.ShowInfo);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(10);
    }

    private void DrawToxicityZonesSection()
    {
        _showZonesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_showZonesFoldout, $"Toxicity Zones ({_toxicityZones.Count})");
        if (_showZonesFoldout)
        {
            EditorGUI.BeginChangeCheck();
            _showAllZones = EditorGUILayout.Toggle("Show All", _showAllZones);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var zone in _toxicityZones.Where(z => z != null))
                {
                    zone.ShowInfo = _showAllZones;
                }
            }

            EditorGUI.indentLevel++;
            foreach (var zone in _toxicityZones)
            {
                if (zone == null) continue;

                EditorGUILayout.BeginHorizontal();
                {
                    SelectObjectButton(zone.gameObject);
                    zone.ShowInfo = EditorGUILayout.Toggle(zone.ShowInfo);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(10);
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