using System.Collections.Generic;
using UnityEngine;

public class WeatherVFXSystem : MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; }

    [field: Header("- - VFX Controllers:")]
    [field: SerializeField] public List<VFXController> CurrentVFXControllers { get; private set; } = new();
    [field: SerializeField] public List<VFXController> NextVFXControllers { get; private set; } = new();

    public void InitializeAndValidateSystem()
    {
        IsSystemValid = true;
    }

    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        // 1. Обновляем старые VFX Controllers (исчезновение):
        foreach (VFXController vfxController in CurrentVFXControllers)
            if (vfxController && vfxController.IsControllerValid())
                vfxController.VFXGraph.SetFloat("t", 1f - t);

        // 2. Обновляем новые VFX Controllers (появление):
        foreach (VFXController vfxController in NextVFXControllers)
            if (vfxController && vfxController.IsControllerValid())
                vfxController.VFXGraph.SetFloat("t", t);

        // 3. Удаляем исчезнувшие и актуализируем списки:
        if (t >= 1)
        {
            // 3.1. Удаляем старые VFX Controllers
            foreach (VFXController vfxController in CurrentVFXControllers)
                if (vfxController)
                    vfxController.Destroy();
            CurrentVFXControllers.Clear();

            // 3.2. Актуализируем старые и новые VFX Controllers
            foreach (VFXController vfxController in NextVFXControllers)
                if (vfxController)
                    CurrentVFXControllers.Add(vfxController);
            NextVFXControllers.Clear();
        }
    }

    public void PreSpawnVFXControllers(WeatherProfile nextProfile)
    {
        if (!nextProfile) return;

        foreach (GameObject vfxPrefab in nextProfile.VFX)
            if (vfxPrefab)
                NextVFXControllers.Add(Instantiate(vfxPrefab, transform.position, Quaternion.identity, transform).GetComponent<VFXController>());
    }

    public void ClearVFXControllers()
    {
        foreach (VFXController vfxController in CurrentVFXControllers)
            if (vfxController)
                vfxController.Destroy();

        CurrentVFXControllers.Clear();
        NextVFXControllers.Clear();
    }
}