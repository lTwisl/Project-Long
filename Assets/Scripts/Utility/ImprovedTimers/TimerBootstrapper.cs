using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityUtils.LowLevel;

namespace ImprovedTimers
{
    internal static class TimerBootstrapper
    {
        private static PlayerLoopSystem _timerSystem;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!InsertTimerManager<Update>(ref currentPlayerLoop, 0))
            {
                Debug.LogWarning("Improved Timers not initialized, unable to register TimerManager onto the Update loop.");
                return;
            }
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            //PlayerLoopUtils.PrintPlayerLoop(currentPlayerLoop);

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;
#endif

            static void OnPlayModeState(PlayModeStateChange state)
            {
                if (state != PlayModeStateChange.ExitingPlayMode) 
                    return;

                PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                RemoveTimerManager<Update>(ref currentPlayerLoop);
                PlayerLoop.SetPlayerLoop(currentPlayerLoop);

                TimerManager.Clear();
            }
        }

        private static void RemoveTimerManager<T>(ref PlayerLoopSystem loop)
        {
            PlayerLoopUtils.RemoveSystem<T>(ref loop, in _timerSystem);
        }

        static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
        {
            _timerSystem = new PlayerLoopSystem()
            {
                type = typeof(TimerManager),
                updateDelegate = TimerManager.UpdateTimers,
                subSystemList = null
            };
            return PlayerLoopUtils.InsertSystem<T>(ref loop, in _timerSystem, index);
        }
    }
}
