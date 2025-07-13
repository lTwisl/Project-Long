using UnityEngine;
using UnityEngine.VFX;

public class VFXControllerSnowflackes : VFXController
{
    private WeatherWindSystem _windSystem;

    private void Awake()
    {
        _windSystem = FindAnyObjectByType<WeatherWindSystem>();
        _targetTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public override bool IsControllerValid()
    {
        return VFXGraph && _windSystem;
    }

    private void LateUpdate()
    {
        UpdatePosition();
        UpdateRealtimePropertys();
    }

    public override void UpdateRealtimePropertys()
    {
        if (!IsControllerValid()) return;

        // Передаем вектор мирового ветра 
        VFXGraph.SetVector2("Wind Vector", _windSystem.GetWindGlobalVector());
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorChangeTracker.RegisterUndo(this, "Find VFX Graph");
        VFXGraph = GetComponent<VisualEffect>();
        EditorChangeTracker.SetDirty(this);
    }
#endif
}