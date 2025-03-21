using UnityEngine;
using Zenject;

public abstract class UseStrategy : ScriptableObject
{
    [Inject] protected Player _player;

    public abstract void Execute(InventoryItem item);
}
