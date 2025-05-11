using UnityEngine;

[SelectionBase, RequireComponent(typeof(CharacterController), typeof(PlayerInputs))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    [SerializeField] private GameObject _cinemachineCameraTarget;
    [SerializeField] private float _topClamp = 89.0f;
    [SerializeField] private float _bottomClamp = -89.0f;
    [SerializeField] public float RotationSpeed = 1.0f;

    private float _cinemachineTargetPitch;
    private float _rotationVelocity;
    private const float _threshold = 0.01f;

    [SerializeField] private LayerMask _layerGround;
    private RaycastHit _groundInfo;
    [SerializeField] private bool _isGrounded = true;

    private CharacterController _characterController;
    private PlayerInputs _input;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputs>();
    }

    private void FixedUpdate()
    {
        //Move();

        bool prevIsGrounded = _isGrounded;
        CheckGrounded();
        Vector3 inputMove = InputMove();

        float speed = 10f;
        if (!_isGrounded)
        {
            inputMove = Vector3.zero;
            speed = 1f;
        }

        currentHorizxontalVel = Vector3.Lerp(currentHorizxontalVel, inputMove, Time.fixedDeltaTime * speed);

        Vector3 slidingVel = Sliding();

        bool prevIsSliding = _isSliding;
        if (slidingVel == Vector3.zero)
            _isSliding = false;
        else
            _isSliding = true;

        slideVel = Vector3.Lerp(slideVel, slidingVel, Time.fixedDeltaTime);

        if (prevIsSliding && !_isSliding && prevIsGrounded && !_isGrounded)
        {
            gravityVel = new Vector3(0f, slideVel.y / Time.fixedDeltaTime, 0f);
            slideVel = new Vector3(slideVel.x, 0f, slideVel.z);
        }

        Vector3 gravity = Gravity();

        Vector3 prevPos = transform.position;
        _characterController.Move(currentHorizxontalVel + slideVel + gravity);
        Debug.DrawLine(prevPos, transform.position, Color.blue, 10f);
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetPitch += _input.look.y * RotationSpeed;
            _rotationVelocity = _input.look.x * RotationSpeed;

            _cinemachineTargetPitch = Utility.ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

            _cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void GroundedCheck()
    {
        Vector3 origin = _characterController.bounds.center - new Vector3(0f, _characterController.height / 2f - _characterController.radius);
        float dist = _characterController.skinWidth + 0.001f;

        _isGrounded = Physics.SphereCast(origin, _characterController.radius, Vector3.down, out _groundInfo, dist, _layerGround, QueryTriggerInteraction.Ignore);

        Debug.DrawLine(transform.position, transform.position + _groundInfo.normal * 2f, Color.red);
    }
    Vector3 currentHorizxontalVel = Vector3.zero;
    [SerializeField] Vector3 gravityVel = Vector3.zero;
    [SerializeField] Vector3 slideVel = Vector3.zero;
    [SerializeField] bool _isSliding = false;
    private void Move()
    {
        Vector3 origin = _characterController.bounds.center;
        float dist = _characterController.height / 2 + _characterController.stepOffset + _characterController.skinWidth;


        // Input move
        Vector3 offset = new Vector3(0f, _characterController.height / 2 - _characterController.radius, 0f);
        Vector3 p1 = origin - offset;
        Vector3 p2 = origin + offset;

        Vector3 inputMove = transform.rotation * new Vector3(_input.move.x, 0f, _input.move.y).normalized;
        //Vector3 inputMove = transform.rotation * new Vector3(0f, 0f, 0f).normalized;

        bool isHitted = Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, dist, _layerGround, QueryTriggerInteraction.Ignore);
        if (isHitted)
            inputMove = Vector3.ProjectOnPlane(inputMove, hitInfo.normal);

        Vector3 horizontalVel = inputMove * Time.fixedDeltaTime * 4;

        if (Physics.CapsuleCast(p1, p2, _characterController.radius, horizontalVel.normalized, out RaycastHit hit, horizontalVel.magnitude, _layerGround, QueryTriggerInteraction.Ignore))
        {
            float deltaHeight = hit.point.y - transform.position.y;
            if (deltaHeight > 0.3f)
            {
                Vector3 normal = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;
                Debug.DrawLine(hit.point, hit.point + normal * 2f, Color.red);

                float scale = 1 - Vector3.Dot(inputMove, -normal);
                horizontalVel *= scale;
            }
        }

        bool prevIsGrounded = _isGrounded;
        dist = _characterController.height / 2 - _characterController.radius + _characterController.skinWidth + _characterController.stepOffset;
        _isGrounded = Physics.SphereCast(origin, _characterController.radius, Vector3.down, out _groundInfo, dist, _layerGround, QueryTriggerInteraction.Ignore);



        float speed = 10f;
        if (!_isGrounded)
        {
            horizontalVel = Vector3.zero;
            speed = 1f;
        }

        currentHorizxontalVel = Vector3.Lerp(currentHorizxontalVel, horizontalVel, Time.fixedDeltaTime * speed);

        // Gravity And Sliding

        //Debug.Log($"1 - {isHitted} ? {hitInfo.distance} : {dist}");
        if (!isHitted)
        {
            dist = _characterController.height / 2 - _characterController.radius + _characterController.skinWidth + gravityVel.magnitude * Time.fixedDeltaTime;
            isHitted = Physics.SphereCast(origin, _characterController.radius, Vector3.down, out hitInfo, dist, _layerGround, QueryTriggerInteraction.Ignore);
            Debug.DrawLine(origin, origin - Vector3.up * (isHitted ? (hitInfo.distance) : dist));
        }



        float angle = Vector3.Angle(Vector3.up, hitInfo.normal);

        Vector3 verticalVel = Vector3.zero;
        //slideVel = Vector3.zero;
        Vector3 currSlideVel = Vector3.zero;
        //Vector3 velocity = currentHorizxontalVel;



        bool prevIsSliding = _isSliding;
        _isSliding = isHitted && angle > _characterController.slopeLimit;

        if (prevIsSliding && !_isSliding && prevIsGrounded && !_isGrounded)
        {
            gravityVel = new Vector3(0f, slideVel.y / Time.fixedDeltaTime, 0f);
            slideVel = new Vector3(slideVel.x, 0f, slideVel.z);
        }

        if (_isSliding)
        {
            currSlideVel = Vector3.ProjectOnPlane(gravityVel * Time.fixedDeltaTime, hitInfo.normal);
        }

        slideVel = Vector3.Lerp(slideVel, currSlideVel, Time.fixedDeltaTime);



        float forceGravity = isHitted ? (hitInfo.distance) : dist;
        forceGravity -= _characterController.height / 2 - _characterController.radius + _characterController.skinWidth;
        forceGravity = Mathf.Max(0, forceGravity);
        verticalVel = Vector3.down * forceGravity;

        //Debug.Log($"2 - {isHitted} ? {hitInfo.distance} : {dist} | {forceGravity} | {gravityVel.magnitude * Time.fixedDeltaTime}");

        Vector3 prevPos = transform.position;
        _characterController.Move(currentHorizxontalVel + verticalVel + slideVel);
        Debug.DrawLine(prevPos, transform.position, Color.blue, 10f);
        //_characterController.Move(gravityVel * Time.fixedDeltaTime);

        if (!isHitted)
            gravityVel += Physics.gravity * Time.fixedDeltaTime;
        else if (angle > _characterController.slopeLimit)
            gravityVel += Physics.gravity * Time.fixedDeltaTime;
        else
            gravityVel = Physics.gravity * Time.fixedDeltaTime;



        gravityVel = Vector3.ClampMagnitude(gravityVel, 15f);
    }

    private void CheckGrounded()
    {
        Vector3 origin = _characterController.bounds.center;
        float dist = _characterController.height / 2 - _characterController.radius + _characterController.skinWidth + _characterController.stepOffset;
        _isGrounded = Physics.SphereCast(origin, _characterController.radius, Vector3.down, out _groundInfo, dist, _layerGround, QueryTriggerInteraction.Ignore);
    }

    private Vector3 InputMove()
    {
        if (!_isGrounded)
            return Vector3.zero;

        Vector3 inputMove = transform.rotation * new Vector3(_input.move.x, 0f, _input.move.y).normalized;
        //Vector3 inputMove = transform.rotation * new Vector3(0f, 0f, 0f).normalized;

        inputMove = Vector3.ProjectOnPlane(inputMove, _groundInfo.normal);

        Vector3 horizontalVel = inputMove * Time.fixedDeltaTime * 4;

        Vector3 origin = _characterController.bounds.center;
        Vector3 offset = new Vector3(0f, _characterController.height / 2 - _characterController.radius, 0f);
        Vector3 p1 = origin - offset;
        Vector3 p2 = origin + offset;
        if (Physics.CapsuleCast(p1, p2, _characterController.radius, horizontalVel.normalized, out RaycastHit hit, horizontalVel.magnitude, _layerGround, QueryTriggerInteraction.Ignore))
        {
            float deltaHeight = hit.point.y - transform.position.y;
            if (deltaHeight > 0.3f)
            {
                Vector3 normal = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;
                Debug.DrawLine(hit.point, hit.point + normal * 2f, Color.red);

                float scale = 1 - Vector3.Dot(inputMove, -normal);
                horizontalVel *= scale;
            }
        }

        return horizontalVel;
    }

    private Vector3 Sliding()
    {
        Vector3 origin = _characterController.bounds.center;
        float dist = _characterController.height / 2 + _characterController.stepOffset + _characterController.skinWidth;
        bool isHitted = Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, dist, _layerGround, QueryTriggerInteraction.Ignore);

        if (!isHitted)
        {
            if (!_isGrounded)
                return Vector3.zero;
            else
                hitInfo = _groundInfo;
        }

        float angle = Vector3.Angle(Vector3.up, hitInfo.normal);
        if (angle < _characterController.slopeLimit)
            return Vector3.zero;

        return Vector3.ProjectOnPlane(gravityVel * Time.fixedDeltaTime, hitInfo.normal);
    }

    private Vector3 Gravity()
    {
        Vector3 origin = _characterController.bounds.center;
        float dist = _characterController.height / 2 - _characterController.radius + _characterController.skinWidth + gravityVel.magnitude * Time.fixedDeltaTime;
        bool isHitted = Physics.SphereCast(origin, _characterController.radius, Vector3.down, out RaycastHit hitInfo, dist, _layerGround, QueryTriggerInteraction.Ignore);

        float forceGravity = isHitted ? (hitInfo.distance) : dist;
        forceGravity -= _characterController.height / 2 - _characterController.radius + _characterController.skinWidth;
        forceGravity = Mathf.Max(0, forceGravity);

        if (!isHitted || _isSliding)
            gravityVel += Physics.gravity * Time.fixedDeltaTime;
        else
            gravityVel = Physics.gravity * Time.fixedDeltaTime;

        gravityVel = Vector3.ClampMagnitude(gravityVel, 15f);

        return Vector3.down * forceGravity;
    }

    bool b = true;
    private void OnDrawGizmosSelected()
    {
        _characterController = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputs>();

        Vector3 origin = _characterController.bounds.center;
        float dist = _characterController.height / 2 + _characterController.radius + _characterController.skinWidth;

        bool isHitted = Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, dist, _layerGround, QueryTriggerInteraction.Ignore);

        Vector3 horizontalVel = transform.rotation * new Vector3(0f, 0f, 1f).normalized * Time.fixedDeltaTime * 4;
        if (!b)
            horizontalVel = Vector3.zero;

        dist = _characterController.height / 2 - _characterController.radius + _characterController.skinWidth + Physics.gravity.magnitude * Time.fixedDeltaTime;
        b = Physics.SphereCast(origin + horizontalVel, _characterController.radius, Vector3.down, out _groundInfo, dist, _layerGround, QueryTriggerInteraction.Ignore);
        Vector3 verticalVel = Vector3.down * (b ? _groundInfo.distance : dist);

        Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(origin + horizontalVel, _characterController.radius);

        Gizmos.DrawSphere(origin + horizontalVel + verticalVel, _characterController.radius);
    }

}
