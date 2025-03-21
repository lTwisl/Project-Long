using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ShelterSystem : MonoBehaviour
{
    public static Action<ShelterSystem> OnEnterShelter;
    public static Action<ShelterSystem> OnExitShelter;

    [Header("Параметры укрытия:")]
    [SerializeField] private bool _hideEntrances = false;
    [SerializeField] private bool _hideShelterInfo = true;
    [SerializeField, Range(-25, 25)] private float _temperature;
    [SerializeField, Range(0, 100)] private float _wetness;
    [SerializeField, Range(0, 15)] private float _toxicity;

    [Header("Входы и выходы укрытия:")]
    [SerializeField] private bool _entrancesState = true;
    [SerializeField] private List<ShelterEntrance> _entrances = new();

    #region Параметры укрытия
    public float Temperature
    {
        get => _temperature;
        set
        {
            if (Mathf.Approximately(_temperature, value)) return;
            _temperature = value;
        }
    }
    public float Wetness
    {
        get => _wetness;
        set
        {
            if (Mathf.Approximately(_wetness, value)) return;
            _wetness = value;
        }
    }
    public float Toxicity
    {
        get => _toxicity;
        set
        {
            if (Mathf.Approximately(_toxicity, value)) return;
            _toxicity = value;
        }
    }
    #endregion

    /// <summary>
    /// Сменить состояние всех входов/выходов
    /// </summary>
    /// <param name="state"></param>
    public void PlayerEntered(bool state)
    {
        if (state == true)
            OnEnterShelter?.Invoke(this);
        else
            OnExitShelter?.Invoke(this);

        foreach (var entrance in _entrances)
        {
            if (entrance != null)
            {
                entrance.SetEntranceStatus(!state);
                _entrancesState = !state;
            }
        }
    }

    #region ВИЗУАЛИЗАЦИЯ
    private void OnDrawGizmos()
    {
        if (!_hideEntrances)
        {
            foreach (var entrance in _entrances)
            {
                if (entrance == null) continue;

                var entrancePosition = entrance.transform.position;
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, entrancePosition);
                entrance.DrawEntranceGizmo();
            }
        }

        // Отрисовка индикаторов параметров укрытия
        if (_hideShelterInfo) return;
        DrawParameterIndicator(_temperature, new Color(0.88f, 0.5f, 0.1f), -0.6f, -25, 25, "°C");
        DrawParameterIndicator(_wetness, new Color(0.11f, 0.65f, 0.88f), 0, 0, 100, "%");
        DrawParameterIndicator(_toxicity, new Color(0.28f, 0.12f, 0.35f), 0.6f, 0, 15, "ед.");

        // Подпись укрытия
        UnityEditor.Handles.Label(
            transform.position + transform.up * -0.5f,
            $"Shelter:\n{gameObject.name}",
            new GUIStyle
            {
                normal = { textColor = Color.black },
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            }
        );
    }

    private void DrawParameterIndicator(float value, Color color, float offset, float minValue, float maxValue, string unit = "")
    {
        // Настройки индикатора
        var barWidth = 0.1f; // Ширина индикатора
        var barHeight = 10f; // Высота индикатора
        var barDepth = 0.4f; // Глубина индикатора (для 3D-визуализации)
        var textOffset = 0.25f; // Смещение текста относительно индикатора
        var fontSize = 12; // Размер шрифта

        // Нормализация значения в диапазоне [0, 1]
        float normalizedValue = Mathf.InverseLerp(minValue, maxValue, value);
        normalizedValue = Mathf.Clamp01(normalizedValue); // Ограничение в диапазоне [0, 1]

        // Позиция индикатора (выравнивание относительно трансформа)
        var position = transform.position + transform.right * offset;

        // Отрисовка фона индикатора
        Gizmos.color = Color.white;
        Gizmos.DrawCube(position + transform.up * (barHeight / 2), new Vector3(barWidth, barHeight, barDepth));

        // Отрисовка заполненной части индикатора (снизу вверх)
        Gizmos.color = color;
        Gizmos.DrawCube(
            position + transform.up * (barHeight * normalizedValue / 2), // Позиция заполненной части
            new Vector3(barWidth, barHeight * normalizedValue, barDepth) // Размер заполненной части
        );

        // Подпись индикатора
        UnityEditor.Handles.Label(
            position + transform.up * (barHeight + textOffset),
            /*$"{label}: {value:F2} {unit}"*/
            $"{value:F2} {unit}",
            new GUIStyle
            {
                normal = { textColor = color },
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold
            }
        );
    }
    #endregion

    #region EDITOR
    private void OnValidate()
    {
        ChangeEntrancesStatus(_entrancesState);
    }

    public void FindAndConfigureEntrances()
    {
        // Найти все входы и выходы и иерархии
        _entrances.Clear();
        GetComponentsInChildren<ShelterEntrance>(true, _entrances);
        _entrances.RemoveAll(e => e == null || e.transform.parent != transform);

        // Сконфигурировать все входы и выходы в иерархии
        foreach (var entrance in _entrances)
        {
            if (entrance.ShelterSystem != this)
            {
                entrance.SetShelterSystem(this);
            }
        }

        ChangeEntrancesStatus(_entrancesState);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void ChangeEntrancesStatus(bool state)
    {
        foreach (var entrance in _entrances)
        {
            if (entrance == null) continue;

            if (entrance.IsEntrance != state)
            {
                entrance.SetEntranceStatus(state);
            }
        }
    }

    public void AddEntrance(ShelterEntrance shelterEntrance)
    {
        _entrances.Add(shelterEntrance);
        shelterEntrance.SetEntranceStatus(_entrancesState);
    }
    #endregion
}