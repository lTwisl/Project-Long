using UnityEditor;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(SphereCollider))]
public class TemperatureZone : MonoBehaviour
{
    [Inject] private World _world;

    [field: SerializeField, Min(0f)] public float Temperature { get; private set; }
    [field: SerializeField] public float MaxRadius { get; private set; }
    [field: SerializeField] public float MinRadius { get; private set; }

    private SphereCollider _sphereCollider;
    private bool _isIn = false;

    private void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Physics.Raycast(transform.position, (other.transform.position - transform.position).normalized, MaxRadius))
            return;

        Debug.Log("Enter");

        _isIn = true;
        _world.InvokeOnEnterTemperatureZone(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        bool newIsIn = !Physics.Raycast(transform.position, (other.transform.position - transform.position).normalized, out RaycastHit hitInfo, MaxRadius);
        Debug.Log(hitInfo.transform?.name);
        if (_isIn && !newIsIn)
        {
            _isIn = false;
            _world.InvokeOnExitTemperatureZone(this);
            Debug.Log("Exit");
        }
        else if (!_isIn && newIsIn)
        {
            _isIn = true;
            _world.InvokeOnEnterTemperatureZone(this);
            Debug.Log("Enter");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || !_isIn)
            return;
        Debug.Log("Exit");
        _isIn = false;
        _world.InvokeOnExitTemperatureZone(this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _sphereCollider ??= GetComponent<SphereCollider>();

        _sphereCollider.radius = Mathf.Max(0, MaxRadius);
        MinRadius = Mathf.Clamp(MinRadius, 0f, _sphereCollider.radius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position + _sphereCollider.center, MinRadius);
    }
#endif
}
