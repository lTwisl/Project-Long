using UnityEngine;
using Zenject;


// Базовый класс для стратегий использования предметов.
public abstract class UseStrategy : ScriptableObject
{
    // Параметры игрока, изменяемые стратегией.
    protected PlayerParameters PlayerParameters { get; private set; }

    [Inject]
    private void Construct(PlayerParameters playerParameters)
    {
        PlayerParameters = playerParameters;
    }


    // Выполняет действие при использовании предмета.
    public virtual void Execute(InventorySlot slot) { }


    // Отменяет действие стратегии.
    public virtual void Cancel() { }
}