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
        [SerializeField] private float _airMultiplier = 0f;
        [SerializeField, DisableField] private bool _readyToJump = true;

        [Header("Crouch")]
        [SerializeField] private float _crouchSpeed = 2f;
        [SerializeField] private float _crouchHeight = 1.2f;
        [SerializeField, DisableField] private bool _isCrouching;

        [Header("Slide")]
        [SerializeField] private float _maxSlideSpeed = 20;
        [SerializeField, DisableField] private bool _isSliding;

        private float _currentSpeed;

        private float _initHeight;
        private float _initCenterHeight;

        // Ground info
        private RaycastHit _groundHit;
        private float _friction;
        private float _slopeLimit;
        private float _groundAngle;


        private Rigidbody _rb;
        private CapsuleCollider _col;
        private PlayerInputs _inputs;

        private StateMachine _stateMachine;
        private CountdownTimer _jumpTimer;

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
            UpdateGroundInfo();

            _stateMachine.Update();

            _currentSpeed = _isCrouching ? _crouchSpeed : (_inputs.sprint ? _runSpeed : _walkSpeed);

            HandleCrouch();

            ClampSpeedAndSetLinearDamping();
        }

        private void UpdateGroundInfo()
        {
            _isGrounded = Physics.Raycast(_col.bounds.center, Vector3.down, out _groundHit, _col.bounds.extents.y + _groundOffset, _whatIsGround);

            if (!_isGrounded)
                _isGrounded = Physics.SphereCast(_col.bounds.center, _col.radius, Vector3.down, out _groundHit, _col.bounds.extents.y - _col.radius + _groundOffset, _whatIsGround);

            _friction = _isGrounded ? Mathf.Min(_col.material.dynamicFriction, _groundHit.collider.material.dynamicFriction) : 0f;
            _slopeLimit = Mathf.Atan(_friction) * Mathf.Rad2Deg;
            _groundAngle = _isGrounded ? Vector3.Angle(Vector3.up, _groundHit.normal) : 0f;
        }

        private void OnDestroy()
        {
            _jumpTimer.Dispose();
        }


        private void Mover()
        {
            Vector3 moveDirection = transform.forward * _inputs.move.y + transform.right * _inputs.move.x;
            moveDirection.Normalize();
            moveDirection = Vector3.ProjectOnPlane(moveDirection, _groundHit.normal);

            if (!_isGrounded)
            {
                _rb.AddForce(moveDirection * (_moveAcceleration * _airMultiplier), ForceMode.Acceleration);
                return;
            }

            if (_stateMachine.CurrentState is SlidingState)
                _rb.AddForce(moveDirection * (_moveAcceleration * Mathf.Pow(Mathf.Cos(_groundAngle * Mathf.Deg2Rad), 2)), ForceMode.Acceleration);
            else
                _rb.AddForce(moveDirection * _moveAcceleration, ForceMode.Acceleration);
        }


        private void HandleCrouch()
        {
            if (_stateMachine.CurrentState is CrouchingState)
            {
                _col.center = new Vector3(0f, _initCenterHeight - (_initHeight - _crouchHeight) / 2, 0f);
                _col.height = _crouchHeight;
            }
            else
            {
                _col.center = new Vector3(_col.center.x, _initCenterHeight, _col.center.z);
                _col.height = _initHeight;
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
            if (_readyToJump && _isGrounded && _stateMachine.CurrentState is not CrouchingState)
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
