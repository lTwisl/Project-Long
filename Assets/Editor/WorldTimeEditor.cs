using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(WorldTime))]
public class WorldTimeEditor : Editor
{
    private int day = 0;
    private int hour = 0;
    private int minute = 0;

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

        // 1. За сколько секунд реального времени проходит одна игровая минута
        float realSecondsPerGameMinuteClassic = 1 / worldTime.timeScaleClassic * 60;
        float realSecondsPerGameMinuteSpeedUp = 1 / worldTime.timeScaleSpeedUp * 60;

        // 2. Сколько игровых минут проходит за одну минуту реального времени
        float gameMinutesPerRealMinuteClassic = worldTime.timeScaleClassic;
        float gameMinutesPerRealMinuteSpeedUp = worldTime.timeScaleSpeedUp;

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
            fontSize = 26,
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
        //EditorGUILayout.LabelField(worldTime.CurrentTime.ToString(@"dd  hh\:mm\:ss"), timerStyle);
        string formattedTime = $"{worldTime.CurrentTime.Days:D3}  {worldTime.CurrentTime.Hours:D2}:{worldTime.CurrentTime.Minutes:D2}:{worldTime.CurrentTime.Seconds:D2}";
        EditorGUILayout.LabelField(formattedTime, timerStyle);

        // Слайдер прогресса дня
        EditorGUILayout.Space();
        float progress = (float)(worldTime.CurrentTime.TotalHours % 24) / 24;
        EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, "Прогресс дня");

        // Добавляем раздел для установки игрового времени
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Установить игровое время:", headerStyle);

        // Горизонтальная группа для полей ввода
        EditorGUILayout.BeginVertical();
        {
            day = EditorGUILayout.IntField("День", day, GUILayout.ExpandWidth(true));
            hour = EditorGUILayout.IntField("Час", hour, GUILayout.ExpandWidth(true));
            minute = EditorGUILayout.IntField("Минута", minute, GUILayout.ExpandWidth(true));
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // Кнопка для применения значений
        if (GUILayout.Button("Применить время"))
        {
            TimeSpan newTime = new TimeSpan(day, hour, minute, 0);
            worldTime.CurrentTime = newTime;
        }

        // Кнопки быстрого перехода по времени
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("-1 час"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromHours(-1));
            }
            if (GUILayout.Button("+1 час"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromHours(1));
            }
            GUILayout.Space(10);
            if (GUILayout.Button("-1 день"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromDays(-1));
            }
            if (GUILayout.Button("+1 день"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromDays(1));
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}