using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeatherSystem))]
public class WeatherSystemEditor : Editor
{
    private Color _baseColor;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        WeatherSystem weatherSystem = (WeatherSystem)target;

        _baseColor = GUI.backgroundColor;

        GUILayout.Space(15);
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.29f, 0.565f, 0.886f);
        if (GUILayout.Button("Проверить модули"))
        {
            weatherSystem.ValidateReferences();
        }

        GUI.backgroundColor = new Color(0.494f, 0.827f, 0.129f);
        if (GUILayout.Button("Настроить сцену"))
        {
            weatherSystem.SetNewWeatherImmediatelyEditor();
        }
        EditorGUILayout.EndHorizontal();


        DrawTransitionControls(weatherSystem);
        DrawTimeInfo(weatherSystem);
    }

    void DrawTransitionControls(WeatherSystem weatherSystem)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Контроль перехода:", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("Остановить преход"))
        {
            weatherSystem.StopWeatherTransition();
        }
        GUI.backgroundColor = _baseColor;
        EditorGUILayout.EndHorizontal();

        // Прогресс перехода
        Rect rect = EditorGUILayout.GetControlRect();
        EditorGUI.ProgressBar(rect, GetTransitionProgress(weatherSystem), "Прогресс перехода");
    }

    void DrawTimeInfo(WeatherSystem weatherSystem)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Временная информация:", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(true);
        if (WorldTime.Instance != null)
        {
            EditorGUILayout.TextField("Начало текущей погоды:", WorldTime.Instance.GetFormattedTime(weatherSystem.TimeStartCurrentWeather));
            EditorGUILayout.TextField("Конец текущей погоды:", WorldTime.Instance.GetFormattedTime(weatherSystem.TimeEndCurrentWeather));
            EditorGUILayout.TextField("Время перехода:", weatherSystem.TransitionDuration.ToString());
        }
        else
        {
            EditorGUILayout.TextField("Начало текущей погоды:", weatherSystem.TimeStartCurrentWeather.ToString());
            EditorGUILayout.TextField("Конец текущей погоды:", weatherSystem.TimeEndCurrentWeather.ToString());
            EditorGUILayout.TextField("Время перехода:", weatherSystem.TransitionDuration.ToString());
        }
        EditorGUI.EndDisabledGroup();
    }

    private float GetTransitionProgress(WeatherSystem weatherSystem)
    {
        if (!weatherSystem.CheckHasTransition()) return 0;
        if (WorldTime.Instance == null) return 0;

        // Получаем реальную длительность из системы
        TimeSpan duration = weatherSystem.TransitionDuration;

        // Используем время начала перехода, а не погоды
        TimeSpan passed = WorldTime.Instance.GetPassedTime(weatherSystem.TimeEndCurrentWeather);

        return Mathf.Clamp01((float)(passed.TotalSeconds / duration.TotalSeconds));
    }
}