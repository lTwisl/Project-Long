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

            // ����������� ���� �� ����������� �������
            OnTimeChanged?.Invoke(value);
            if (value.Minutes != _currentTime.Minutes) OnMinuteChanged?.Invoke(value);
            if (value.Hours != _currentTime.Hours) OnHourChanged?.Invoke(value);
            if (value.Days != _currentTime.Days) OnDayChanged?.Invoke(value);

            _currentTime = value;
        }
    }


    [SerializeField, Tooltip("������������ ��������� �������?")] private bool _IsTimeSpeedUp;
    [field: SerializeField, Tooltip("����� �����������?")] public bool IsTimeStopped { get; private set; } = false;
    [field: SerializeField, Tooltip("������� �������� ������� ������� � ��������"), Range(1, 48)] public float TimeScaleGame { get; private set; } = 12f;
    [field: SerializeField, Tooltip("������� ���������� �������� ������� ������� � ��������"), Range(24, 12000)] public float TimeScaleSpeedUp { get; private set; } = 6000f;

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
    /// �������� ������� �������� ������� �� ����� ������ waitTime.
    /// </summary>
    /// <param name="waitTime">�����, ������� ���������� ����� ������� ����� ��� ���������</param>
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
    /// �������� ������� �������� ������� �� ����������� waitTime
    /// </summary>
    /// <param name="waitTime">����� �� �������� ���� ���������</param>
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
    /// �������� ��������� �������
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
    /// ���������, ������ �� ��������� ���������� ������� � ������� oldTime.
    /// </summary>
    public bool CheckTimeHasPassed(TimeSpan oldTime, TimeSpan checkingTime)
    {
        return _currentTime - oldTime >= checkingTime;
    }

    /// <summary>
    /// ���������� ���������� ���������� ������� � ������� oldTime.
    /// </summary>
    public TimeSpan GetPassedTime(TimeSpan oldTime)
    {
        return _currentTime - oldTime;
    }

    /// <summary>
    /// ���������� ����� �� ����������� ���������� �������.
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
    /// ���������� ������� ����� � ������� "���� X, HH:MM".
    /// </summary>
    public string GetFormattedTime(TimeSpan time)
    {
        return $"���� {time.Days}, {time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
    }

    /// <summary>
    /// ���������� ������� �������� �������.
    /// </summary>
    public void StopTime()
    {
        IsTimeStopped = true;
    }

    /// <summary>
    /// ����������� ������� �������� �������.
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