using EditorAttributes;
using FiniteStateMachine;
using ImprovedTimers;
using System;
using UnityEngine;
using Zenject;


namespace FirstPersonMovement
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMovement : MonoBehaviour
    {
        public event Action OnJump = delegate { };
        public event Action OnLand = delegate { };
        public event Action<IState> OnChangedState = delegate { };

        public float WalkSpeed => Request(MoveMode.Walk, _walkSpeed);
        public float RunSpeed => Request(MoveMode.Run, _runSpeed);
        public float CrouchSpeed => Request(MoveMode.Crouch, _crouchSpeed);

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


        [Header("Crouch")]
        [SerializeField] private float _crouchSpeed = 2f;
        [SerializeField] private float _crouchHeight = 1.2f;
        [SerializeField] private float _speedTransitionCrouch = 1f;

        [Header("Slide")]
        [SerializeField] private float _maxSlideSpeed = 20;
        [SerializeField] private float _freeSlideAngle = 55;

        [Header("Wind")]
        [SerializeField] private bool _useWind = true;
        [ShowField(nameof(_useWind)), Range(0f, 1f)]
        [SerializeField] private float _effectWindOnMaxSpeed = 0.5f;

        [Inject] private World _world;

        private bool _readyToJump = true;

        public float CurrentMaxSpeed { get; private set; }
        public IState CurrentState => _stateMachine.CurrentState;

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
        private InputReader _input;
        private CameraController _cameraController;

        private StateMachine _stateMachine;
        private CountdownTimer _jumpTimer;

        public StatsModifiers.StatsMediator SpeedMediator { get; private set; } = new();

        Vector3 moveDirection;

        [HideInInspector] public bool CanWalk = true;
        [HideInInspector] public bool CanRun = true;
        [HideInInspector] public bool CanJump = true;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;

            _col = GetComponent<CapsuleCollider>();

            _input = GetComponent<InputReader>();
            _input.Jump += isPressed =>
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
            SpeedMediator.UpdateModifiers(Time.deltaTime);


            _stateMachine.Update();

            UpdateGroundInfo();

            CurrentMaxSpeed = GetMaxCurrentSpeed();

            UpdateCrouch();

            ClampSpeedAndSetLinearDamping();
        }

        private float GetMaxCurrentSpeed()
        {
            float maxSpeed = CurrentMaxSpeed;

            if (_stateMachine.CurrentState is WalkState)
                maxSpeed = WalkSpeed;
            else if (_stateMachine.CurrentState is RunState)
                maxSpeed = RunSpeed;
            else if (_stateMachine.CurrentState is CrouchingState)
                maxSpeed = CrouchSpeed;

            if (_useWind)
            {
                Vector2 wind = _world.GetWindLocalVector();
                float dot = Vector3.Dot(
                    new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z).normalized,
                    new Vector3(wind.x, 0f, wind.y).normalized
                    );

                float scale = dot * wind.magnitude / WeatherWindSystem.MaxSize;
                maxSpeed *= Utility.MapRange(scale, -1, 1, 1 - _effectWindOnMaxSpeed, 1 + _effectWindOnMaxSpeed);
            }

            if (CurrentMaxSpeed != maxSpeed && maxSpeed < CurrentMaxSpeed)
                return Mathf.MoveTowards(CurrentMaxSpeed, maxSpeed, Time.deltaTime * 10f);
            else
                return maxSpeed;
        }

        protected float Request(MoveMode moveMode, float value)
        {
            var q = new StatsModifiers.Query(new MaxSpeedCondition(moveMode), value);
            SpeedMediator.PerformQuery(this, q);
            return q.Value;
        }

        private void UpdateGroundInfo()
        {
            _isGrounded = Physics.Raycast(_col.bounds.center, Vector3.down, out _groundHit,
                _col.bounds.extents.y + _groundOffset, _whatIsGround, QueryTriggerInteraction.Ignore);

            if (!_isGrounded)
                _isGrounded = Physics.SphereCast(_col.bounds.center, _col.radius, Vector3.down, out _groundHit,
                    _col.bounds.extents.y - _col.radius + _groundOffset, _whatIsGround, QueryTriggerInteraction.Ignore);

            _friction = _isGrounded ? GetFriction(_groundHit.collider.material) : 0f;
            _slopeLimit = Mathf.Atan(_friction) * Mathf.Rad2Deg;
            _groundAngle = _isGrounded ? Vector3.Angle(Vector3.up, _groundHit.normal) : 0f;
        }

        private float GetFriction(PhysicsMaterial other)
        {
            var m1 = other.frictionCombine;
            var m2 = _col.material.frictionCombine;

            if (m1 == PhysicsMaterialCombine.Maximum || m2 == PhysicsMaterialCombine.Maximum)
                return Mathf.Max(_col.material.dynamicFriction, other.dynamicFriction);
            else if (m1 == PhysicsMaterialCombine.Multiply || m2 == PhysicsMaterialCombine.Multiply)
                return _col.material.dynamicFriction * other.dynamicFriction;
            else if (m1 == PhysicsMaterialCombine.Minimum || m2 == PhysicsMaterialCombine.Minimum)
                return Mathf.Min(_col.material.dynamicFriction, other.dynamicFriction);
            else
                return (_col.material.dynamicFriction + other.dynamicFriction) / 2f;
        }


        private void OnDestroy()
        {
            _jumpTimer?.Dispose();
        }


        private void Mover()
        {
            moveDirection = transform.forward * _input.Move.y + transform.right * _input.Move.x;
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

            if (CurrentMaxSpeed != 0)
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

                _rb.linearVelocity = Vector3.ClampMagnitude(horVel, CurrentMaxSpeed) + vertVel;
                return;
            }

            if (!_readyToJump)
                return;

            if (_stateMachine.CurrentState is not SlidingState)
            {
                _rb.linearDamping = _groundDamping * Mathf.Pow(_friction, 2);
                _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, CurrentMaxSpeed);
            }
            else if (_stateMachine.CurrentState is DownSlidingState)
            {
                _rb.linearDamping = 0f;

                Vector3 slidingDir = Vector3.ProjectOnPlane(Vector3.down, _groundHit.normal).normalized;
                Vector3 slidingVel = Utility.ExtractDotVector(_rb.linearVelocity, slidingDir);

                Vector3 lateralVel = Vector3.ClampMagnitude(_rb.linearVelocity - slidingVel, CurrentMaxSpeed);
                slidingVel = Vector3.ClampMagnitude(slidingVel, _maxSlideSpeed);

                _rb.linearVelocity = slidingVel + lateralVel;
            }
            else if (_stateMachine.CurrentState is UpSlidingState)
            {
                _rb.linearDamping = _groundDamping * Mathf.Pow(_friction, 2);
                _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, CurrentMaxSpeed * Mathf.Cos(_groundAngle * Mathf.Deg2Rad));
            }
        }


        private void Jump()
        {
            if (_readyToJump && _isGrounded && _stateMachine.CurrentState is not CrouchingState && _groundAngle < _freeSlideAngle && CanJump)
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

            _stateMachine.OnChangedState += state => OnChangedState.Invoke(state);


            var idel = new IdelState(this);
            var walk = new WalkState(this);
            var run = new RunState(this);
            var crouch = new CrouchingState(this);
            var upSlide = new UpSlidingState(this);
            var downSlide = new DownSlidingState(this);
            var jump = new JumpingState(this);
            var fall = new FallingState(this);

            _stateMachine.AddAnyTransition(idel, new FuncPredicate(() => _isGrounded && _slopeLimit > _groundAngle && !_input.IsCrouching && _input.Move == Vector2.zero));
            _stateMachine.AddAnyTransition(walk, new FuncPredicate(() => _isGrounded && _slopeLimit > _groundAngle && !_input.IsCrouching && CanWalk && (!_input.IsRunning || !CanRun)));
            _stateMachine.AddAnyTransition(run, new FuncPredicate(() => _isGrounded && _slopeLimit > _groundAngle && !_input.IsCrouching && _input.IsRunning && CanRun));
            _stateMachine.AddAnyTransition(crouch, new FuncPredicate(() => _isGrounded && _slopeLimit > _groundAngle && _input.IsCrouching));

            _stateMachine.AddAnyTransition(upSlide, new FuncPredicate(() => _isGrounded && _slopeLimit < _groundAngle && _rb.linearVelocity.y > 0f));
            _stateMachine.AddAnyTransition(downSlide, new FuncPredicate(() => _isGrounded && _slopeLimit < _groundAngle && _rb.linearVelocity.y < 0f));

            _stateMachine.AddAnyTransition(jump, new FuncPredicate(() => !_isGrounded && _rb.linearVelocity.y > 0f && CanJump));
            _stateMachine.AddAnyTransition(fall, new FuncPredicate(() => !_isGrounded && _rb.linearVelocity.y < 0f));

            _stateMachine.SetState(fall);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider != null && collision.collider != _groundHit.collider)
                Debug.Log($"Contact with {collision.gameObject.name}, friction = {GetFriction(collision.collider.material)}");
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            Rect rect = new Rect(5, Screen.height - 50, 170, 40);
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            GUI.Box(rect, $"<size=30><color=white>Speed: {_rb.linearVelocity.magnitude:f2}</color></size>");
            GUI.color = Color.white;
        }

        private void OnValidate()
        {
            if (_col == null)
                _col = GetComponent<CapsuleCollider>();

            _crouchHeight = Mathf.Max(_crouchHeight, _col.radius * 2);
        }


