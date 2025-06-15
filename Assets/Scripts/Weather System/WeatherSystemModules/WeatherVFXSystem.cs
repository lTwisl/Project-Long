using UnityEngine;
using System.Collections.Generic;

public class WeatherVFXSystem: MonoBehaviour, IWeatherSystem
{
    [SerializeField, DisableEdit] private bool _isSystemValid;
    public bool IsSystemValid => _isSystemValid;

    [field: Header("- - VFX Graphs:")]
    [field: SerializeField] public List<VFXController> CurrentVFXControllers { get; private set; }
    [field: SerializeField] public List<VFXController> NewVFXControllers { get; private set; }

    public void InitializeAndValidateSystem()
    {
        _isSystemValid = true;
    }

    public void SpawnVFXControllers(WeatherProfile nextProfile)
    {
        foreach (GameObject vfx in nextProfile.VFX)
        {
            if (vfx == null) continue;
            NewVFXControllers.Add(Instantiate(vfx, transform.position, Quaternion.identity, transform).GetComponent<VFXController>());
        }
    }

    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
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