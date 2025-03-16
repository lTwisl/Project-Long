using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(WeatherSkyboxSystem))]
public class WeatherSkyboxSystemEditor : Editor
{
    private WeatherSkyboxSystem _weatherSkyboxSystem;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        _weatherSkyboxSystem = (WeatherSkyboxSystem)target;

        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null)
        {
            _weatherSkyboxSystem.ValidateReferences();
            _weatherSkyboxSystem.UpdateSunDirection();
        }
    }
}