using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Inject] private PlayerMovementConfig _moveConfig;
    [Inject] private PlayerParameters _parameters;

    [Header("Cinemachine Camera")]
    [SerializeField] private GameObject _cinemachineCameraTarget;
    [SerializeField] private float _topClamp = 89.0f;
    [SerializeField] private float _bottomClamp = -89.0f;

    public enum PlayerMoveMode
    {
        Idel,
        BaseMove,
        Sprint,
        Crouching,
        Falling,
    }

    public event Action<PlayerMoveMode> OnChangedMoveMode;
    private PlayerMoveMode _moveMode;
    public PlayerMoveMode MoveMode
    {
        get => _moveMode;
        private set
        {
            if (_moveMode == value)
                return;

            _moveMode = value;
            OnChangedMoveMode?.Invoke(_moveMode);
        }
    }

    private bool _grounded = true;
    private RaycastHit _groundInfo;

    private bool _ceilinged = false;

    // cinemachine
    private float _cinemachineTargetPitch;

    // player
    private float _rotationVelocity;
    private float _verticalVelocity;

    private Vector3 _slidingGradient;

    // crouching
    private bool _isCrouching;
    private bool _isTransitioning;
    private float _originalCrouchHeight;
    private float _originalCenter;
    private float _orignalCameraHeight;

    private PlayerInput _playerInput;
    private CharacterController _controller;
    private PlayerInputs _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputs>();
        _playerInput = GetComponent<PlayerInput>();

        _originalCrouchHeight = _controller.height;
        _originalCenter = _controller.center.y;
        _orignalCameraHeight = _cinemachineCameraTarget.transform.localPosition.y;
    }

    private void Update()
    {
        GroundedCheck();
        CeilingedCheck();

        UpdateMoveMode();

        UpdateGravity();
        CrouchHandler();
        Move();
    }

    private void LateUpdate()
    {
        if (_isTransitioning)
            _cinemachineCameraTarget.transform.localPosition = new Vector3(
                _cinemachineCameraTarget.transform.localPosition.x,
                _orignalCameraHeight - (_originalCrouchHeight - _controller.height),
                _cinemachineCameraTarget.transform.localPosition.z);

        CameraRotation();
    }

    private void UpdateMoveMode()
    {
        if (!_grounded)
            MoveMode = PlayerMoveMode.Falling;
        else if (_input.move == Vector2.zero)
            MoveMode = PlayerMoveMode.Idel;
        else if (_isCrouching)
            MoveMode = PlayerMoveMode.Crouching;
        else if (_input.sprint && !_parameters.IsOverLoad && !_parameters.Stamina.IsZero)
            MoveMode = PlayerMoveMode.Sprint;
        else
            MoveMode = PlayerMoveMode.BaseMove;
    }

    private void CrouchHandler()
    {
        if (_input.crouch && !_isCrouching && _grounded)
        {
            _isCrouching = true;
            _isTransitioning = true;
        }
        else if (!_input.crouch && _isCrouching && !_ceilinged)
        {
            _isCrouching = false;
            _isTransitioning = true;
        }

        UpdateCrouch();
    }

    public void UpdateCrouch()
    {
        if (_isTransitioning == false)
            return;

        float targetHeight = _isCrouching ? _moveConfig.CrouchHeight : _originalCrouchHeight;

        _controller.height = Mathf.MoveTowards(_controller.height, targetHeight, Time.deltaTime * _moveConfig.CrouchTransitionSpeed);
        _controller.center = new Vector3(0.0f, _originalCenter - (_originalCrouchHeight - _controller.height) / 2.0f, 0.0f);

        if (Mathf.Abs(targetHeight - _controller.height) < Mathf.Epsilon)
            _isTransitioning = false;
    }

    public void CeilingedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + _originalCrouchHeight - _moveConfig.CeilingedOffset, transform.position.z);
        _ceilinged = Physics.CheckSphere(spherePosition, _moveConfig.CeilingedRadius, _moveConfig.CeilingedLayers, QueryTriggerInteraction.Ignore);
    }


    private void GroundedCheck()
    {
        Vector3 spherePosition = transform.position + Vector3.up * _controller.radius;
        _grounded = Physics.SphereCast(spherePosition, _moveConfig.GroundedRadius, Vector3.down, out _groundInfo, _moveConfig.GroundedOffset, _moveConfig.GroundLayers);
    }

    private void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= _threshold)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetPitch += _input.look.y * _moveConfig.RotationSpeed * deltaTimeMultiplier;
            _rotationVelocity = _input.look.x * _moveConfig.RotationSpeed * deltaTimeMultiplier;

            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

            _cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void Move()
    {
        float targetSpeed = MoveMode switch
        {
            PlayerMoveMode.Sprint => _moveConfig.SprintSpeed,
            PlayerMoveMode.Crouching => _moveConfig.SpeedCrouch,
            _ => _moveConfig.MoveSpeed,
        };


        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        if (_input.move != Vector2.zero)
        {
            inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        }

        float materialFriction = _grounded ? _groundInfo.collider.material.dynamicFriction : 1;

        // Обработка наклонных поверхностей
        if (_grounded)
        {
            inputDirection = Vector3.ProjectOnPlane(inputDirection, _groundInfo.normal).normalized;

            // Угол поверхности
            float angle = Vector3.Angle(Vector3.up, _groundInfo.normal);

            float slidingAngleByFriction = _moveConfig.SlidingAngleCurve.Evaluate(materialFriction) * _moveConfig.SlidingAngle;
            //float slidingAngleByFriction = materialFriction * SlidingAngle; // Угол скальжения в зависимости от физического материала

            // Направление скольжения
            if (angle > slidingAngleByFriction)
            {
                Vector3 targetSlidingGradient = Vector3.ProjectOnPlane(-Vector3.up, _groundInfo.normal);
                _slidingGradient = Vector3.MoveTowards(_slidingGradient, targetSlidingGradient, Time.deltaTime * _moveConfig.SlidingChangeRate);
            }
            else
            {
                _slidingGradient = Vector3.zero;
            }

            // Изменение целевой скорости в зависимости от угла наклона поверхности к направлению движения
            float angleDir = 90 - Vector3.Angle(Vector3.up, inputDirection);
            if (angleDir > 0)
                targetSpeed *= Mathf.Pow(Mathf.Cos(Mathf.Deg2Rad * angleDir), 2);
            else
                targetSpeed += targetSpeed * -Mathf.Sin(Mathf.Deg2Rad * angleDir);
        }

        // Целевая скорость
        float slidingSpeedByFriction = _moveConfig.SlidingSpeedCurve.Evaluate(materialFriction) * _moveConfig.SlidingSpeed;
        Vector3 targetVelocity = inputDirection * targetSpeed + _slidingGradient * slidingSpeedByFriction + new Vector3(0.0f, _grounded ? 0.0f : _verticalVelocity, 0.0f);

        // Интерполяция скорости от текущей к целевой
        float speedChangeRateByFriction = _moveConfig.SpeedChangeRateCurve.Evaluate(materialFriction) * _moveConfig.SpeedChangeRate;
        //float speedChangeRateByFriction = materialFriction * SpeedChangeRate; // Скосрось интерполяции скорости в зависимости от физического материала повепхности
        Vector3 currentVelocity = Vector3.ProjectOnPlane(_controller.velocity, _groundInfo.normal); // Исправляет баг с подскоком на наклонных поверхностях
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * speedChangeRateByFriction);

        // Перемещение 
        _controller.Move(currentVelocity * Time.deltaTime);
    }

    private void UpdateGravity()
    {
        if (_grounded)
            _verticalVelocity = 0.0f;
        else
            _verticalVelocity += _moveConfig.Gravity * Time.deltaTime;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        if (_moveConfig == null) return;

        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (_grounded)
            Gizmos.color = transparentGreen;
        else
            Gizmos.color = transparentRed;

        if (_controller != null)
            Gizmos.DrawSphere(transform.position + Vector3.up * _controller.radius + Vector3.down * _moveConfig.GroundedOffset, _moveConfig.GroundedRadius);

        if (_ceilinged)
            Gizmos.color = transparentGreen;
        else
            Gizmos.color = transparentRed;

        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y + _originalCrouchHeight - _moveConfig.CeilingedOffset, transform.position.z), _moveConfig.CeilingedRadius);
    }

    private void OnGUI()
    {
        if (_controller != null)
            GUILayout.Label($"Speed: {Mathf.Round(_controller.velocity.magnitude * 100) / 100.0}");
    }
}
