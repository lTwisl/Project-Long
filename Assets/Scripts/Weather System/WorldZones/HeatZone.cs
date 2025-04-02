using EditorAttributes;
using UnityEditor;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(SphereCollider))]
public class HeatZone : MonoBehaviour
{
    [Inject] private World _world;

    public float Temp { get; private set; }
    [field: SerializeField] public float MaxRadius { get; private set; }
    [field: SerializeField] public float MinRadius { get; private set; }

    private SphereCollider _sphereCollider;

    private void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _world.InvokeOnEnterHeatZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _world.InvokeOnExitHeatZone(this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_sphereCollider == null)
        {
            _sphereCollider = GetComponent<SphereCollider>();
            Undo.RecordObject(this, "Инициализация ссылки на SphereCollider");
            EditorUtility.SetDirty(this);
        }

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
