using UnityEditor;
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
            // 1. Обновляем параметры света от направления солнца:
            _weatherSystem.SunLight?.UpdateLightingParameters();
            _weatherSystem.MoonLight?.UpdateLightingParameters();

            // 2. Обновляем глобальное освещение сцены от направления солнца:
            //_weatherSystem.SunLight?.UpdateEnviromentLight();

            // 3. Обновляем направление солнца для материалов:
            _weatherSystem.WeatherFogSystem?.UpdateMaterialsSunDirection();
            _weatherSystem.WeatherSkyboxSystem?.UpdateMaterialsSunDirection();
        }
    }

#if UNITY_EDITOR
    public void FindReferences()
    {
        if (_weatherSystem) return;

        _weatherSystem = FindAnyObjectByType<WeatherSystem>();
    }

    private void OnValidate()
    {
        // 0. Не валидируем, если это префаб-ассет (не экземпляр)
        if (PrefabUtility.IsPartOfPrefabAsset(this)) return;

        // 1. Автоматически инициализируем систему в редакторе
        FindReferences();

        // 2. Сохраняем значения для префаба
        if (PrefabUtility.IsPartOfPrefabInstance(this))
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
#endif
}