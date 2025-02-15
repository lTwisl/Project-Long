using System;
using System.Collections;
using UnityEngine;

public class WorldTime : MonoBehaviour
{
    public static WorldTime Instance { get; private set; }

    [Header("Current Time:")]
    private TimeSpan currentTime = new TimeSpan(1, 0, 0, 0);
    public TimeSpan CurrentTime
    {
        get => currentTime;
        private set
        {
            if (currentTime == value)
                return;

            OnTimeChanged?.Invoke(value);
            // Проверяем, изменились ли минуты, часы или дни
            if (value.Minutes != currentTime.Minutes)
                OnMinuteChanged?.Invoke(value);
            if (value.Hours != currentTime.Hours)
                OnHourChanged?.Invoke(value);
            if (value.Days != currentTime.Days)
                OnDayChanged?.Invoke(value);
            currentTime = value;
        }
    }

    // Параметры течения времени 
    [field: SerializeField, Tooltip("Остановить время")] public bool TimePaused { get; private set; } = false;

    [SerializeField, Tooltip("За сколько секунд проходит одна игровая минута")] private float timeRate = 5f;
    [SerializeField, Tooltip("Ускорить время?")] private bool IsSpeedingUpTime = false;
    [SerializeField, Tooltip("Множитель скорости течения времени при ускорении")] private float speedingUpTime = 100;

    public float TimeMultiplier
    {
        get => speedingUpTime;
        private set => speedingUpTime = Mathf.Clamp(value, 1, float.MaxValue);
    }

    // События для уведомления о смене времени
    public event Action<TimeSpan> OnTimeChanged;
    public event Action<TimeSpan> OnMinuteChanged;
    public event Action<TimeSpan> OnHourChanged;
    public event Action<TimeSpan> OnDayChanged;

    private bool isTimerRunning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Чтобы время не сбрасывалось при смене сцены
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(TimerTick());
    }

    /// <summary>
    /// Таймер игрового времени
    /// </summary>
    private IEnumerator TimerTick()
    {
        isTimerRunning = true;
        while (true)
        {
            if (!TimePaused)
            {
                // Течение времени
                CurrentTime += TimeSpan.FromMinutes(1);
            }

            Debug.Log(currentTime.ToString(@"dd\.hh\:mm"));
            if (IsSpeedingUpTime)
            {
                CurrentTime += TimeSpan.FromMinutes(1);
                yield return new WaitForSeconds(timeRate / speedingUpTime); // Ускорение времени
            }
            else
            {
                CurrentTime += TimeSpan.FromMinutes(1);
                yield return new WaitForSeconds(timeRate);
            }
        }
    }

    public void WaitTime(TimeSpan waitTime)
    {
        StartCoroutine(SpeedUpTime(waitTime));
    }

    private IEnumerator SpeedUpTime(TimeSpan time)
    {
        TimeSpan timeAfterWait = currentTime + time;
        IsSpeedingUpTime = true;
        while (currentTime < timeAfterWait)
        {
            yield return new WaitForSeconds(timeRate / 60 / speedingUpTime);
        }
        IsSpeedingUpTime = false;
        yield return null;
    }

    /// <summary>
    /// Проверяет, прошло ли указанное количество времени с момента oldTime.
    /// </summary>
    public bool CheckTimeHasPassed(TimeSpan oldTime, TimeSpan timeSpan)
    {
        return currentTime - oldTime >= timeSpan;
    }

    /// <summary>
    /// Возвращает количество прошедшего времени с момента oldTime.
    /// </summary>
    public TimeSpan GetPassedTime(TimeSpan oldTime)
    {
        return currentTime - oldTime;
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
        if (!isTimerRunning)
        {
            StartCoroutine(TimerTick());
        }
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
    }
}