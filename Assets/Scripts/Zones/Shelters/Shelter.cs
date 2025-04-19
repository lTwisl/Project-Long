using EditorAttributes;
using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zenject;

public enum PassageType
{
    Entry,
    Exit
}

public class Shelter : MonoBehaviour
{
    [Inject] private World _world;

    [Header("Параметры укрытия:")]
    [SerializeField, Range(-25, 25)] private float _temperature;
    [SerializeField, Range(0, 100)] private float _wetness;
    [SerializeField, Range(0, 15)] private float _toxicity;
    public float Temperature
    {
        get => _temperature;
        set
        {
            if (Mathf.Approximately(_temperature, value)) return;
            _temperature = Mathf.Clamp(value, -25, 25);
        }
    }
    public float Wetness
    {
        get => _wetness;
        set
        {
            if (Mathf.Approximately(_wetness, value)) return;
            _wetness = Mathf.Clamp(value, 0, 100);
        }
    }
    public float Toxicity
    {
        get => _toxicity;
        set
        {
            if (Mathf.Approximately(_toxicity, value)) return;
            _toxicity = Mathf.Clamp(value, 0, 15);
        }
    }

    [Header("Параметры визуализации:")]
    public bool ShowInfo = true;

    [Header("Проходы:")]
    [SerializeField] private PassageType _passagesType;
    [SerializeField] private List<ShelterPassage> _passages = new();
    public int GetPassagesCount => _passages.Count;


    public void PassageExit(PassageType type)
    {
        // Если вышли из входа, значит вошли внутрь убежища
        if (type == PassageType.Entry)
        {
            _world.InvokeOnEnterShelter(this);
            SetPassagesType(PassageType.Exit);
            return;
        }

        // Если вышли из выхода, значит вышли из убежища
        if (type == PassageType.Exit)
        {
            _world.InvokeOnExitShelter(this);
            SetPassagesType(PassageType.Entry);
            return;
        }
    }

    private void SetPassagesType(PassageType type)
    {
        foreach (var passage in _passages)
        {
            if (!IsPassageValid(passage))
            {
                Debug.LogWarning($"У <color=orange>{name}</color>. Сломалась дверка! Проверь!");
                continue;
            }

            passage.PassageType = type;
        }
    }

    private bool IsPassageValid(ShelterPassage passage)
    {
        if (passage == null) return false;

        return true;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        _passages.RemoveAll(passage => passage == null || passage.transform.parent != transform || passage.ParentShelter != this);
        SetPassagesType(_passagesType);
    }

    [Button("Найти все дочерние Passages")]
    public void FindAndConfigurePassages()
    {
        UnityEditor.Undo.RecordObject(this, "Find And Configure Passages");

        // Найти все проходы в иерархии
        _passages.Clear();
        GetComponentsInChildren<ShelterPassage>(true, _passages);

        // Отсекаем невалидные
        _passages.RemoveAll(e => e == null || e.transform.parent != transform);

        // Конфигурируем все найденные и добавленные проходы
        foreach (ShelterPassage passage in _passages)
        {
            passage.ParentShelter = this;
        }
    }


    [Button("Создать новый проход", buttonHeight: 30)]
    private void CreateNewPassage()
    {
        UnityEditor.Undo.RegisterCreatedObjectUndo(this, "Create New Passage");

        GameObject passageGO = new();
        passageGO.transform.position = transform.position + transform.forward * 5;
        passageGO.name = $"Passage_{_passages.Count + 1}";
        passageGO.transform.SetParent(transform);

        Collider collider = passageGO.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        ShelterPassage newPassage = passageGO.AddComponent<ShelterPassage>();
        newPassage.PassageID = $"[{_passagesType}] Passage_{_passages.Count + 1}";
        newPassage.ParentShelter = this;
        newPassage.OnInitialize();

        // Добавляем в список проходов
        AddPassage(newPassage);
        
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    public void AddPassage(ShelterPassage passage)
    {
        _passages.Add(passage);
        passage.PassageType = _passagesType;
    }

    private void OnDrawGizmos()
    {
        if (!ShowInfo) return;

        // Проходы
        foreach (var passage in _passages)
        {
            if (!IsPassageValid(passage)) continue;

            Gizmos.color = Color.black;
            // Рисуем линию от убежища к проходу
            Gizmos.DrawLine(transform.position, passage.transform.position);

            // Рисуем проход
            passage.DrawEntranceGizmo();
        }

        var style = new GUIStyle()
        {
            normal = { textColor = Color.black },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };

        // Подпись укрытия
        UnityEditor.Handles.Label(transform.position + transform.up * -0.5f, $"{gameObject.name}\n({_temperature}°C; {_wetness}%; {_toxicity}ед)", style);
    }
#endif
}