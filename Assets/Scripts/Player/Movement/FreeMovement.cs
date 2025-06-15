using EditorAttributes;
using System.Collections.Generic;
using UnityEngine;

public class FreeMovement : MonoBehaviour
{
    [SerializeField] private bool _useGodMode = false;

    [Header("- - Параметры режима GodMode:")]
    [SerializeField, HideIf(nameof(_useGodMode), false), Min(1)] private float _startSpeed = 10f;
    [SerializeField, HideIf(nameof(_useGodMode), false)] private Vector2 _speedRange = new(0.1f, 100f);
    [SerializeField, HideIf(nameof(_useGodMode), false), Range(1, 25)] private float _changeSpeedMultiplier = 5f;
    [SerializeField, HideIf(nameof(_useGodMode), false)] private List<MonoBehaviour> _disableComponents;

    private Rigidbody _rb;
    private Camera _mainCamera;

    private void Awake()
    {
        _rb = GetComponentInChildren<Rigidbody>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            _useGodMode = !_useGodMode;

        ToggleGodMode();

        if (_useGodMode)
        {
            ChangeSpeed();
            Move();
        }
    }

    /// <summary>
    /// Переключает режим GodMode.
    /// </summary>
    private void ToggleGodMode()
    {
        if (_useGodMode)
        {
            SetEnabledStateTargetComponents(false);
            SetEnabledKinematicRigidbody(true);
        }
        else
        {
            SetEnabledStateTargetComponents(true);
            SetEnabledKinematicRigidbody(false);
        }
    }

    private void SetEnabledStateTargetComponents(bool state)
    {
        foreach (var component in _disableComponents)
        {
            if (component != null && component.enabled != state)
                component.enabled = state;
        }
    }

    private void SetEnabledKinematicRigidbody(bool state)
    {
        if (_rb && _rb.isKinematic != state)
            _rb.isKinematic = state;
    }

    /// <summary>
    /// Обновляет позицию объекта на основе ввода.
    /// </summary>
    private void Move()
    {
        // Получаем направление из камеры
        Vector3 moveDirection = _mainCamera.transform.rotation * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        // Добавляем вертикальное движение
        if (Input.GetKey(KeyCode.E))
            moveDirection.y = 1;
        else if (Input.GetKey(KeyCode.Q))
            moveDirection.y = -1;

        transform.position += moveDirection.normalized * (_startSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Меняет скорость с помощью колеса мыши.
    /// </summary>
    private void ChangeSpeed()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            _startSpeed += scroll * _changeSpeedMultiplier;
            _startSpeed = Mathf.Clamp(_startSpeed, _speedRange.x, _speedRange.y);
        }
    }
}