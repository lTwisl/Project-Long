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

    PropertyField _timeScaleField;
    PropertyField _speedUpTimeScaleField;

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

        var box = new Box();

        Label[] labels = new Label[6];
        for (int i = 0; i < labels.Length; ++i)
        {
            labels[i] = new Label();

            labels[i].style.fontSize = 12;
            labels[i].style.maxHeight = 34;
            labels[i].style.minWidth = 200;

            labels[i].style.textOverflow = TextOverflow.Ellipsis;
            labels[i].style.whiteSpace = WhiteSpace.Normal;
            labels[i].style.overflow = Overflow.Hidden;

            if (i % 2 == 0 || i == labels.Length - 1)
                continue;

            labels[i].style.marginBottom = 10;
        }

        Func<string, VisualElement> createHeaderLabel = (string text) =>
        {
            var v = new Label("<b><size=12>" + text + "</b></size>");
            v.style.maxHeight = 34;
            v.style.minWidth = 200;

            v.style.textOverflow = TextOverflow.Ellipsis;
            v.style.whiteSpace = WhiteSpace.Normal;
            v.style.overflow = Overflow.Hidden;

            return v;
        };

        box.Add(createHeaderLabel("- За сколько секунд реального времени проходит одна игровая минута:"));
        box.Add(labels[0]);
        box.Add(labels[1]);
        box.Add(createHeaderLabel("- Сколько игровых минут проходит за одну минуту реального времени:"));
        box.Add(labels[2]);
        box.Add(labels[3]);
        box.Add(createHeaderLabel("- За сколько реальных минут проходит игровой день:"));
        box.Add(labels[4]);
        box.Add(labels[5]);

        root.Add(box);

        _timeScaleField.RegisterValueChangeCallback(value =>
        {
            float realSecondsPerGameMinuteClassic = 1 / value.changedProperty.floatValue * 60;
            float gameMinutesPerRealMinuteClassic = value.changedProperty.floatValue;
            float realMinutesPerGameDayClassic = realSecondsPerGameMinuteClassic * 24;

            labels[0].text = $"Классическая скорость: <b>{realSecondsPerGameMinuteClassic:F4}</b> (реал. сек)";
            labels[2].text = $"Классическая скорость: <b>{gameMinutesPerRealMinuteClassic:F4}</b> (игр. мин)";
            labels[4].text = $"Классическая скорость: <b>{realMinutesPerGameDayClassic:F4}</b> (реал. мин)";
        });

        _speedUpTimeScaleField.RegisterValueChangeCallback(value =>
        {
            float realSecondsPerGameMinuteClassic = 1 / value.changedProperty.floatValue * 60;
            float gameMinutesPerRealMinuteClassic = value.changedProperty.floatValue;
            float realMinutesPerGameDayClassic = realSecondsPerGameMinuteClassic * 24;

            labels[1].text = $"Ускоренная скорость: <b>{realSecondsPerGameMinuteClassic:F4}</b> (реал. сек)";
            labels[3].text = $"Ускоренная скорость: <b>{gameMinutesPerRealMinuteClassic:F4}</b> (игр. мин)";
            labels[5].text = $"Ускоренная скорость: <b>{realMinutesPerGameDayClassic:F4}</b> (реал. мин)";
        });


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

        var horBox = new VisualElement();
        horBox.style.flexDirection = FlexDirection.Row;
        horBox.style.justifyContent = Justify.SpaceBetween; // Distribute space between items
        horBox.style.alignItems = Align.Stretch; // Stretch items vertically
        horBox.style.flexWrap = Wrap.NoWrap;
        horBox.style.width = Length.Percent(100);
        //horBox.style.height = Length.Percent(100); // Ensure container has height

        var btnSubtractDay = new Button(() =>
        {
            TimeSpan time = GetCurrentTime((GameTime)target);
            SetCurrentTime((GameTime)target, new TimeSpan(time.Days - 1, time.Hours, time.Minutes, 0));
        });
        btnSubtractDay.text = "- 1 Day";
        btnSubtractDay.style.flexGrow = 1;
        horBox.Add(btnSubtractDay);

        var btnSubtractHour = new Button(() =>
        {
            TimeSpan time = GetCurrentTime((GameTime)target);
            SetCurrentTime((GameTime)target, new TimeSpan(time.Days, time.Hours - 1, time.Minutes, 0));
        });
        btnSubtractHour.text = "- 1 Hour";
        btnSubtractHour.style.flexGrow = 1;
        horBox.Add(btnSubtractHour);


        var btnAddHour = new Button(() =>
        {
            TimeSpan time = GetCurrentTime((GameTime)target);
            SetCurrentTime((GameTime)target, new TimeSpan(time.Days, time.Hours + 1, time.Minutes, 0));
        });
        btnAddHour.text = "+ 1 Hour";
        btnAddHour.style.flexGrow = 1;
        horBox.Add(btnAddHour);

        var btnAddDay = new Button(() =>
        {
            TimeSpan time = GetCurrentTime((GameTime)target);
            SetCurrentTime((GameTime)target, new TimeSpan(time.Days + 1, time.Hours, time.Minutes, 0));
        });
        btnAddDay.text = "+ 1 Day";
        btnAddDay.style.flexGrow = 1;
        horBox.Add(btnAddDay);


        root.Add(horBox);

        // Update loop
        root.schedule.Execute(() =>
        {
            if (Application.isPlaying)
                UpdateTimeDisplay();

            btnApplyTime.style.display = Application.isPlaying ? DisplayStyle.Flex : DisplayStyle.None;
            horBox.style.display = Application.isPlaying ? DisplayStyle.Flex : DisplayStyle.None;

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

                switch (iterator.name)
                {
                    case "_timeScale":
                        _timeScaleField = propertyField;
                        break;
                    case "_speedUpTimeScale":
                        _speedUpTimeScaleField = propertyField;
                        break;
                    default:
                        break;
                }

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