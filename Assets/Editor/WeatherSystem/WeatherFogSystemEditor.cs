using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(WeatherFogSystem))]
public class WeatherFogSystemEditor : Editor
{
    private WeatherFogSystem weatherFogSystem;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        weatherFogSystem = (WeatherFogSystem)target;

        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null)
        {
            weatherFogSystem.ValidateReferences();
            weatherFogSystem.UpdateSunDirection();
        }
    }
}
