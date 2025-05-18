using System;
using UnityEngine;

namespace ImprovedTimers
{
    public abstract class Timer : IDisposable
    {
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; protected set; }

        protected float _initialTime;

        public float Progress => Mathf.Clamp01(CurrentTime / _initialTime);

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };

        private bool _disposed;

        protected Timer(float initialTime)
        {
            _initialTime = initialTime;
        }

        public void Start()
        {
            CurrentTime = _initialTime;
            if (IsRunning)
                return;

            IsRunning = true;
            TimerManager.RegisterTimer(this);
            OnTimerStart.Invoke();
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            TimerManager.DeregisterTimer(this);
            OnTimerStop.Invoke();
        }

        public abstract void Tick();
        public abstract bool IsFinished { get; }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public virtual void Reset() => CurrentTime = _initialTime;
        public virtual void Reset(float newInitialTime)
        {
            _initialTime = newInitialTime;
            Reset();
        }

        ~Timer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                TimerManager.DeregisterTimer(this);

            _disposed = true;
        }
    }
}
