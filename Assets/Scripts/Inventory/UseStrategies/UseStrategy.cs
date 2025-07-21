using System;
using UnityEngine;
using Zenject;


// Базовый класс для стратегий использования предметов.
public abstract class UseStrategy : ScriptableObject
{
    // Параметры игрока, изменяемые стратегией.
    protected Player _player;
    protected PlayerParameters PlayerParameters => _player.Parameters;

    protected void OnUsageCompleted(in UsageItemInfo @is)
    {
        Debug.Log("OnUsageCompleted");
    }

    [Inject]
    private void Construct(Player player)
    {
        _player = player;
        //_playerParameters = playerParameters;
    }


    // Выполняет действие при использовании предмета.
    public virtual void Execute(InventorySlot slot) { }


    // Отменяет действие стратегии.
    public virtual void Cancel() { }
}