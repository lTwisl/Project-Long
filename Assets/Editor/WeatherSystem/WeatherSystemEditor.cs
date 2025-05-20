using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(WeatherSystem))]
public class WeatherSystemEditor : Editor
{
    private const float SPACING = 15f;

    public override VisualElement CreateInspectorGUI()
    {
        var container = new VisualElement();

        // Основной инспектор по умолчанию
        InspectorElement.FillDefaultInspector(container, serializedObject, this);

        // Кастомные элементы управления
        CreateValidationControls(container);
        CreateTransitionControls(container);
        CreateTimeInfoSection(container);

        return container;
    }

    private void CreateValidationControls(VisualElement container)
    {
        var horizontalBox = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween,
                marginTop = SPACING
            }
        };

        var buttonValidateRefs = new Button(() => ((WeatherSystem)target).ValidateReferences())
        {
            text = "Проверить модули",
            style =
            {
                backgroundColor = new Color(0.3f, 0.7f, 0.4f),
                color = Color.white,
                flexGrow = 1,
                marginLeft = 5,
                marginRight = 5
            }
        };
        horizontalBox.Add(buttonValidateRefs);

        var buttonCheckModules = new Button(() => ((WeatherSystem)target).SetSceneWeatherInEditor())
        {
            text = "Настроить погоду в сцене (Editor)",
            style =
            {
                backgroundColor = new Color(0.25f, 0.5f, 0.8f),
                color = Color.white,
                flexGrow = 1,
                marginLeft = 5,
                marginRight = 5
            }
        };
        horizontalBox.Add(buttonCheckModules);

        container.Add(horizontalBox);
    }

    private void CreateTransitionControls(VisualElement container)
    {
        var transitionSection = new VisualElement { style = { marginTop = SPACING } };

        var progressBar = new ProgressBar
        {
            title = "Прогресс перехода",
            style = { height = 20, marginTop = 5 }
        };

        var stopButton = new Button(() => ((WeatherSystem)target).StopWeatherTransition())
        {
            text = "Остановить переход",
            style =
            {
                backgroundColor = new Color(0.8f, 0.3f, 0.3f),
                color = Color.white,
                unityFontStyleAndWeight = FontStyle.Bold
            }
        };

        // Обновление прогресса в реальном времени
        progressBar.schedule.Execute(() =>
        {
            progressBar.value = GetTransitionProgress((WeatherSystem)target) * 100;
        }).Every(100);

        transitionSection.Add(new Label("Контроль перехода:") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
        transitionSection.Add(stopButton);
        transitionSection.Add(progressBar);

        container.Add(transitionSection);
    }

    private void CreateTimeInfoSection(VisualElement container)
    {
        var timeSection = new VisualElement { style = { marginTop = SPACING } };
        var system = (WeatherSystem)target;

        CreateReadOnlyField("Начало текущей погоды:", GameTime.GetFormattedTime(system.TimeStartCurrentWeather), timeSection);
        CreateReadOnlyField("Конец текущей погоды:", GameTime.GetFormattedTime(system.TimeEndCurrentWeather), timeSection);
        CreateReadOnlyField("Время перехода:", system.TransitionDuration.ToString(), timeSection);

        container.Add(new Label("Временная информация:") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
        container.Add(timeSection);
    }

    private void CreateReadOnlyField(string label, string value, VisualElement parent)
    {
        var field = new TextField(label) { value = value };
        field.SetEnabled(false);
        field.style.marginTop = 5;
        parent.Add(field);
    }

    private float GetTransitionProgress(WeatherSystem weatherSystem)
    {
        if (!weatherSystem.IsWeatherOnTransitionState) return 0;

        TimeSpan passed = GameTime.GetPassedTime(weatherSystem.TimeEndCurrentWeather);
        return Mathf.Clamp01((float)(passed.TotalSeconds / weatherSystem.TransitionDuration.TotalSeconds));
    }
}