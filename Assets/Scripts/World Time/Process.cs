using System;
using System.Diagnostics;



public class Process : IComparable<Process>, IDisposable
{
    public TimeSpan Duration { get; private set; }
    public TimeSpan StartTime
    {
        get => _startTime;
        private set
        {
            _startTime = value;
            EndTime = _startTime + Duration;
        }

    }
    public TimeSpan EndTime { get; private set; }

    private Action _compliteAction;
    private Action<TimeSpan> _terminateAction;

    private TimeSpan _startTime = TimeSpan.Zero;
    private bool _disposed;

    public Process(TimeSpan duration, Action onAction, Action<TimeSpan> onTerminate = null)
    {
        Duration = duration;
        _compliteAction = onAction;
        _terminateAction = onTerminate;
    }

    public bool Pause()
    {
        if (_disposed)
            return false;

        if (!ProcessScheduler.Instance.RemoveProcess(this)) 
            return false;

        TimeSpan remaining = EndTime - WorldTime.Instance.CurrentTime;
        Duration = remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        StartTime = TimeSpan.Zero;
        return true;
    }

    public void Play()
    {
        if (_disposed)
            return;

        StartTime = WorldTime.Instance.CurrentTime;
        ProcessScheduler.Instance.AddProcess(this);
    }

    public bool Kill()
    {
        if (_disposed)
            return false;

        if (!ProcessScheduler.Instance.RemoveProcess(this))
            return false;

        TimeSpan remainsTime = EndTime - WorldTime.Instance.CurrentTime;
        _terminateAction?.Invoke(remainsTime);
        Dispose();

        return true;
    }

    public void Call()
    {
        if (_disposed)
            return;

        _compliteAction?.Invoke();
        Dispose();
    }

    public int CompareTo(Process other)
    {
        int comparison = EndTime.CompareTo(other.EndTime);
        if (comparison != 0)
            return comparison;
        return GetHashCode().CompareTo(other.GetHashCode());
    }

    public void Dispose()
    {
        if (_disposed) 
            return;

        _compliteAction = null;
        _terminateAction = null;
        _disposed = true;
    }
}