#endif

        public class State : IState
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

        public class IdelState : State
        {
            public IdelState(PlayerMovement movement) : base(movement)
            {
            }
        }

        public class WalkState : State
        {
            public WalkState(PlayerMovement movement) : base(movement)
            {
            }
        }

        public class RunState : State
        {
            public RunState(PlayerMovement movement) : base(movement)
            {
            }
        }

        public class CrouchingState : State
        {
            public CrouchingState(PlayerMovement movement) : base(movement)
            {
            }
        }

        public abstract class SlidingState : State
        {
            protected SlidingState(PlayerMovement movement) : base(movement)
            {
            }
        }

        public class UpSlidingState : SlidingState
        {
            public UpSlidingState(PlayerMovement movement) : base(movement)
            {
            }
        }

        public class DownSlidingState : SlidingState
        {
            public DownSlidingState(PlayerMovement movement) : base(movement)
            {
            }
        }

        public class JumpingState : State
        {
            public JumpingState(PlayerMovement movement) : base(movement)
            {
            }

            public override void OnEnter()
            {
                _movement.OnJump?.Invoke();
            }
        }

        public class FallingState : State
        {
            public FallingState(PlayerMovement movement) : base(movement)
            {
            }

            public override void OnExit()
            {
                _movement.OnLand.Invoke();
            }
        }

    }

}
