using EditorAttributes;
using UnityEngine;

[ExecuteInEditMode]
public class WeatherSystemEditorUpdater : MonoBehaviour
{
    [SerializeField] private bool _useModulesUpdate = false;
    [SerializeField] private WeatherSystem _weatherSystem;

    void Update()
    {
        // Обновление параметров погодных систем в Editor
        if (_useModulesUpdate && _weatherSystem)
        {
            _weatherSystem.SunLight?.UpdateLightingParameters();
            _weatherSystem.MoonLight?.UpdateLightingParameters();
        }
    }

#if UNITY_EDITOR
    [Button]
    public void UpdateGlobalIllumination()
    {
        DynamicGI.UpdateEnvironment();
    }

    private void OnValidate()
    {
        if (EditorChangeTracker.IsPrefabInstance(this))
        {
        EditorChangeTracker.RegisterUndo(this, "Find Weather System In Editor");
        _weatherSystem ??= FindAnyObjectByType<WeatherSystem>();
        EditorChangeTracker.SetDirty(this);
        }
    }
#endif
}