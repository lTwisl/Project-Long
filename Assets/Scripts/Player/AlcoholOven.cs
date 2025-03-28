using UnityEditor;
using UnityEngine;
using Zenject;


[SelectionBase, RequireComponent(typeof(SphereCollider))]
public class AlcoholOven : WorldItem
{
    

    public float Temp { get; private set; } = 20;
    public Vector3 Position => transform.position;
    [field: SerializeField] public float MaxRadius { get; private set; }
    [field: SerializeField] public float MinRadius { get; private set; }

    

    public override void Interact(Player player)
    {
        Debug.Log("Ineract");
    }

    


}
