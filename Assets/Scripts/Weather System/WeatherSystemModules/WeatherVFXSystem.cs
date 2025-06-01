using UnityEngine;
using System.Collections.Generic;

public class WeatherVFXSystem: MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; } = true;

    [field: Header("VFX Graphs:")]
    [field: SerializeField] public List<VFXController> CurrentVFXControllers { get; private set; } = new();
    [field: SerializeField] public List<VFXController> NewVFXControllers { get; private set; } = new();

    public void ValidateSystem() 
    {
        
    }

    public void SpawnVFX(WeatherProfile nextProfile)
    {
        foreach (GameObject vfx in nextProfile.VFX)
        {
            if (vfx == null) continue;
            NewVFXControllers.Add(Instantiate(vfx, transform.position, Quaternion.identity, transform).GetComponent<VFXController>());
        }
    }

    public void UpdateSystem(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        // Заставляем исчезнуть старые эффекты
        foreach (VFXController vfx in CurrentVFXControllers)
            vfx.SetVFXParameter("t", 1f - t);

        // Заставляем появиться новые эффекты
        foreach (VFXController vfx in NewVFXControllers)
            vfx.SetVFXParameter("t", t);

        if (t >= 1)
        {
            // Заставляем исчезнуть старые эффекты
            foreach (VFXController vfx in CurrentVFXControllers)
                vfx.DestroyVFX();
            CurrentVFXControllers.Clear();

            // Заставляем появиться новые эффекты
            foreach (VFXController vfx in NewVFXControllers)
                CurrentVFXControllers.Add(vfx);
            NewVFXControllers.Clear();
        }
    }
}