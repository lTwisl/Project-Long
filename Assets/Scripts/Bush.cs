using UnityEngine;

public class Bush : Storage
{
    [SerializeField] private GameObject[] _objects;

    public override void Interact(Player player)
    {
        base.Interact(player);

        foreach (var obj in _objects)
        {
            obj.SetActive(false);
        }
    }
}
