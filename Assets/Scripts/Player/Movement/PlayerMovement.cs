using EditorAttributes;
using FiniteStateMachine;
using ImprovedTimers;
using System;
using UnityEngine;


namespace FirstPersonMovement
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMovement : MonoBehaviour
    {
        public event Action OnJump = delegate { };
        public event Action OnLand = delegate { };

        public bool Use;
        public Vector3 Input;

        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 4f;
        [SerializeField] private float _runSpeed = 6f;
        [SerializeField] private float _moveAcceleration = 50f;
        [SerializeField] private float _groundDamping = 5;

        [Header("Ground")]
        [SerializeField] private LayerMask _whatIsGround = 1;
        [SerializeField] private float _groundOffset = 0.2f;
        [SerializeField, DisableField] private bool _isGrounded;

        [Header("Jump")]
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private float _jumpCooldown = 0.25f;
        [SerializeField] private float _airAcceleration = 0f;
        [SerializeField, DisableField] private bool _readyToJump = true;

        [Header("Crouch")]
        [SerializeField] private float _crouchSpeed = 2f;
        [SerializeField] private float _crouchHeight = 1.2f;
        [SerializeField] private float _speedTransitionCrouch = 1f;
        [SerializeField, DisableField] private bool _isCrouching;

        [Header("Slide")]
        [SerializeField] private float _maxSlideSpeed = 20;
        [SerializeField] private float _freeSlideAngle = 55;

        private float _currentSpeed;

        private float _initHeight;
        private float _initCenterHeight;

        // Ground info
        [Header("Debug info")]
        [SerializeField, DisableField] private float _friction;
        [SerializeField, DisableField] private float _slopeLimit;
        [SerializeField, DisableField] private float _groundAngle;
        private RaycastHit _groundHit;

        private Rigidbody _rb;
        private CapsuleCollider _col;
        private PlayerInputs _inputs;
        private CameraController _cameraController;

        private StateMachine _stateMachine;
        private CountdownTimer _jumpTimer;

        Vector3 moveDirection;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;

            _col = GetComponent<CapsuleCollider>();

            _inputs = GetComponent<PlayerInputs>();
            _inputs.Jump += isPressed =>
            {
                if (isPressed)
                    Jump();
            };

            TryGetComponent(out _cameraController);

            _jumpTimer = new CountdownTimer(_jumpCooldown);
            _jumpTimer.OnTimerStop += ResetJump;

            _initHeight = _col.height;
            _initCenterHeight = _col.center.y;

            SetupStateMachine();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();

            Mover();
        }


        private void Update()
        {
            _stateMachine.Update();

            UpdateGroundInfo();

            _currentSpeed = GetCurrentSpeed();

            UpdateCrouch();

            ClampSpeedAndSetLinearDamping();
        }

        private float GetCurrentSpeed()
        {
            if (_stateMachine.CurrentState is CrouchingState)
                return _crouchSpeed;
            else if (_isGrounded && _inputs.sprint)
                return _runSpeed;
            else
                return _walkSpeed;
        }

        private void UpdateGroundInfo()
        {
            _isGrounded = Physics.Raycast(_col.bounds.center, Vector3.down, out _groundHit,
                _col.bounds.extents.y + _groundOffset, _whatIsGround, QueryTriggerInteraction.Ignore);

            if (!_isGrounded)
                _isGrounded = Physics.SphereCast(_col.bounds.center, _col.radius, Vector3.down, out _groundHit,
                    _col.bounds.extents.y - _col.radius + _groundOffset, _whatIsGround, QueryTriggerInteraction.Ignore);

            _friction = GetFriction();
            _slopeLimit = Mathf.Atan(_friction) * Mathf.Rad2Deg;
            _groundAngle = _isGrounded ? Vector3.Angle(Vector3.up, _groundHit.normal) : 0f;
        }

        private float GetFriction()
        {
            if (!_isGrounded)
                return 0f;

            var m1 = _groundHit.collider.material.frictionCombine;
            var m2 = _col.material.frictionCombine;

            if (m1 == PhysicsMaterialCombine.Maximum || m2 == PhysicsMaterialCombine.Maximum)
                return Mathf.Max(_col.material.dynamicFriction, _groundHit.collider.material.dynamicFriction);
            else if (m1 == PhysicsMaterialCombine.Multiply || m2 == PhysicsMaterialCombine.Multiply)
                return _col.material.dynamicFriction * _groundHit.collider.material.dynamicFriction;
            else if (m1 == PhysicsMaterialCombine.Minimum || m2 == PhysicsMaterialCombine.Minimum)
                return Mathf.Min(_col.material.dynamicFriction, _groundHit.collider.material.dynamicFriction);
            else
                return (_col.material.dynamicFriction + _groundHit.collider.material.dynamicFriction) / 2f;
        }

        private void OnDestroy()
        {
            _jumpTimer.Dispose();
        }


        private void Mover()
        {
            moveDirection = transform.forward * _inputs.move.y + transform.right * _inputs.move.x;
            if (Use)
                moveDirection = Input;
            moveDirection.Normalize();

            if (!_isGrounded)
            {
                _rb.AddForce(moveDirection * _airAcceleration, ForceMode.Acceleration);
                return;
            }

            moveDirection = Vector3.ProjectOnPlane(moveDirection, _groundHit.normal) * _friction;

            if (_stateMachine.CurrentState is SlidingState)
            {
                Vector3 slidingDir = Vector3.ProjectOnPlane(Vector3.down, _groundHit.normal).normalized;

                float dot = Vector3.Dot(moveDirection.normalized, slidingDir);
                if (dot < 0)
                {
                    if (_groundAngle > _freeSlideAngle)
                    {
                        Vector3 rigth = Vector3.Cross(new Vector3(slidingDir.x, 0f, slidingDir.z).normalized, Vector3.up);
                        moveDirection = Vector3.Project(moveDirection, rigth);
                    }
                    else
                    {
                        Vector3 d = Utility.ExtractDotVector(moveDirection, slidingDir);
                        moveDirection -= d;
                        d *= Mathf.Pow(Mathf.Cos(_groundAngle * Mathf.Deg2Rad), 2);
                        moveDirection += d;
                    }
                }
            }

            _rb.AddForce(moveDirection * _moveAcceleration, ForceMode.Acceleration);
        }

        private void UpdateCrouch()
        {
            if (_stateMachine.CurrentState is CrouchingState)
            {
                if (_col.height != _crouchHeight)
                {
                    _col.height = Mathf.MoveTowards(_col.height, _crouchHeight, Time.deltaTime * _speedTransitionCrouch);
                }
            }
            else if (_col.height != _initHeight && 
                !Physics.CheckSphere(transform.position + new Vector3(0f, _initHeight - _col.radius + 0.1f, 0f), _col.radius, _whatIsGround))
            {
                _col.height = Mathf.MoveTowards(_col.height, _initHeight, Time.deltaTime * _speedTransitionCrouch);
            }

            if (_col.height != _initHeight || _col.height != _crouchHeight)
            {
                _col.center = new Vector3(0f, _initCenterHeight - (_initHeight - _col.height) / 2, 0f);
                _cameraController?.SetCameraOffset(new Vector3(0f, -_initHeight + _col.height, 0f));
            }
        }

        private void ClampSpeedAndSetLinearDamping()
        {
            if (!_isGrounded)
            {
                _rb.linearDamping = 0f;

                Vector3 vertVel = new Vector3(0f, _rb.linearVelocity.y, 0f);
                Vector3 horVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

                _rb.linearVelocity = Vector3.ClampMagnitude(horVel, _currentSpeed) + vertVel;
                return;
            }

            if (!_readyToJump)
                return;

            if (_stateMachine.CurrentState is not SlidingState)
            {
                _rb.linearDamping = _groundDamping * Mathf.Pow(_friction, 2);
                _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, _currentSpeed);
            }
            else if (_stateMachine.CurrentState is DownSlidingState)
            {
                _rb.linearDamping = 0f;

                Vector3 slidingDir = Vector3.ProjectOnPlane(Vector3.down, _groundHit.normal).normalized;
                Vector3 slidingVel = Utility.ExtractDotVector(_rb.linearVelocity, slidingDir);

                Vector3 lateralVel = Vector3.ClampMagnitude(_rb.linearVelocity - slidingVel, _currentSpeed);
                slidingVel = Vector3.ClampMagnitude(slidingVel, _maxSlideSpeed);

                _rb.linearVelocity = slidingVel + lateralVel;
            }
            else if (_stateMachine.CurrentState is UpSlidingState)
            {
                _rb.linearDamping = _groundDamping * Mathf.Pow(_friction, 2);
                _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, _currentSpeed * Mathf.Cos(_groundAngle * Mathf.Deg2Rad));
            }
        }


        private void Jump()
        {
            if (_readyToJump && _isGrounded && _stateMachine.CurrentState is not CrouchingState && _groundAngle < _freeSlideAngle)
            {
                _rb.linearDamping = 0f;
                _readyToJump = false;
                _jumpTimer.Start();

                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
                _rb.AddForce(transform.up * _jumpForce, ForceMode.VelocityChange);
            }
        }


        private void ResetJump()
        {
            _readyToJump = true;
        }

        private void SetupStateMachine()
        {
            _stateMachine = new StateMachine();
            //_stateMachine.printLog = true;

            var grounded = new GroundedState(this);
            var upSlide = new UpSlidingState(this);
            var downSlide = new DownSlidingState(this);
            var jump = new JumpingState(this);
            var fall = new FallingState(this);
            var crouch = new CrouchingState(this);

            _stateMachine.AddAnyTransition(grounded, new FuncPredicate(() => _isGrounded && _slopeLimit > _groundAngle && !_inputs.crouch));
            _stateMachine.AddAnyTransition(upSlide, new FuncPredicate(() => _isGrounded && _slopeLimit < _groundAngle && _rb.linearVelocity.y > 0f));
            _stateMachine.AddAnyTransition(downSlide, new FuncPredicate(() => _isGrounded && _slopeLimit < _groundAngle && _rb.linearVelocity.y < 0f));
            _stateMachine.AddAnyTransition(jump, new FuncPredicate(() => !_isGrounded && _rb.linearVelocity.y > 0f));
            _stateMachine.AddAnyTransition(fall, new FuncPredicate(() => !_isGrounded && _rb.linearVelocity.y < 0f));
            _stateMachine.AddAnyTransition(crouch, new FuncPredicate(() => _isGrounded && _slopeLimit > _groundAngle && _inputs.crouch));

            _stateMachine.SetState(fall);
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            GUILayout.TextArea($"Speed: {_rb.linearVelocity.magnitude}");
        }
