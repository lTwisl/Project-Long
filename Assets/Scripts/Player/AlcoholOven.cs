using UnityEditor;
using UnityEngine;
using Zenject;

public interface IExternalHeat
{ 
    public float Temp { get; }
    public Vector3 Position { get; }
    public float MaxRadius { get; }
    public float MinRadius { get; }
}


[SelectionBase, RequireComponent(typeof(SphereCollider))]
public class AlcoholOven : WorldItem, IExternalHeat
{
    [Inject] private World _world;

    public float Temp { get; private set; } = 20;
    public Vector3 Position => transform.position;
    [field: SerializeField] public float MaxRadius { get; private set; }
    [field: SerializeField] public float MinRadius { get; private set; }

    private SphereCollider _sphereCollider;

    private void Awake()
    {
        _sphereCollider ??= GetComponent<SphereCollider>();
    }

    public override void Interact(Player player)
    {
        Debug.Log("Ineract");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Player player))
            return;

        _world.AddExternalHeat(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out Player player))
            return;

        _world.RemoveExternalHeat(this);
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

        Gizmos.DrawWireSphere(Position + _sphereCollider.center, MinRadius);
    }
#endif
}
