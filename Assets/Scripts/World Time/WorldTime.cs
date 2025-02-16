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

            // ���������, ���������� �� ������, ���� ��� ���
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

    // ��������� ������� ������� 
    [field: SerializeField, Tooltip("���������� �����")] public bool TimePaused { get; private set; } = false;

    [SerializeField, Tooltip("������������ ��������� �������?")] private bool _useSpeedUp;
    [field: SerializeField, Tooltip("������������ �������� ������� ������� � ��������"), Range(1, 24)] public float timeScaleClassic { get; private set; } = 12f;
    [field: SerializeField, Tooltip("���������� �������� ������� ������� � ��������"), Range(12, 12000)] public float timeScaleSpeedUp { get; private set; } = 6000f;

    // ������� ��� ����������� � ����� �������
    public event Action<TimeSpan> OnTimeChanged;
    public event Action<TimeSpan> OnWaitingEnd;
    public event Action<TimeSpan> OnMinuteChanged;
    public event Action<TimeSpan> OnHourChanged;
    public event Action<TimeSpan> OnDayChanged;

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
            if (_useSpeedUp)
                CurrentTime = CurrentTime.Add(TimeSpan.FromSeconds(Time.deltaTime * timeScaleSpeedUp));
            else
                CurrentTime = CurrentTime.Add(TimeSpan.FromSeconds(Time.deltaTime * timeScaleClassic));

            //Debug.Log(_currentTime.ToString(@"dd\.hh\:mm\:ss"));
        }
    }

    /// <summary>
    /// �������� ������� ������� �� ����� ������ waitTime.
    /// </summary>
    /// <param name="waitTime">����� ������� ���� ���������</param>
    public void WaitTheTime(TimeSpan waitTime)
    {
        StartCoroutine(WaitTheTimeCor(waitTime));
    }

    private IEnumerator WaitTheTimeCor(TimeSpan time)
    {
        TimeSpan timeAfterWait = _currentTime + time;
        _useSpeedUp = true;

        while (_currentTime < timeAfterWait)
        {
            yield return null;
        }

        _useSpeedUp = false;
        OnWaitingEnd?.Invoke(CurrentTime);
    }

    /// <summary>
    /// �������� ����� �� ����������� waitTime
    /// </summary>
    /// <param name="waitTime">����� �� �������� ���� ���������</param>
    public void WaitTargetTime(TimeSpan waitTime)
    {
        StartCoroutine(WaitTargetTimeCor(waitTime));
    }

    private IEnumerator WaitTargetTimeCor(TimeSpan time)
    {
        if (time <= CurrentTime)
            yield return null;
        _useSpeedUp = true;

        while (_currentTime < time)
        {
            yield return null; // ���� ���������� �����
        }

        _useSpeedUp = false;
        OnWaitingEnd?.Invoke(CurrentTime);
    }

    /// <summary>
    /// ���������, ������ �� ��������� ���������� ������� � ������� oldTime.
    /// </summary>
    public bool CheckTimeHasPassed(TimeSpan oldTime, TimeSpan timeSpan)
    {
        return _currentTime - oldTime >= timeSpan;
    }

    /// <summary>
    /// ���������� ���������� ���������� ������� � ������� oldTime.
    /// </summary>
    public TimeSpan GetPassedTime(TimeSpan oldTime)
    {
        return _currentTime - oldTime;
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
        OnTimeChanged = null;
        OnWaitingEnd = null;
    }
}