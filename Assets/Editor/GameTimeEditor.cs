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

    private SerializedProperty _initDaysProp;
    private SerializedProperty _initHoursProp;
    private SerializedProperty _initMinutesProp;
    private PropertyField _timeScaleField;
    private PropertyField _speedUpTimeScaleField;

    private bool isDarkTheme => EditorGUIUtility.isProSkin;

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        serializedObject.Update();
        OnEnable(); // Инициализируем свойства

        AddDefaultInspectorProperties(root);
        AddTimeInfoBox(root);
        AddCurrentTimeDisplay(root);
        AddInitTimeFields(root);
        AddApplyButton(root);
        AddTimeControlButtons(root);

        // Обновление UI
        root.schedule.Execute(UpdateTimeDisplay).Every(100);

        return root;
    }

    private void OnEnable()
    {
        _initDaysProp = serializedObject.FindProperty("_initDays");
        _initHoursProp = serializedObject.FindProperty("_initHours");
        _initMinutesProp = serializedObject.FindProperty("_initMinutes");
    }

    #region === UI Sections ===

    private void AddDefaultInspectorProperties(VisualElement root)
    {
        var iterator = serializedObject.GetIterator();
        if (iterator.NextVisible(true))
        {
            do
            {
                if (iterator.name == "_initTime" || iterator.name == "m_Script")
                    continue;

                var propertyField = new PropertyField(iterator);
                root.Add(propertyField);

                switch (iterator.name)
                {
                    case "_timeScale":
                        _timeScaleField = propertyField;
                        break;
                    case "_speedUpTimeScale":
                        _speedUpTimeScaleField = propertyField;
                        break;
                }
            } while (iterator.NextVisible(false));
        }
    }

    private void AddTimeInfoBox(VisualElement root)
    {
        var box = new Box();
        box.style.backgroundColor = isDarkTheme ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.9f, 0.9f, 0.9f);
        box.style.paddingLeft = 10;
        box.style.marginBottom = 10;

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
            if (i % 2 != 0 && i != labels.Length - 1)
                labels[i].style.marginBottom = 10;
        }

        Func<string, VisualElement> createHeader = text =>
        {
            var label = new Label($"<b>{text}</b>");
            label.style.fontSize = 13;
            label.style.marginTop = 10;
            label.style.marginBottom = 5;
            return label;
        };

        box.Add(createHeader("Одна игровая минута в реальном времени:"));
        box.Add(labels[0]);
        box.Add(labels[1]);

        box.Add(createHeader("Одна реальная минута в игровом времени:"));
        box.Add(labels[2]);
        box.Add(labels[3]);

        box.Add(createHeader("Один игровой день в реальном времени:"));
        box.Add(labels[4]);
        box.Add(labels[5]);

        _timeScaleField.RegisterValueChangeCallback(value =>
        {
            float scale = value.changedProperty.floatValue;
            float realSecondsPerGameMinute = 1 / scale * 60;
            float gameMinutesPerRealMinute = scale;
            float realMinutesPerGameDay = realSecondsPerGameMinute * 24;

            labels[0].text = $"Базовое время: <b>{realSecondsPerGameMinute:F3}</b> (реал. сек)";
            labels[2].text = $"Базовое время: <b>{gameMinutesPerRealMinute:F3}</b> (игр. мин)";
            labels[4].text = $"Базовое время: <b>{realMinutesPerGameDay:F3}</b> (реал. мин)";
        });

        _speedUpTimeScaleField.RegisterValueChangeCallback(value =>
        {
            float scale = value.changedProperty.floatValue;
            float realSecondsPerGameMinute = 1 / scale * 60;
            float gameMinutesPerRealMinute = scale;
            float realMinutesPerGameDay = realSecondsPerGameMinute * 24;

            labels[1].text = $"Ускоренное время: <b>{realSecondsPerGameMinute:F3}</b> (реал. сек)";
            labels[3].text = $"Ускоренное время: <b>{gameMinutesPerRealMinute:F3}</b> (игр. мин)";
            labels[5].text = $"Ускоренное время: <b>{realMinutesPerGameDay:F3}</b> (реал. мин)";
        });

        root.Add(box);
    }

    private void AddCurrentTimeDisplay(VisualElement root)
    {
        timeLabel = new Label();
        timeLabel.style.fontSize = 20;
        timeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        timeLabel.style.marginBottom = 10;
        timeLabel.style.color = isDarkTheme ? Color.white : Color.black;
        timeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        root.Add(timeLabel);

        dayProgressBar = new ProgressBar();
        dayProgressBar.style.height = 20;
        dayProgressBar.style.marginBottom = 15;
        root.Add(dayProgressBar);
    }

    private void AddInitTimeFields(VisualElement root)
    {
        var initDaysField = new PropertyField(_initDaysProp);
        initDaysField.RegisterValueChangeCallback(_ =>
        {
            _initDaysProp.intValue = Mathf.Max(0, _initDaysProp.intValue);
            ApplyChangesAndSetTime();
        });
        root.Add(initDaysField);

        var initHoursField = new PropertyField(_initHoursProp);
        initHoursField.RegisterValueChangeCallback(_ =>
        {
            ClampHours();
            ApplyChangesAndSetTime();
        });
        root.Add(initHoursField);

        var initMinutesField = new PropertyField(_initMinutesProp);
        initMinutesField.RegisterValueChangeCallback(_ =>
        {
            ClampMinutes();
            ApplyChangesAndSetTime();
        });
        root.Add(initMinutesField);
    }

    private void ClampHours()
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
    }

    private void ClampMinutes()
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
    }

    private void ApplyChangesAndSetTime()
    {
        serializedObject.ApplyModifiedProperties();
        if (!Application.isPlaying)
        {
            SetCurrentTime((GameTime)target, new TimeSpan(_initDaysProp.intValue, _initHoursProp.intValue, _initMinutesProp.intValue, 0));
            UpdateTimeDisplay();
        }
    }

    private void AddTimeControlButtons(VisualElement root)
    {
        var horBox = new VisualElement();
        horBox.style.flexDirection = FlexDirection.Row;
        horBox.style.justifyContent = Justify.SpaceEvenly;
        horBox.style.alignItems = Align.Center;
        horBox.style.marginBottom = 10;

        Action<TimeSpan> modifyTime = delta =>
        {
            TimeSpan current = GetCurrentTime((GameTime)target);
            SetCurrentTime((GameTime)target, current + delta);
        };

        AddStyledButton(horBox, "◀ -1 Day", () => modifyTime(new TimeSpan(-1, 0, 0, 0)));
        AddStyledButton(horBox, "◀ -1 Hour", () => modifyTime(new TimeSpan(0, -1, 0, 0)));
        AddStyledButton(horBox, "◀ -10 Min", () => modifyTime(new TimeSpan(0, 0, -10, 0)));
        AddStyledButton(horBox, "+10 Min ▶", () => modifyTime(new TimeSpan(0, 0, 10, 0)));
        AddStyledButton(horBox, "+1 Hour ▶", () => modifyTime(new TimeSpan(0, 1, 0, 0)));
        AddStyledButton(horBox, "+1 Day ▶", () => modifyTime(new TimeSpan(1, 0, 0, 0)));

        root.Add(horBox);
    }

    private void AddStyledButton(VisualElement container, string text, Action action)
    {
        var btn = new Button(action) { text = text };
        btn.style.flexGrow = 1;
        btn.style.marginLeft = 2;
        btn.style.backgroundColor = isDarkTheme ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.75f, 0.75f, 0.75f);
        container.Add(btn);
    }

    private void AddApplyButton(VisualElement root)
    {
        var btnApplyTime = new Button(() =>
        {
            TimeSpan time = new TimeSpan(_initDaysProp.intValue, _initHoursProp.intValue, _initMinutesProp.intValue, 0);
            SetCurrentTime((GameTime)target, time);
        });
        btnApplyTime.text = "Apply Time";
        btnApplyTime.style.backgroundColor = isDarkTheme ? new Color(0.3f, 0.5f, 0.3f) : new Color(0.6f, 0.8f, 0.6f);
        btnApplyTime.style.marginBottom = 10;
        btnApplyTime.style.display = DisplayStyle.None;

        root.Add(btnApplyTime);

        root.schedule.Execute(() =>
        {
            btnApplyTime.style.display = Application.isPlaying ? DisplayStyle.Flex : DisplayStyle.None;
        }).Every(100);
    }

    #endregion

    #region === Reflection Helpers ===

    private void UpdateTimeDisplay()
    {
        var gameTime = target as GameTime;
        if (gameTime == null) return;

        var currentTime = GetCurrentTime(gameTime);
        timeLabel.text = $"Day {currentTime.Days}: {currentTime:hh\\:mm\\:ss}";

        double totalDaySeconds = 24 * 3600;
        double currentDaySeconds = currentTime.Hours * 3600 +
                                   currentTime.Minutes * 60 +
                                   currentTime.Seconds;

        dayProgressBar.value = (float)(currentDaySeconds / totalDaySeconds * 100);
        dayProgressBar.title = $"День прошел на: {dayProgressBar.value:F1}%";
    }

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

    #endregion
}