using System;
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameTime : MonoBehaviour
{
    [SerializeField, HideInInspector] private int _initDays, _initHours, _initMinutes;

    private TimeSpan _currentTime;
    [SerializeField] private bool _useSpeedUp;
    [SerializeField] private bool _isTimeStopped;
    [SerializeField, Range(1, 48)] private float _timeScale = 12;
    [SerializeField, Range(24, 12000)] private float _speedUpTimeScale = 96;

    // Статический API
    public static TimeSpan Time => _instance == null ? TimeSpan.Zero : _instance._currentTime;
    public static float TimeScale => _instance == null ? 12 : _instance._timeScale;
    public static float SpeedUpTimeScale => _instance == null ? 96 : _instance._speedUpTimeScale;
    public static bool IsTimeStopped => _instance == null ? false : _instance._isTimeStopped;
    public static bool UseSpeedUp => _instance == null ? false : _instance._useSpeedUp;
    public static float DeltaTime => IsTimeStopped ? 0f : (UseSpeedUp ? SpeedUpTimeScale : TimeScale) * UnityEngine.Time.deltaTime;

    public static event Action OnTimeChanged;
    public static event Action OnLateTimeChanged;
    public static event Action OnMinuteChanged;
    public static event Action OnHourChanged;
    public static event Action OnDayChanged;
    public static event Action OnSpeedUpStarted;
    public static event Action OnSpeedUpEnded;

    private static GameTime _instance;
    private Coroutine _activeSpeedUp;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        _currentTime = new TimeSpan(_initDays, _initHours, _initMinutes, 0);
    }

    private void Update()
    {
        if (IsTimeStopped)
            return;
        
        var newTime = Time.Add(TimeSpan.FromSeconds(DeltaTime));
        UpdateTime(newTime);
    }

    private void OnDestroy()
    {
        if (_instance == null)
            return;

        _instance = null;

        OnTimeChanged = null;
        OnMinuteChanged = null;
        OnHourChanged = null;
        OnDayChanged = null;
        OnSpeedUpStarted = null;
        OnSpeedUpEnded = null;
    }

    private void UpdateTime(TimeSpan newTime)
    {
        TimeSpan oldTime = Time;
        TimeSpan delta = newTime - oldTime;

        if (delta <= TimeSpan.Zero)
            return;
        int i = 0;
        TimeSpan current = oldTime;
        while (current < newTime)
        {
            TimeSpan next = current.Add(TimeSpan.FromMinutes(1));
            if (next > newTime) next = newTime;

            if (current.Minutes != next.Minutes)
            {

                OnMinuteChanged?.Invoke();
            }

            if (current.Hours != next.Hours)
                OnHourChanged?.Invoke();

            if (current.Days != next.Days)
                OnDayChanged?.Invoke();


            current = next;
            ++i;
        }
        //Debug.Log(i);
        _currentTime = newTime;
        OnTimeChanged?.Invoke();
        OnLateTimeChanged?.Invoke();
    }

    public static void StartSpeedUp(TimeSpan duration)
    {
        if (_instance == null)
        {
            Debug.LogError("GameTime instance not found!");
            return;
        }

        StopAllSpeedUps();
        _instance.StartCoroutine(_instance.SpeedUpCoroutine(duration));
    }

    public static void StartSpeedUpUntil(TimeSpan targetTime)
    {
        if (_instance == null)
        {
            Debug.LogError("GameTime instance not found!");
            return;
        }

        StopAllSpeedUps();
        _instance.StartCoroutine(_instance.SpeedUpUntilCoroutine(targetTime));
    }

    public static void StopAllSpeedUps()
    {
        if (_instance != null && _instance._activeSpeedUp != null)
        {
            _instance.StopCoroutine(_instance._activeSpeedUp);
            EndSpeedUp();
        }
    }

    private IEnumerator SpeedUpCoroutine(TimeSpan duration)
    {
        _useSpeedUp = true;
        OnSpeedUpStarted?.Invoke();

        var startTime = Time;
        while (Time - startTime < duration)
        {
            yield return null;
        }

        EndSpeedUp();
    }

    private IEnumerator SpeedUpUntilCoroutine(TimeSpan targetTime)
    {
        _useSpeedUp = true;
        OnSpeedUpStarted?.Invoke();

        while (Time < targetTime)
        {
            yield return null;
        }

        EndSpeedUp();
    }

    private static void EndSpeedUp()
    {
        _instance._useSpeedUp = false;
        OnSpeedUpEnded?.Invoke();
    }

    /// <summary>
    /// Возвращает количество прошедшего времени с момента oldTime.
    /// </summary>
    public static TimeSpan GetPassedTime(TimeSpan oldTime)
    {
        return Time - oldTime;
    }

    public static bool HasTimePassed(TimeSpan checkTime, TimeSpan requiredDuration)
        => Time - checkTime >= requiredDuration;

    public static string GetFormattedTime(string format = "d':'hh':'mm':'ss")
        => $"Day {Time.Days}: {Time.ToString(format)}";

    /// <summary>
    /// Возвращает текущее время в формате "День X: HH:MM:SS".
    /// </summary>
    public static string GetFormattedTime(TimeSpan time)
    {
        //return $"День {time.Days}: {time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds:00}";
        return $"День {time.Days}: {time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
        //return $"День {time.Days}: {time.Hours:00}:{time.Minutes:00}";
    }

    public static IEnumerator YieldNull()
    {
        do
            yield return null;
        while (IsTimeStopped);
    }
}