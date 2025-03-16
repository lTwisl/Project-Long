using UnityEngine;

[ExecuteInEditMode]
public class EnviromentEditorHelper : MonoBehaviour
{
    [Header("Материалы, в которые надо просылать направление солнца")]
    public Material fogMaterial;
    public Material fogFarMaterial;

    void Update()
    {
        UpdateSunDirection();
    }

    private void UpdateSunDirection()
    {
        if (RenderSettings.sun.transform == null)
        {
            Debug.LogWarning("<color=orange>Сука, не тупи назначь источник освещения в (Lighting -> Enviroment -> Sun Source)</color>");
            return;
        }

        // Обновление обьемного тумана
        if (fogMaterial != null && fogFarMaterial != null && fogMaterial.HasProperty("_Sun_Direction") && fogFarMaterial.HasProperty("_Sun_Direction"))
        {
            fogMaterial.SetVector("_Sun_Direction", RenderSettings.sun.transform.forward);
            fogFarMaterial.SetVector("_Sun_Direction", RenderSettings.sun.transform.forward);
        }
    }
}
