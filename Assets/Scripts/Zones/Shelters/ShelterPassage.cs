﻿using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ShelterPassage : MonoBehaviour
{
    [Header("Параметры прохода:")]
    [DisableEdit, SerializeField] private Shelter _parentShelter;
    [SerializeField] private string _passageID = "Passage_1";
    [DisableEdit, SerializeField] private PassageType _passageType;
    [DisableEdit, SerializeField] private BoxCollider _collider;

    public Shelter ParentShelter
    {
        get => _parentShelter;
        set => _parentShelter = value;
    }
    public string PassageID
    {
        get => _passageID;
        set => _passageID = value;
    }
    public PassageType PassageType
    {
        get => _passageType;
        set
        {
            if (_passageType != value)
                _passageType = value;
            SetObjectName();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Определяем направление выхода из коллайдера
            Vector3 playerDirection = (other.transform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(playerDirection, transform.forward);

            // Если проход — вход, а игрок движется против направления, то он заходит
            if (_passageType == PassageType.Entry && dotProduct < 0)
            {
                _parentShelter?.PassageExit(_passageType);
                return;
            }

            // Если проход — выход, а игрок движется по направлению двери, то он выходит
            if (_passageType == PassageType.Exit && dotProduct > 0)
            {
                _parentShelter?.PassageExit(_passageType);
                return;
            }
        }
    }

    private void SetObjectName()
    {
        gameObject.name = _passageID;
    }

#if UNITY_EDITOR
    public void OnInitialize()
    {
        UnityEditor.Undo.RecordObject(this, "Initialize Passage");
        GetCollider();
        SetObjectName();
    }

    private void GetCollider()
    {
        if (_collider == null)
            _collider = GetComponent<BoxCollider>();

        if (_collider != null)
        {
            _collider.isTrigger = true;
            _collider.size = new(1f, 2.1f, 0.25f);
            _collider.center = new(0, _collider.size.y / 2, 0);
        }
    }

    private void OnDrawGizmos()
    {
        // Визуализация для "потерянной" двери
        DrawErrorInfo();
    }

    private void DrawErrorInfo()
    {
        if (_parentShelter != null) return;

        Gizmos.color = Color.red;
        if (_collider != null)
            Gizmos.DrawWireCube(transform.position + _collider.center, _collider.bounds.size);
        Gizmos.DrawIcon(transform.position + _collider.center, "console.erroricon");

        GUIStyle _guiStyle = new GUIStyle
        {
            normal = { textColor = Color.red },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        UnityEditor.Handles.Label
        (
            transform.position + Vector3.up * 1.0f,
            "Проход не относится\nни к какому укрытию!",
            _guiStyle
        );
    }

    public void DrawEntranceGizmo()
    {
        if (_parentShelter == null) return;

        var direction = Vector3.zero;
        if (_passageType == PassageType.Entry)
            direction = transform.forward;
        if (_passageType == PassageType.Exit)
            direction = -transform.forward;

        // Управление цветом отрисовки дверей
        var color = _passageType == PassageType.Entry ? new Color(0f, 0.05f, 0.15f, 1) : new Color(0.15f, 0.05f, 0, 1);

        // Отрисовка направления входа/выхода
        Gizmos.color = color;
        Gizmos.DrawRay(transform.position, direction);
        Gizmos.DrawWireSphere(transform.position + direction, 0.15f);

        // Отрисовка текстовой метки
        GUIStyle _guiStyle = new GUIStyle
        {
            normal = { textColor = color },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        UnityEditor.Handles.Label
        (
            transform.position + Vector3.up * 0.75f,
            $"⬇\n{_passageID}",
            _guiStyle
        );

        // Отрисовка значка двери
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, 0.1f);
    }
#endif
}