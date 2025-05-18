using System.Collections.Generic;

namespace ImprovedTimers
{
    public static class TimerManager
    {
        private static readonly List<Timer> _timers = new();

        public static void RegisterTimer(Timer timer) => _timers.Add(timer);
        public static void DeregisterTimer(Timer timer) => _timers.Remove(timer);

        public static void UpdateTimers()
        {
            for (int i = 0; i < _timers.Count; i++)
                _timers[i].Tick();
        }

        public static void Clear()
        {
            _timers.Clear();
        }
    }
}
