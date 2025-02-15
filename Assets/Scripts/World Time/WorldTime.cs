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
            // ���������, ���������� �� ������, ���� ��� ���
            if (value.Minutes != currentTime.Minutes)
                OnMinuteChanged?.Invoke(value);
            if (value.Hours != currentTime.Hours)
                OnHourChanged?.Invoke(value);
            if (value.Days != currentTime.Days)
                OnDayChanged?.Invoke(value);
            currentTime = value;
        }
    }

    // ��������� ������� ������� 
    [field: SerializeField, Tooltip("���������� �����")] public bool TimePaused { get; private set; } = false;

    [SerializeField, Tooltip("�� ������� ������ �������� ���� ������� ������")] private float timeRate = 5f;
    [SerializeField, Tooltip("�������� �����?")] private bool IsSpeedingUpTime = false;
    [SerializeField, Tooltip("��������� �������� ������� ������� ��� ���������")] private float speedingUpTime = 100;

    public float TimeMultiplier
    {
        get => speedingUpTime;
        private set => speedingUpTime = Mathf.Clamp(value, 1, float.MaxValue);
    }

    // ������� ��� ����������� � ����� �������
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
            DontDestroyOnLoad(gameObject); // ����� ����� �� ������������ ��� ����� �����
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
    /// ������ �������� �������
    /// </summary>
    private IEnumerator TimerTick()
    {
        isTimerRunning = true;
        while (true)
        {
            if (!TimePaused)
            {
                // ������� �������
                CurrentTime += TimeSpan.FromMinutes(1);
            }

            Debug.Log(currentTime.ToString(@"dd\.hh\:mm"));
            if (IsSpeedingUpTime)
            {
                CurrentTime += TimeSpan.FromMinutes(1);
                yield return new WaitForSeconds(timeRate / speedingUpTime); // ��������� �������
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
    /// ���������, ������ �� ��������� ���������� ������� � ������� oldTime.
    /// </summary>
    public bool CheckTimeHasPassed(TimeSpan oldTime, TimeSpan timeSpan)
    {
        return currentTime - oldTime >= timeSpan;
    }

    /// <summary>
    /// ���������� ���������� ���������� ������� � ������� oldTime.
    /// </summary>
    public TimeSpan GetPassedTime(TimeSpan oldTime)
    {
        return currentTime - oldTime;
    }

    /// <summary>
    /// ������������� ������� �������� �������.
    /// </summary>
    public void PauseTime()
    {
        TimePaused = true; // ������������� �����
    }

    /// <summary>
    /// ������������ ������� �������� �������.
    /// </summary>
    public void ResumeTime()
    {
        TimePaused = false; // ������������ �����
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

        // ������� ������� ��� ����������� �������
        OnMinuteChanged = null;
        OnHourChanged = null;
        OnDayChanged = null;
    }
}