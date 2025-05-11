using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    // ����� ����������
    [SerializeField, DisableEdit] protected bool _isControllerValide;
    [SerializeField] protected bool _destroyVFXInstantly;

    // ����� ������
    [field: SerializeField] public VisualEffect VFXGraph { get; private set; }

    /// <summary>
    /// ����� �������� ���������� ������
    /// </summary>
    public virtual void ValidateReferences()
    {
        // ������ ��������� ������
    }

    public void DestroyVFX()
    {
        if (_destroyVFXInstantly)
            Destroy(gameObject);
        else
            StartCoroutine(WaitDestroyVFX());
    }

    private IEnumerator WaitDestroyVFX()
    {
        // ���� ������� ��� ���, ������� �����
        if (VFXGraph == null)
        {
            Destroy(gameObject);
            yield return null;
        }
        // ���� ���� ��� ������� �����
        while (VFXGraph.aliveParticleCount > 0)
        {
            yield return null;
        }
        // ���������� ������
        Destroy(gameObject);
        yield return null;
    }

    /// <summary>
    /// ����� �������� ���������� ��������� � VFX Graph
    /// </summary>
    public virtual void SetVFXPermanentParameters()
    {
        // ���������� ��������� � VFX Graph
    }

    #region ���������� ���������� VFX
    public void SetVFXParameter(string nameProperty, float valueProperty)
    {
        VFXGraph.SetFloat(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, Vector2 valueProperty)
    {
        VFXGraph.SetVector2(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, Vector3 valueProperty)
    {
        VFXGraph.SetVector3(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, Vector4 valueProperty)
    {
        VFXGraph.SetVector4(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, bool valueProperty)
    {
        VFXGraph.SetBool(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, int valueProperty)
    {
        VFXGraph.SetInt(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, Texture2D valueProperty)
    {
        VFXGraph.SetTexture(nameProperty, valueProperty);
    }
    #endregion
}