#endif

        private class State : IState
        {
            protected readonly PlayerMovement _movement;

            public State(PlayerMovement movement)
            {
                _movement = movement;
            }

            public virtual void FixedUpdate() { }

            public virtual void OnEnter() { }

            public virtual void OnExit() { }

            public virtual void Update() { }
        }

        private class GroundedState : State
        {
            public GroundedState(PlayerMovement movement) : base(movement)
            {
            }
        }

        private abstract class SlidingState : State
        {
            protected SlidingState(PlayerMovement movement) : base(movement)
            {
            }
        }

        private class UpSlidingState : SlidingState
        {
            public UpSlidingState(PlayerMovement movement) : base(movement)
            {
            }
        }

        private class DownSlidingState : SlidingState
        {
            public DownSlidingState(PlayerMovement movement) : base(movement)
            {
            }
        }

        private class JumpingState : State
        {
            public JumpingState(PlayerMovement movement) : base(movement)
            {
            }

            public override void OnEnter()
            {
                _movement.OnJump?.Invoke();
            }
        }

        private class FallingState : State
        {
            public FallingState(PlayerMovement movement) : base(movement)
            {
            }

            public override void OnExit()
            {
                _movement.OnLand.Invoke();
            }
        }

        private class CrouchingState : State
        {
            public CrouchingState(PlayerMovement movement) : base(movement)
            {
            }
        }
    }




}
