using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTime : MonoBehaviour
{
    public static WorldTime Instance { get; private set; }

    private TimeSpan _currentTime = new TimeSpan(1, 8, 0, 0);
    public TimeSpan CurrentTime
    {
        get => _currentTime;
        set
        {
            if (_currentTime == value)
                return;

            // Информируем всех об измененении времени
            OnTimeChanged?.Invoke(value);
            if (value.Minutes != _currentTime.Minutes) OnMinuteChanged?.Invoke(value);
            if (value.Hours != _currentTime.Hours) OnHourChanged?.Invoke(value);
            if (value.Days != _currentTime.Days) OnDayChanged?.Invoke(value);

            _currentTime = value;
        }
    }


    [SerializeField, Tooltip("Использовать ускорение времени?")] private bool _IsTimeSpeedUp;
    [field: SerializeField, Tooltip("Время остановлено?")] public bool IsTimeStopped { get; private set; } = false;
    [field: SerializeField, Tooltip("Игровая скорость течения времени к реальной"), Range(1, 48)] public float TimeScaleGame { get; private set; } = 12f;
    [field: SerializeField, Tooltip("Игровая ускоренная скорость течения времени к реальной"), Range(24, 12000)] public float TimeScaleSpeedUp { get; private set; } = 6000f;

    public float TimeScale => _IsTimeSpeedUp ? TimeScaleSpeedUp : TimeScaleGame;
    public float DeltaTime => Time.deltaTime * TimeScale;
    public float FixedDeltaTime => Time.fixedDeltaTime * TimeScale;


    public event Action<TimeSpan> OnTimeChanged;
    public event Action<TimeSpan> OnMinuteChanged;
    public event Action<TimeSpan> OnHourChanged;
    public event Action<TimeSpan> OnDayChanged;
    public event Action<TimeSpan> OnStartSpeedUpTime;
    public event Action<TimeSpan> OnEndSpeedUpTime;

    private Coroutine _waitCoroutine;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (IsTimeStopped) return;

        CurrentTime += TimeSpan.FromSeconds(Time.deltaTime * TimeScale);
    }

    /// <summary>
    /// Ускорить течение игрового времени на время равное waitTime.
    /// </summary>
    /// <param name="waitTime">Время, которое необходимо чтобы игровое время шло ускоренно</param>
    public void WaitTheTime(TimeSpan waitTime)
    {
        StopWaitTime();
        _waitCoroutine = StartCoroutine(WaitTheTimeCor(waitTime));
    }

    private IEnumerator WaitTheTimeCor(TimeSpan time)
    {
        TimeSpan timeAfterWait = _currentTime + time;
        OnStartSpeedUpTime?.Invoke(CurrentTime);
        _IsTimeSpeedUp = true;

        yield return new WaitWhile(() => _currentTime < timeAfterWait);

        _IsTimeSpeedUp = false;
        OnEndSpeedUpTime?.Invoke(CurrentTime);
    }

    /// <summary>
    /// Ускорить течение игрового времени до наступления waitTime
    /// </summary>
    /// <param name="waitTime">Время до которого надо подождать</param>
    public void WaitUntilTime(TimeSpan waitTime)
    {
        StopWaitTime();
        _waitCoroutine = StartCoroutine(WaitUntilTimeCor(waitTime));
    }

    private IEnumerator WaitUntilTimeCor(TimeSpan time)
    {
        if (time <= CurrentTime)
            yield break;

        OnStartSpeedUpTime?.Invoke(CurrentTime);
        _IsTimeSpeedUp = true;

        yield return new WaitWhile(() => _currentTime < time);

        _IsTimeSpeedUp = false;
        OnEndSpeedUpTime?.Invoke(CurrentTime);
    }

    /// <summary>
    /// Прервать ускорение времени
    /// </summary>
    public void StopWaitTime()
    {
        if (_waitCoroutine is not null)
        {
            StopCoroutine(_waitCoroutine);
            OnEndSpeedUpTime?.Invoke(CurrentTime);
            _IsTimeSpeedUp = false;
        }
    }

    /// <summary>
    /// Проверяет, прошло ли указанное количество времени с момента oldTime.
    /// </summary>
    public bool CheckTimeHasPassed(TimeSpan oldTime, TimeSpan checkingTime)
    {
        return _currentTime - oldTime >= checkingTime;
    }

    /// <summary>
    /// Возвращает количество прошедшего времени с момента oldTime.
    /// </summary>
    public TimeSpan GetPassedTime(TimeSpan oldTime)
    {
        return _currentTime - oldTime;
    }

    /// <summary>
    /// Возвращает время до наступления указанного времени.
    /// </summary>
    /// <param name="targetTime"></param>
    /// <returns></returns>
    public TimeSpan GetTimeUntil(TimeSpan targetTime)
    {
        if (targetTime > CurrentTime)
        {
            return targetTime - CurrentTime;
        }
        else
        {
            return new TimeSpan(1, 0, 0, 0) - (CurrentTime - targetTime);
        }
    }

    /// <summary>
    /// Возвращает текущее время в формате "День X, HH:MM".
    /// </summary>
    public string GetFormattedTime(TimeSpan time)
    {
        return $"День {time.Days}, {time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
    }

    /// <summary>
    /// Остановить течение игрового времени.
    /// </summary>
    public void StopTime()
    {
        IsTimeStopped = true;
    }

    /// <summary>
    /// Возобновить течение игрового времени.
    /// </summary>
    public void ResumeTime()
    {
        IsTimeStopped = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}