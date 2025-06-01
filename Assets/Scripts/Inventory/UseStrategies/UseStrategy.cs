using UnityEngine;
using Zenject;


// Базовый класс для стратегий использования предметов.
public abstract class UseStrategy : ScriptableObject
{
    // Ссылка на игрока, которому принадлежит стратегия.
    protected Player Player { get; private set; }

    // Параметры игрока, изменяемые стратегией.
    protected PlayerParameters PlayerParameters { get; private set; }


    // Инициализация зависимости через Zenject.
    [Inject]
    private void Construct(Player player)
    {
        Player = player ?? throw new System.NullReferenceException("Player не может быть null");

        if (!Player.TryGetComponent(out PlayerStatusManager statusManager))
        {
            throw new System.NullReferenceException("PlayerStatusManager не найден у игрока");
        }

        PlayerParameters = statusManager.PlayerParameters;
    }


    // Выполняет действие при использовании предмета.
    public virtual void Execute(InventorySlot slot) { }


    // Отменяет действие стратегии.
    public virtual void Cancel() { }
}