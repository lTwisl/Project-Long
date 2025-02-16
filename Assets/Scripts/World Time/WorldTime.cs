using System;
using System.Collections;
using UnityEngine;

public class WorldTime : MonoBehaviour
{
    public static WorldTime Instance { get; private set; }

    private TimeSpan _currentTime = new TimeSpan(1, 2, 0, 0);
    public TimeSpan CurrentTime
    {
        get => _currentTime;
        set
        {
            if (_currentTime == value)
                return;

            // Проверяем, изменились ли минуты, часы или дни
            OnTimeChanged?.Invoke(value);
            if (value.Minutes != _currentTime.Minutes)
                OnMinuteChanged?.Invoke(value);
            if (value.Hours != _currentTime.Hours)
                OnHourChanged?.Invoke(value);
            if (value.Days != _currentTime.Days)
                OnDayChanged?.Invoke(value);
            _currentTime = value;
        }
    }

    // Параметры течения времени 
    [field: SerializeField, Tooltip("Остановить время")] public bool TimePaused { get; private set; } = false;

    [SerializeField, Tooltip("Использовать ускорение времени?")] private bool _useSpeedUp;
    [field: SerializeField, Tooltip("Классическая скорость течения времени к реальной"), Range(1, 24)] public float TimeScaleClassic { get; private set; } = 12f;
    [field: SerializeField, Tooltip("Ускоренная скорость течения времени к реальной"), Range(12, 12000)] public float TimeScaleSpeedUp { get; private set; } = 6000f;
    public float TimeScale => _useSpeedUp ? TimeScaleSpeedUp : TimeScaleClassic;

    // События для уведомления о смене времени
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!TimePaused)
        {
            CurrentTime += TimeSpan.FromSeconds(Time.deltaTime * TimeScale);
        }
    }

    /// <summary>
    /// Ускорить течение времени на время равное waitTime.
    /// </summary>
    /// <param name="waitTime">Время которое надо подождать</param>
    public void WaitTheTime(TimeSpan waitTime)
    {
        StopWaitTime();
        _waitCoroutine = StartCoroutine(WaitTheTimeCor(waitTime));
    }

    private IEnumerator WaitTheTimeCor(TimeSpan time)
    {
        TimeSpan timeAfterWait = _currentTime + time;
        OnStartSpeedUpTime?.Invoke(CurrentTime);
        _useSpeedUp = true;

        yield return new WaitWhile(() => _currentTime < timeAfterWait);

        _useSpeedUp = false;
        OnEndSpeedUpTime?.Invoke(CurrentTime);
    }

    /// <summary>
    /// Ускорить время до наступления waitTime
    /// </summary>
    /// <param name="waitTime">Время до которого надо подождать</param>
    public void WaitTargetTime(TimeSpan waitTime)
    {
        StopWaitTime();
        _waitCoroutine = StartCoroutine(WaitTargetTimeCor(waitTime));
    }

    private IEnumerator WaitTargetTimeCor(TimeSpan time)
    {
        if (time <= CurrentTime)
            yield break;

        OnStartSpeedUpTime?.Invoke(CurrentTime);
        _useSpeedUp = true;

        yield return new WaitWhile(() => _currentTime < time);

        _useSpeedUp = false;
        OnEndSpeedUpTime?.Invoke(CurrentTime);
    }

    public void StopWaitTime()
    {
        if (_waitCoroutine is not null)
        {
            StopCoroutine(_waitCoroutine);
            OnEndSpeedUpTime?.Invoke(CurrentTime);
            _useSpeedUp = false;
        }    
    }

    /// <summary>
    /// Проверяет, прошло ли указанное количество времени с момента oldTime.
    /// </summary>
    public bool CheckTimeHasPassed(TimeSpan oldTime, TimeSpan timeSpan)
    {
        return _currentTime - oldTime >= timeSpan;
    }

    /// <summary>
    /// Возвращает количество прошедшего времени с момента oldTime.
    /// </summary>
    public TimeSpan GetPassedTime(TimeSpan oldTime)
    {
        return _currentTime - oldTime;
    }

    /// <summary>
    /// Останавливает течение игрового времени.
    /// </summary>
    public void PauseTime()
    {
        TimePaused = true; // Останавливаем время
    }

    /// <summary>
    /// Возобновляет течение игрового времени.
    /// </summary>
    public void ResumeTime()
    {
        TimePaused = false; // Возобновляем время
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Очистка событий при уничтожении объекта
        OnMinuteChanged = null;
        OnHourChanged = null;
        OnDayChanged = null;
        OnTimeChanged = null;

        OnStartSpeedUpTime = null;
        OnEndSpeedUpTime = null;
    }
}