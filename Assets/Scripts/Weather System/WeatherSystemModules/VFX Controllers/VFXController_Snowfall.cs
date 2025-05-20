using UnityEngine;

public class VFXController_Snowfall : VFXController
{
    private WeatherWindSystem _windSystem;

    private void Awake()
    {
        _windSystem = FindAnyObjectByType<WeatherWindSystem>();
    }

    public override bool IsVFXControllerValid()
    {
        if (VFXGraph != null && _windSystem != null)
            return true;

        Debug.LogWarning("<color=orange>VFX Controller Snowfall не валиден, требуется проверка значений!</color>", this);
        return false;
    }

    void Update()
    {
        RepositionVFX();

        if (IsVFXControllerValid())
            UpdateRealtimeVFXParameters();
    }

    public override void UpdateRealtimeVFXParameters()
    {
        // Передаем вектор мирового ветра 
        SetVFXParameter("Wind Vector", _windSystem.GetWindGlobalVector());
    }
}