using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(WeatherPostProcessSystem))]
public class WeatherPostProcessSystemEditor : Editor
{
    private WeatherPostProcessSystem _weatherSkyboxSystem;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        _weatherSkyboxSystem = (WeatherPostProcessSystem)target;

        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null)
        {
            _weatherSkyboxSystem.ValidateReferences();
        }
    }
}