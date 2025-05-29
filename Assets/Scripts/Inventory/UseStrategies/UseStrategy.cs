using UnityEngine;
using Zenject;

public abstract class UseStrategy : ScriptableObject
{
    [Inject] protected Player _player;

    protected PlayerParameters _playerParameters;

    public virtual void Execute(InventorySlot slot) { }

    public virtual void Cancel() { }
}
