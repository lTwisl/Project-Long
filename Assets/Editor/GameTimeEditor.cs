using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(GameTime))]
public class GameTimeEditor : Editor
{
    private Label timeLabel;
    private ProgressBar dayProgressBar;

    SerializedProperty _initDaysProp;
    SerializedProperty _initHoursProp;
    SerializedProperty _initMinutesProp;

    private void OnEnable()
    {
        _initDaysProp = serializedObject.FindProperty("_initDays");
        _initHoursProp = serializedObject.FindProperty("_initHours");
        _initMinutesProp = serializedObject.FindProperty("_initMinutes");
    }

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        // Default inspector properties
        AddDefaultInspectorProperties(root);

        // Current Time Display
        timeLabel = new Label();
        timeLabel.style.fontSize = 20;
        timeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        timeLabel.style.marginBottom = 10;
        timeLabel.style.color = Color.white;
        root.Add(timeLabel);

        // Day Progress Bar
        dayProgressBar = new ProgressBar();
        dayProgressBar.style.height = 20;
        dayProgressBar.style.marginBottom = 15;
        root.Add(dayProgressBar);

        var initDaysField = new PropertyField(_initDaysProp);
        initDaysField.RegisterValueChangeCallback(_ =>
        {
            _initDaysProp.intValue = Mathf.Max(0, _initDaysProp.intValue);
            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
                return;

            SetCurrentTime((GameTime)target, new TimeSpan(_initDaysProp.intValue, _initHoursProp.intValue, _initMinutesProp.intValue, 0));

            UpdateTimeDisplay();
        });
        root.Add(initDaysField);

        var initHoursField = new PropertyField(_initHoursProp);
        initHoursField.RegisterValueChangeCallback(value =>
        {
            if (_initHoursProp.intValue < 0)
            {
                _initDaysProp.intValue -= 1;

                if (_initDaysProp.intValue < 0)
                {
                    _initDaysProp.intValue = 0;
                    _initHoursProp.intValue = 0;
                }
                else
                    _initHoursProp.intValue = 23;
            }
            else if (_initHoursProp.intValue > 23)
            {
                _initDaysProp.intValue += 1;
                _initHoursProp.intValue = 0;
            }

            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
                return;

            SetCurrentTime((GameTime)target, new TimeSpan(_initDaysProp.intValue, _initHoursProp.intValue, _initMinutesProp.intValue, 0));

            UpdateTimeDisplay();
        });
        root.Add(initHoursField);

        var initMinutesField = new PropertyField(_initMinutesProp);
        initMinutesField.RegisterValueChangeCallback(_ =>
        {
            if (_initMinutesProp.intValue < 0)
            {
                _initHoursProp.intValue -= 1;

                if (_initHoursProp.intValue < 0)
                {
                    _initHoursProp.intValue = 0;
                    _initMinutesProp.intValue = 0;
                }
                else
                    _initMinutesProp.intValue = 59;
            }
            else if (_initMinutesProp.intValue > 59)
            {
                _initHoursProp.intValue += 1;
                _initMinutesProp.intValue = 0;
            }

            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
                return;

            SetCurrentTime((GameTime)target, new TimeSpan(_initDaysProp.intValue, _initHoursProp.intValue, _initMinutesProp.intValue, 0));

            UpdateTimeDisplay();
        });
        root.Add(initMinutesField);

        var btnApplyTime = new Button(() =>
        {
            TimeSpan time = new TimeSpan(_initDaysProp.intValue, _initHoursProp.intValue, _initMinutesProp.intValue, 0);
            SetCurrentTime((GameTime)target, time);
        });
        btnApplyTime.text = "Apply";
        root.Add(btnApplyTime);

        // Update loop
        root.schedule.Execute(() =>
        {
            if (Application.isPlaying)
                UpdateTimeDisplay();

            btnApplyTime.style.display = Application.isPlaying ? DisplayStyle.Flex : DisplayStyle.None;

        }).Every(100);

        return root;
    }

    private void AddDefaultInspectorProperties(VisualElement root)
    {
        var iterator = serializedObject.GetIterator();
        if (iterator.NextVisible(true))
        {
            do
            {
                if (iterator.name == "_initTime") 
                    continue;

                var propertyField = new PropertyField(iterator);
                root.Add(propertyField);

                if (iterator.name == "m_Script")
                    propertyField.SetEnabled(false);
            }
            while (iterator.NextVisible(false));
        }
    }

    private void UpdateTimeDisplay()
    {
        var gameTime = target as GameTime;
        if (gameTime == null) 
            return;

        var currentTime = GetCurrentTime(gameTime);
        timeLabel.text = $"Day {currentTime.Days}: {currentTime:hh\\:mm\\:ss}";

        double totalDaySeconds = 24 * 3600;
        double currentDaySeconds = currentTime.Hours * 3600 +
                                  currentTime.Minutes * 60 +
                                  currentTime.Seconds;
        dayProgressBar.value = (float)(currentDaySeconds / totalDaySeconds * 100);
        dayProgressBar.title = $"Day Progress: {dayProgressBar.value:F1}%";
    }

    // Reflection helpers
    private TimeSpan GetCurrentTime(GameTime gameTime)
    {
        var field = typeof(GameTime).GetField("_currentTime",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (TimeSpan)field.GetValue(gameTime);
    }

    private void SetCurrentTime(GameTime gameTime, TimeSpan value)
    {
        typeof(GameTime).GetField("_currentTime",
            BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    }
}