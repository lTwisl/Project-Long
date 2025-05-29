using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeatherSystem))]
public class WeatherSystemEditor : Editor
{
    private bool showWeatherState = true;
    private bool showTransitionControls = true;
    private bool showSystemStatus = true;
    private bool showWeatherParameters = true;

    public override void OnInspectorGUI()
    {
        // Получаем ссылку на целевой объект
        WeatherSystem weatherSystem = (WeatherSystem)target;

        // Обновляем объект перед редактированием
        serializedObject.Update();

        // Разделы кастомного интерфейса
        showWeatherState = EditorGUILayout.BeginFoldoutHeaderGroup(showWeatherState, "📊 Параметры погодных условий");
        if (showWeatherState)
        {
            DrawWeatherState(weatherSystem);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space();
        showTransitionControls = EditorGUILayout.BeginFoldoutHeaderGroup(showTransitionControls, "🔄 Управление переходом");
        if (showTransitionControls)
        {
            DrawTransitionControls(weatherSystem);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space();
        showSystemStatus = EditorGUILayout.BeginFoldoutHeaderGroup(showSystemStatus, "✓ Статус систем");
        if (showSystemStatus)
        {
            DrawSystemStatus(weatherSystem);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space();
        showWeatherParameters = EditorGUILayout.BeginFoldoutHeaderGroup(showWeatherParameters, "🌡️ Погодные параметры");
        if (showWeatherParameters)
        {
            DrawWeatherParameters(weatherSystem);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(15);

        // Сохраняем изменения
        serializedObject.ApplyModifiedProperties();

        // Основные параметры через стандартный инспектор
        DrawDefaultInspector();
    }

    private void DrawWeatherState(WeatherSystem weatherSystem)
    {
        EditorGUI.indentLevel++;

        // Текущий профиль
        EditorGUILayout.LabelField("Текущая погода:", weatherSystem.CurrentWeatherProfile?.Identifier.ToString() ?? "Погода не установлена");
        EditorGUILayout.LabelField("Следующая погода:", weatherSystem.NextWeatherProfile?.Identifier.ToString() ?? "Погода не предсказана");
        
        // Состояние перехода
        EditorGUILayout.LabelField("Состояние перехода:", weatherSystem.IsWeatherOnTransitionState ? "В процессе" : "Не начался", weatherSystem.IsWeatherOnTransitionState ? EditorStyles.boldLabel : EditorStyles.label);
        
        // Прогресс-бар перехода
        if (weatherSystem.IsWeatherOnTransitionState)
        {
            float progress = (float)(GameTime.GetPassedTime(weatherSystem.TimeEndCurrentWeather).TotalSeconds / weatherSystem.TransitionDuration.TotalSeconds);
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 20), Mathf.Clamp01(progress), $"Прогресс перехода: {Mathf.Round(progress * 100)}%");
        }
        EditorGUILayout.Space();

        // Временные рамки
        EditorGUILayout.LabelField("Погода началась в:", GameTime.GetFormattedTime(weatherSystem.TimeStartCurrentWeather));
        EditorGUILayout.LabelField("Погода начнет меняться в:", GameTime.GetFormattedTime(weatherSystem.TimeEndCurrentWeather));
        EditorGUILayout.LabelField("Длительность перехода:", GameTime.GetFormattedTime(weatherSystem.TransitionDuration));
        EditorGUILayout.LabelField("Переход закончится в:", GameTime.GetFormattedTime(weatherSystem.TimeEndCurrentWeather + weatherSystem.TransitionDuration));

        EditorGUI.indentLevel--;
    }

    private void DrawTransitionControls(WeatherSystem weatherSystem)
    {
        EditorGUI.indentLevel++;

        // Кнопки управления переходом
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Смена состояния погоды по CurrentProfile"))
            {
                Undo.RecordObject(weatherSystem, "Смена состояния погоды по CurrentProfile");
                weatherSystem.SetWeatherConditionImmediately(weatherSystem.CurrentWeatherProfile);
            }
        }
        else
        {
            if (GUILayout.Button("Смена состояния погоды по CurrentProfile"))
            {
                Undo.RecordObject(weatherSystem, "Смена состояния погоды по CurrentProfile");
                weatherSystem.SetWeatherConditionEditor();
            }
        }
            
        if (GUILayout.Button("Остановить переход погоды"))
        {
            Undo.RecordObject(weatherSystem, "Остановить переход погоды");
            weatherSystem.StopWeatherTransition();
        }

        EditorGUI.indentLevel--;
    }

    private void DrawSystemStatus(WeatherSystem weatherSystem)
    {
        EditorGUI.indentLevel++;

        DrawSystemStatusItem("Освещение", weatherSystem.IsLightingSystemsValid);
        DrawSystemStatusItem("Ветер", weatherSystem.IsWindSystemValid);
        DrawSystemStatusItem("Туман", weatherSystem.IsFogSystemValid);
        DrawSystemStatusItem("Скайбокс", weatherSystem.IsSkyboxSystemValid);
        DrawSystemStatusItem("Постпроцессинг", weatherSystem.IsPostProcessSystemValid);
        DrawSystemStatusItem("VFX", weatherSystem.IsVfxSystemValid);

        if (GUILayout.Button("Проверить системы"))
        {
            weatherSystem.ValidateReferences();
        }

        EditorGUI.indentLevel--;
    }

    private void DrawSystemStatusItem(string systemName, bool isValid)
    {
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = isValid ? Color.green : Color.red;
        GUILayout.Button(isValid ? "✓" : "✗", GUILayout.Width(20), GUILayout.Height(20));
        GUI.backgroundColor = Color.white;
        EditorGUILayout.LabelField(systemName);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawWeatherParameters(WeatherSystem weatherSystem)
    {
        EditorGUI.indentLevel++;

        DrawParameterWithArrow(weatherSystem.Temperature, -25, 25, "Температура", "°C", Color.blue, new Color(1f, 0.64f, 0f));

        DrawParameterWithArrow(weatherSystem.Wetness, 0, 1, "Влажность", "%", Color.white, new Color(0f, 0.7f, 1f));

        DrawParameterWithArrow(weatherSystem.Toxicity, 0, 250, "Токсичность", "ед.", new Color(0.75f, 0.75f, 0.62f, 0.85f), new Color(0.75f, 0.46f, 0.75f, 0.85f));

        EditorGUI.indentLevel--;
    }

    private void DrawParameterWithArrow(float value, float minValue, float maxValue,
        string label, string unit, Color colorStart, Color colorEnd)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        // Название параметра
        EditorGUILayout.LabelField(label + ":", GUILayout.Width(100));

        // Значение справа
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField($"{value:F1}{unit}", GUILayout.Width(75));
        EditorGUILayout.EndHorizontal();

        // Градиентная полоска
        Rect rect = GUILayoutUtility.GetRect(1f, 20f);
        float t = Mathf.InverseLerp(minValue, maxValue, value);
        EditorGUI.DrawRect(rect, Color.Lerp(colorStart, colorEnd, t));

        // Стрелочка/индикатор
        float arrowPos = Mathf.Clamp(t, 0.01f, 0.99f); // чтобы не выходило за границы
        Rect arrowRect = new Rect(rect.x + rect.width * arrowPos - 5, rect.y - 5, 10, 10);
        EditorGUI.DrawRect(arrowRect, new Color(0, 0, 0, 0.7f));
        EditorGUI.DrawRect(new Rect(arrowRect.x + 2, arrowRect.y + 2, 6, 6), Color.white);

        EditorGUILayout.EndVertical();
    }
}