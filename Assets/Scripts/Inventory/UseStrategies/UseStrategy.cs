using UnityEngine;

public abstract class UseStrategy : ScriptableObject
{
    public abstract void Execute(InventoryItem item, Player player);
}
