using System;
using UnityEngine;

public abstract class GameTimeYieldInstruction : CustomYieldInstruction
{
    public override bool keepWaiting => !_isCompleted;
    
    protected Action _handler;

    private bool _isCompleted;
    protected void Complete()
    {
        if (_isCompleted) 
            return;

        _isCompleted = true;
        Dispose();
    }

    public virtual void Dispose()
    {
        // Для безопасной отписки от событий
        if (_handler != null)
        {
            UnsubscribeFromEvent();
            _handler = null;
        }
    }

    protected abstract void UnsubscribeFromEvent();
}

// Ожидание одной игровой минуты
public sealed class WaitForNextMinute : GameTimeYieldInstruction
{
    public WaitForNextMinute()
    {
        _handler = Complete;

        GameTime.OnMinuteChanged += _handler;
    }

    protected override void UnsubscribeFromEvent() => GameTime.OnMinuteChanged -= _handler;
}

// Ожидание N игровых минут
public sealed class WaitForGameMinutes : GameTimeYieldInstruction
{
    public WaitForGameMinutes(float minutes)
    {
        TimeSpan _targetTime = GameTime.Time + TimeSpan.FromMinutes(minutes);

        _handler = () =>
        {
            if (_targetTime <= GameTime.Time)
                Complete();
        };

        GameTime.OnTimeChanged += _handler;
    }

    protected override void UnsubscribeFromEvent() => GameTime.OnMinuteChanged -= _handler;
}