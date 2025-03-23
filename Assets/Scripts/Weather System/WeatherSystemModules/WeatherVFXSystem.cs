using UnityEngine;
using System.Collections.Generic;

public class WeatherVFXSystem: MonoBehaviour
{
    [Header("VFX Graphs:")]
    [field: SerializeField] public List<VFXController> CurrentVFXControllers { get; private set; }
    [field: SerializeField] public List<VFXController> NewVFXControllers { get; private set; }

    public void SpawnVFX(WeatherProfile newProfile, Transform parent)
    {
        foreach (GameObject vfx in newProfile.VFX)
        {
            if (vfx == null) continue;
            NewVFXControllers.Add(Instantiate(vfx, parent.position, Quaternion.identity, parent).GetComponent<VFXController>());
        }
    }

    public void UpdateVFX(float t)
    {
        // Заставляем исчезнуть старые эффекты
        foreach (VFXController vfx in CurrentVFXControllers)
        {
            vfx.SetVFXParameter("t", 1f - t);
        }
        // Заставляем появиться новые эффекты
        foreach (VFXController vfx in NewVFXControllers)
        {
            vfx.SetVFXParameter("t", t);
        }

        if (t >= 1)
        {
            // Заставляем исчезнуть старые эффекты
            foreach (VFXController vfx in CurrentVFXControllers)
            {
                vfx.DestroyVFX();
            }
            CurrentVFXControllers.Clear();

            // Заставляем появиться новые эффекты
            foreach (VFXController vfx in NewVFXControllers)
            {
                CurrentVFXControllers.Add(vfx);
            }
            NewVFXControllers.Clear();
        }
    }
}
