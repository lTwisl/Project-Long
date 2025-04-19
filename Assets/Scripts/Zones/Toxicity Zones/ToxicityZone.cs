using UnityEditor;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Collider))]
public class ToxicityZone : MonoBehaviour
{
    [Inject] private World _world;

    public enum ZoneType
    {
        Rate,
        Single
    }

    [field: Header("Параметры обьекта:")]
    [SerializeField] private string _zoneID = "Toxicity Zone 1";
    [field: SerializeField, Min(0)] public float Toxicity { get; private set; }
    [SerializeField] private ZoneType _currentType;
    [DisableEdit, SerializeField] private Collider _collider;

    public string ZoneID => _zoneID;
    public ZoneType CurrentType => _currentType;


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _world.InvokeOnEnterToxicityZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _world.InvokeOnExitToxicityZone(this);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        ChangeNaming();
        CacheCollider();
    }

    private void ChangeNaming()
    {
        gameObject.name = $"[{_currentType}] {_zoneID}";
    }

    private void CacheCollider()
    {
        if (_collider == null)
            _collider = GetComponent<Collider>();

        if (_collider != null || _collider.isTrigger != true)
            _collider.isTrigger = true;
    }

    private void OnDrawGizmosSelected()
    {
        var color = _currentType == ZoneType.Rate ? new Color(0.5f, 0, 1, 1) : new Color(1, 0, 0.5f, 1);
        Gizmos.color = color;

        // Отрисовка значка зоны
        if (_collider != null)
        {
            Handles.color = color;
            Handles.DrawSolidDisc(transform.position, Vector3.up, Mathf.Max(_collider.bounds.extents.x, _collider.bounds.extents.y, _collider.bounds.extents.z));
        }

        // Отрисовка текстовой метки
        GUIStyle _guiStyle = new GUIStyle
        {
            normal = { textColor = Color.black },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        Handles.Label
        (
            transform.position + Vector3.up * 0.75f,
            $"ID: {_zoneID}\n{_currentType}",
            _guiStyle
        );
    }
#endif
}