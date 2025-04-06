using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-100)]
public class ProcessScheduler : MonoBehaviour
{
    public static ProcessScheduler Instance { get; private set; }

    private SortedSet<Process> _waitingProcesses = new SortedSet<Process>();

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

    public void AddProcess(Process process)
    {
        if (process.StartTime == TimeSpan.Zero)
        {
            process.Play();
            return;
        }

        _waitingProcesses.Add(process);

        if (_waitingProcesses.Count == 1)
            StartCoroutine(WaitProcesses());
    }

    public bool RemoveProcess(Process process)
    {
        return _waitingProcesses.Remove(process);
    }

    public IEnumerator WaitProcesses()
    {
        while (_waitingProcesses.Count > 0)
        {
            // Ожидать, пока текущее время не достигнет EndTime текущего Min
            yield return new WaitWhile(() =>
                _waitingProcesses.Count > 0 &&
                GameTime.Time < _waitingProcesses.Min.EndTime);

            if (_waitingProcesses.Count == 0) yield break;

            var minProcess = _waitingProcesses.Min;
            // Проверка, что процесс всё ещё в коллекции
            if (_waitingProcesses.Contains(minProcess))
            {
                minProcess.Call();
                RemoveProcess(minProcess);
            }
        }
    }
}
