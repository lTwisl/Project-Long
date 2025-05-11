using UnityEngine;

public interface IMovementController
{
    public Vector3 Velocity { get; }
}

[SelectionBase, RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(PlayerInputs))]
public class KinematicMovementController : MonoBehaviour, IMovementController
{
    [SerializeField] private float _speed = 1f;
    [SerializeField] private int _maxBounces = 5;
    [SerializeField] private float _skinWidth = 0.015f;
    [SerializeField] private float _maxSlopeAngle = 55;

    [SerializeField] private bool _isGrounded;


    [Header("Debug")]
    //public bool debug = true;
    public Mesh meshCapsule;
    public LayerMask layerMask;
    public float distance = 5f;


    public Vector3 Velocity { get; private set; }

    private Rigidbody _rb;
    private Collider _collider;
    private PlayerInputs _input;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _input = GetComponent<PlayerInputs>();
    }

    private void FixedUpdate()
    {
        Move(transform.rotation * new Vector3(_input.move.x, 0f, _input.move.y).normalized * _speed, Physics.gravity);
        //Move(transform.rotation * new Vector3(0f, 0f, 1f).normalized, Physics.gravity);
    }

    public void Move(Vector3 inputVelocity, Vector3 gravity)
    {
        inputVelocity = CollideAndSlide(inputVelocity * Time.fixedDeltaTime, transform.position, 0, false, inputVelocity * Time.fixedDeltaTime);
        Debug.DrawLine(transform.position, transform.position + inputVelocity, Color.yellow, 10f);

        gravity = CollideAndSlide(gravity * Time.fixedDeltaTime, transform.position + inputVelocity, 0, true, gravity * Time.fixedDeltaTime);
        Debug.DrawLine(transform.position + inputVelocity, transform.position + inputVelocity + gravity, Color.red, 10f);

        Vector3 vel = inputVelocity + gravity;

        if (inputVelocity.magnitude > 0 && vel.magnitude > _speed * Time.fixedDeltaTime)
            vel = vel.normalized * _speed * Time.fixedDeltaTime;

        Debug.DrawLine(transform.position, transform.position + vel, Color.blue, 10f);

        Debug.Log(vel.magnitude / Time.fixedDeltaTime);
        _rb.MovePosition(transform.position + vel);
    }

    private Vector3 CollideAndSlide(Vector3 vel, Vector3 pos, int depth, bool gravityPass, Vector3 velInit)
    {
        if (depth >= _maxBounces)
            return Vector3.zero;

        float dist = vel.magnitude + _skinWidth;

        Vector3 p1 = pos + Vector3.up * (_collider.bounds.extents.x);
        Vector3 p2 = pos + Vector3.up * (2 * _collider.bounds.extents.y - _collider.bounds.extents.x);

        if (!Physics.CapsuleCast(p1, p2, _collider.bounds.extents.x, vel.normalized, out RaycastHit hitInfo, dist, layerMask))
        {
            Debug.DrawLine(pos, pos + vel, depth % 2 == 0 ? Color.red : Color.green);
            return vel;
        }

        Vector3 snapToSurface = vel.normalized * (hitInfo.distance - _skinWidth);
        if (snapToSurface.magnitude < _skinWidth)
        {
            pos -= snapToSurface;
            snapToSurface = Vector3.zero;
        }

        Vector3 leftover = vel - snapToSurface;
        float angle = Vector3.Angle(Vector3.up, hitInfo.normal);

        if (angle <= _maxSlopeAngle)
        {
            if (gravityPass)
            {
                Debug.DrawLine(pos, pos + snapToSurface, depth % 2 == 0 ? Color.red : Color.green);
                return snapToSurface;
            }

            leftover = ProjectAndScale(leftover, hitInfo.normal);
        }
        else
        {
            float scale = 1 - Vector3.Dot(
                new Vector3(hitInfo.normal.x, 0, hitInfo.normal.z).normalized,
                -new Vector3(velInit.x, 0, velInit.z).normalized
            );

            if (_isGrounded && !gravityPass)
            {
                leftover = ProjectAndScale(
                    new Vector3(leftover.x, 0, leftover.z),
                    new Vector3(hitInfo.normal.x, 0, hitInfo.normal.z)
                );
                leftover *= scale;
            }
            else
            {
                leftover = ProjectAndScale(leftover, hitInfo.normal) * scale;
            }
        }

        Debug.DrawLine(pos, pos + snapToSurface, depth % 2 == 0 ? Color.red : Color.green);

        return snapToSurface + CollideAndSlide(leftover, pos + snapToSurface, depth + 1, gravityPass, velInit);
    }

    private static Vector3 ProjectAndScale(Vector3 leftover, Vector3 normal)
    {
        float mag = leftover.magnitude;
        leftover = Vector3.ProjectOnPlane(leftover, normal).normalized;
        leftover *= mag;
        return leftover;
    }

    public Vector3 to;
    private void OnDrawGizmos()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        CollideAndSlide(transform.forward * distance, transform.position, 0, false, transform.forward * distance);
    }

}
