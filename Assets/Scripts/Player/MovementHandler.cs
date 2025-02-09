using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class MovementHandler : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        [SerializeField] private float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        [SerializeField] private float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        [SerializeField] private float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        [SerializeField] private float SpeedChangeRate = 10.0f;
        [SerializeField] private AnimationCurve speedChangeRateCurve;

        [Header("Player Crouch")]
        [SerializeField] private float SpeedCrouch = 1.0f;
        [SerializeField] private float CrouchTransitionSpeed = 2.0f;
        [SerializeField] private float CrouchHeight = 1.0f;

        [Header("Player Sliding")]
        [SerializeField] private float SlidingSpeed = 2.0f;
        [SerializeField] private AnimationCurve slidingSpeedCurve;
        [SerializeField] private float SlidingAngle = 20.0f;
        [SerializeField] private AnimationCurve slidingAngleCurve;
        [SerializeField] private float SlidingChangeRate = 5.0f;

        [Space(10)]
        //[Tooltip("The height the player can jump")]
        //[SerializeField] private float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [SerializeField] private float Gravity = -15.0f;

        //[Space(10)]
        //[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        //[SerializeField] private float JumpTimeout = 0.1f;
        //[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        //[SerializeField] private float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [SerializeField] private bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        [SerializeField] private float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [SerializeField] private float GroundedRadius = 0.5f;
        [Tooltip("What layers the character uses as ground")]
        [SerializeField] private LayerMask GroundLayers;

        [Header("Player Ceiling")]
        [SerializeField] private bool Ceilinged = false;
        [Tooltip("Радиус проверки препятствий над головой")]
        [SerializeField] private float CeilingedRadius = 0.5f;
        [SerializeField] private float CeilingedOffset = 0.2f;
        [Tooltip("Слои для проверки препятствий")]
        [SerializeField] private LayerMask CeilingedLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        [SerializeField] private GameObject CinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] private float TopClamp = 89.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] private float BottomClamp = -89.0f;


        // cinemachine
        private float _cinemachineTargetPitch;

        // player
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private Vector3 _slidingGradient;
        float _slidingSpeedChangeRateByFriction;

        RaycastHit _groundInfo;

        // timeout deltatime
        //private float _jumpTimeoutDelta;
        //private float _fallTimeoutDelta;

        private bool _isCrouching;
        private bool _isTransitioning;
        private float _originalCrouchHeight;
        private float _originalCenter;
        private float _orignalCameraHeight;

        private PlayerInput _playerInput;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
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
            _input = GetComponent<StarterAssetsInputs>();
            _playerInput = GetComponent<PlayerInput>();

            _originalCrouchHeight = _controller.height;
            _originalCenter = _controller.center.y;
            _orignalCameraHeight = CinemachineCameraTarget.transform.localPosition.y;
        }

        private void Update()
        {
            GroundedCheck();
            CeilingedCheck();

            JumpAndGravity();
            CrouchHandler();
            Move();
        }

        private void LateUpdate()
        {
            if (_isTransitioning)
                CinemachineCameraTarget.transform.localPosition = new Vector3(CinemachineCameraTarget.transform.localPosition.x, _orignalCameraHeight - (_originalCrouchHeight - _controller.height), CinemachineCameraTarget.transform.localPosition.z);

            CameraRotation();
        }

        private void CrouchHandler()
        {
            if (_input.crouch && !_isCrouching && Grounded)
            {
                _isCrouching = true;
                _isTransitioning = true;
            }
            else if (!_input.crouch && _isCrouching && !Ceilinged)
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

            float targetHeight = _isCrouching ? CrouchHeight : _originalCrouchHeight;

            _controller.height = Mathf.MoveTowards(_controller.height, targetHeight, Time.deltaTime * CrouchTransitionSpeed);
            _controller.center = new Vector3(0.0f, _originalCenter - (_originalCrouchHeight - _controller.height) / 2.0f, 0.0f);

            if (Mathf.Abs(targetHeight - _controller.height) < Mathf.Epsilon)
                _isTransitioning = false;
        }

        public void CeilingedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + _originalCrouchHeight - CeilingedOffset, transform.position.z);
            Ceilinged = Physics.CheckSphere(spherePosition, CeilingedRadius, CeilingedLayers, QueryTriggerInteraction.Ignore);
        }


        private void GroundedCheck()
        {
            Grounded = Physics.SphereCast(transform.position + Vector3.up * _controller.radius, GroundedRadius, Vector3.down, out _groundInfo, GroundedOffset, GroundLayers);
        }

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move()
        {
            // Определение целевой скорости
            float targetSpeed;
            if (_input.move == Vector2.zero)
                targetSpeed = 0.0f;
            else if (_isCrouching)
                targetSpeed = SpeedCrouch;
            else if (_input.sprint)
                targetSpeed = SprintSpeed;
            else
                targetSpeed = MoveSpeed;


            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            }

            float materialFriction = Grounded ? _groundInfo.collider.material.dynamicFriction : 1;

            // Обработка наклонных поверхностей
            if (Grounded)
            {
                inputDirection = Vector3.ProjectOnPlane(inputDirection, _groundInfo.normal).normalized;

                // Угол поверхности
                float angle = Vector3.Angle(Vector3.up, _groundInfo.normal);

                float slidingAngleByFriction = slidingAngleCurve.Evaluate(materialFriction) * SlidingAngle;
                //float slidingAngleByFriction = materialFriction * SlidingAngle; // Угол скальжения в зависимости от физического материала

                // Направление скольжения
                if (angle > slidingAngleByFriction)
                {
                    Vector3 targetSlidingGradient = Vector3.ProjectOnPlane(-Vector3.up, _groundInfo.normal);
                    _slidingGradient = Vector3.MoveTowards(_slidingGradient, targetSlidingGradient, Time.deltaTime * SlidingChangeRate);
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
            float slidingSpeedByFriction = slidingSpeedCurve.Evaluate(materialFriction) * SlidingSpeed;
            Vector3 targetVelocity = inputDirection * targetSpeed + _slidingGradient * slidingSpeedByFriction + new Vector3(0.0f, Grounded ? 0.0f : _verticalVelocity, 0.0f);

            // Интерполяция скорости от текущей к целевой
            float speedChangeRateByFriction = speedChangeRateCurve.Evaluate(materialFriction) * SpeedChangeRate;
            //float speedChangeRateByFriction = materialFriction * SpeedChangeRate; // Скосрось интерполяции скорости в зависимости от физического материала повепхности
            Vector3 currentVelocity = Vector3.ProjectOnPlane(_controller.velocity, _groundInfo.normal); // Исправляет баг с подскоком на наклонных поверхностях
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * speedChangeRateByFriction);

            // Перемещение 
            _controller.Move(currentVelocity * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                //_fallTimeoutDelta = FallTimeout;

                //if (_verticalVelocity < 0.0f)
                //{
                    _verticalVelocity = -2.0f;
                //}

                //if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                //{
                //    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                //}

                //if (_jumpTimeoutDelta >= 0.0f)
                //{
                //    _jumpTimeoutDelta -= Time.deltaTime;
                //}
            }
            //else
            //{
            //    _jumpTimeoutDelta = JumpTimeout;

            //    if (_fallTimeoutDelta >= 0.0f)
            //    {
            //        _fallTimeoutDelta -= Time.deltaTime;
            //    }

            //    _input.jump = false;
            //}

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded)
                Gizmos.color = transparentGreen;
            else
                Gizmos.color = transparentRed;
            
            if (_controller != null) 
                Gizmos.DrawSphere(transform.position + Vector3.up * _controller.radius + Vector3.down * GroundedOffset, GroundedRadius);

            if (Ceilinged)
                Gizmos.color = transparentGreen;
            else
                Gizmos.color = transparentRed;

            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y + _originalCrouchHeight - CeilingedOffset, transform.position.z), CeilingedRadius);
        }

        private void OnGUI()
        {
            if (_controller != null) 
                GUILayout.Label($"Speed: {Mathf.Round(_controller.velocity.magnitude * 100) / 100.0}");
        }
    }
}