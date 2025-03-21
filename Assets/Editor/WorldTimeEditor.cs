using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(WorldTime))]
public class WorldTimeEditor : Editor
{
    private int day = 0;
    private int hour = 0;
    private int minute = 0;

    private Color _baseColor;

    private void OnEnable()
    {
        EditorApplication.update += Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Repaint;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        WorldTime worldTime = (WorldTime)target;

        _baseColor = GUI.backgroundColor;

        // 1. За сколько секунд реального времени проходит одна игровая минута
        float realSecondsPerGameMinuteClassic = 1 / worldTime.TimeScaleGame * 60;
        float realSecondsPerGameMinuteSpeedUp = 1 / worldTime.TimeScaleSpeedUp * 60;

        // 2. Сколько игровых минут проходит за одну минуту реального времени
        float gameMinutesPerRealMinuteClassic = worldTime.TimeScaleGame;
        float gameMinutesPerRealMinuteSpeedUp = worldTime.TimeScaleSpeedUp;

        // 3. За сколько реальных минут проходит игровой день (24 игровых часа)
        float realMinutesPerGameDayClassic = realSecondsPerGameMinuteClassic * 24;
        float realMinutesPerGameDaySpeedUp = realSecondsPerGameMinuteSpeedUp * 24;

        // Стиль для заголовков
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
        };

        // Стиль для значений
        GUIStyle valueStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 10,
            normal = { textColor = Color.yellow }
        };

        // Стиль для таймера времени
        GUIStyle timerStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.yellow }
        };

        // Отображаем информацию в инспекторе
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("- За сколько секунд реального времени проходит одна игровая минута:", headerStyle);
        EditorGUILayout.LabelField($"Классическая скорость: {realSecondsPerGameMinuteClassic:F4} (реал. сек)", valueStyle);
        EditorGUILayout.LabelField($"Ускоренная скорость: {realSecondsPerGameMinuteSpeedUp:F4} (реал. сек)", valueStyle);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("- Сколько игровых минут проходит за одну минуту реального времени:", headerStyle);
        EditorGUILayout.LabelField($"Классическая скорость: {gameMinutesPerRealMinuteClassic:F4} (игр. мин)", valueStyle);
        EditorGUILayout.LabelField($"Ускоренная скорость: {gameMinutesPerRealMinuteSpeedUp:F4} (игр. мин)", valueStyle);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("- За сколько реальных минут проходит игровой день:", headerStyle);
        EditorGUILayout.LabelField($"Классическая скорость: {realMinutesPerGameDayClassic:F4} (реал. мин)", valueStyle);
        EditorGUILayout.LabelField($"Ускоренная скорость: {realMinutesPerGameDaySpeedUp:F4} (реал. мин)", valueStyle);

        // Отображение текущего игрового времени
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Текущее игровое время:", headerStyle);
        EditorGUILayout.LabelField(worldTime.GetFormattedTime(worldTime.CurrentTime), timerStyle);

        // Слайдер прогресса дня
        EditorGUILayout.Space();
        float progress = (float)(worldTime.CurrentTime.TotalHours % 24) / 24;
        EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, "Прогресс дня");

        // Добавляем раздел для установки игрового времени
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Управление временем:", headerStyle);

        // Кнопки быстрого перехода по времени
        EditorGUILayout.BeginHorizontal();
        {
            GUI.backgroundColor = new Color(1f, 0.8f, 0.4f);
            if (GUILayout.Button("-1 минута"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromMinutes(-1));
            }
            if (GUILayout.Button("+1 минута"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromMinutes(1));
            }
            GUILayout.Space(10);
            GUI.backgroundColor = new Color(1f, 0.6f, 0.3f);
            if (GUILayout.Button("-1 час"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromHours(-1));
            }
            if (GUILayout.Button("+1 час"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromHours(1));
            }
            GUILayout.Space(10);
            GUI.backgroundColor = new Color(1f, 0.4f, 0.2f);
            if (GUILayout.Button("-1 день"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromDays(-1));
            }
            if (GUILayout.Button("+1 день"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromDays(1));
            }
            GUI.backgroundColor = _baseColor;
        }
        EditorGUILayout.EndHorizontal();

        // Горизонтальная группа для полей ввода
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Установить игровое время:", headerStyle);
        EditorGUILayout.BeginVertical();
        {
            day = EditorGUILayout.IntField("День", day, GUILayout.ExpandWidth(false));
            hour = EditorGUILayout.IntField("Час", hour, GUILayout.ExpandWidth(false));
            minute = EditorGUILayout.IntField("Минута", minute, GUILayout.ExpandWidth(false));
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // Кнопка для применения значений
        if (GUILayout.Button("Установить время"))
        {
            TimeSpan newTime = new TimeSpan(day, hour, minute, 0);
            worldTime.CurrentTime = newTime;
        }
    }
}