using UnityEngine;


[SelectionBase, RequireComponent(typeof(SphereCollider))]
public class AlcoholOven : WorldItem
{
    public override void Interact(Player player)
    {
        Debug.Log("Ineract");
    }
}
