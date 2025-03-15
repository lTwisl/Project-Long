using UnityEngine;

[ExecuteInEditMode]
public class EnviromentEditorHelper : MonoBehaviour
{
    [Header("���������, � ������� ���� ��������� ����������� ������")]
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
            Debug.LogWarning("<color=orange>����, �� ���� ������� �������� ��������� � (Lighting -> Enviroment -> Sun Source)</color>");
            return;
        }

        // ���������� ��������� ������
        if (fogMaterial != null && fogFarMaterial != null && fogMaterial.HasProperty("_Sun_Direction") && fogFarMaterial.HasProperty("_Sun_Direction"))
        {
            fogMaterial.SetVector("_Sun_Direction", RenderSettings.sun.transform.forward);
            fogFarMaterial.SetVector("_Sun_Direction", RenderSettings.sun.transform.forward);
        }
    }
}
