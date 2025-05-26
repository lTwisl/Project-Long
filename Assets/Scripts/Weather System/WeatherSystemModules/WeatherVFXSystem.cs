using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WeatherVFXSystem: MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; } = true;

    [field: Header("VFX Graphs:")]
    [field: SerializeField] public List<VFXController> CurrentVFXControllers { get; private set; }
    [field: SerializeField] public List<VFXController> NewVFXControllers { get; private set; }

    public void ValidateSystem() { }

    public void SpawnVFX(WeatherProfile newProfile, Transform parent)
    {
        foreach (GameObject vfx in newProfile.VFX)
        {
            if (vfx == null) continue;
            NewVFXControllers.Add(Instantiate(vfx, parent.position, Quaternion.identity, parent).GetComponent<VFXController>());
        }
    }

    public void UpdateSystem(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        // ���������� ��������� ������ �������
        foreach (VFXController vfx in CurrentVFXControllers)
            vfx.SetVFXParameter("t", 1f - t);

        // ���������� ��������� ����� �������
        foreach (VFXController vfx in NewVFXControllers)
            vfx.SetVFXParameter("t", t);

        if (t >= 1)
        {
            // ���������� ��������� ������ �������
            foreach (VFXController vfx in CurrentVFXControllers)
                vfx.DestroyVFX();
            CurrentVFXControllers.Clear();

            // ���������� ��������� ����� �������
            foreach (VFXController vfx in NewVFXControllers)
                CurrentVFXControllers.Add(vfx);
            NewVFXControllers.Clear();
        }
    }
}
