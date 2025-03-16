using UnityEngine;
using UnityEngine.VFX;

public class VFXSnowController : VFXController
{
    private WindSystem _windSystem;

    void Start()
    {
        _windSystem = WindSystem.Instance;
    }

    public override void ValidateReferences()
    {
        isValide = _vfx != null && _windSystem != null;

        if (!isValide) Debug.LogWarning("<color=orange>Не найдены референсы скрипта VFX Controller</color>", this);
    }

    void Update()
    {
        ValidateReferences();
        if (isValide)
        {
            SetVFXPermanentParameters();
            
        }
    }

    public override void SetVFXPermanentParameters()
    {
        // Передаем вектор мирового ветра
        SetVFXParameter("Wind Vector", _windSystem.GetWindGlobalVector());
    }
}