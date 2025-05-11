using UnityEngine;

public class VFXSnowController : VFXController
{
    private WeatherWindSystem _windSystem;

    private void Awake()
    {
        _windSystem = FindAnyObjectByType<WeatherWindSystem>();
    }

    public override void ValidateReferences()
    {
        _isControllerValide = VFXGraph != null && _windSystem != null;

        if (!_isControllerValide) Debug.LogWarning("<color=orange>Не найдены референсы скрипта VFX Controller</color>", this);
    }

    void Update()
    {
        if (!_isControllerValide) return;

        SetVFXPermanentParameters();
    }

    public override void SetVFXPermanentParameters()
    {
        // Передаем вектор мирового ветра
        SetVFXParameter("Wind Vector", _windSystem.GetWindGlobalVector());
    }
}