using UnityEngine;

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
    public float Temp { get; private set; } = 20;
    public Vector3 Position => transform.position;
    public float MaxRadius { get; private set; }
    [field: SerializeField] public float MinRadius { get; private set; }

    private SphereCollider _sphereCollider;

    private void Awake()
    {
        _sphereCollider ??= GetComponent<SphereCollider>();
        MaxRadius = _sphereCollider.radius;
    }

    public override void Interact(Player player)
    {
        Debug.Log("Ineract");
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerStatusController psc))
            return;

        psc.AddExternalHeat(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out PlayerStatusController psc))
            return;

        psc.RemoveExternalHeat(this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _sphereCollider ??= GetComponent<SphereCollider>();

        MinRadius = Mathf.Clamp(MinRadius, 0f, _sphereCollider.radius);
    }

    private void OnDrawGizmosSelected()
    {
        _sphereCollider ??= GetComponent<SphereCollider>();

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(Position + _sphereCollider.center, MinRadius);
    }
#endif
}
