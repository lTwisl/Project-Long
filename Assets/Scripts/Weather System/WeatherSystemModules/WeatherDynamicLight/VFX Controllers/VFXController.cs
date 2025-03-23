using UnityEngine;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    // Общие переменные
    [DisableEdit, SerializeField] protected bool _isControllerValide;

    // Общие ссылки
    public VisualEffect _vfx;

    /// <summary>
    /// Метод проверки валидности ссылок
    /// </summary>
    public virtual void ValidateReferences()
    {
        // Логика валидации ссылок
    }

    public void DestroyVFX()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Метод передачи постоянных изменений в VFX Graph
    /// </summary>
    public virtual void SetVFXPermanentParameters()
    {
        // Постоянные изменения в VFX Graph
    }

    #region ПРИСВОЕНИЕ ПАРАМЕТРОВ VFX
    public void SetVFXParameter(string nameProperty, float valueProperty)
    {
        _vfx.SetFloat(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, Vector2 valueProperty)
    {
        _vfx.SetVector2(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, Vector3 valueProperty)
    {
        _vfx.SetVector3(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, Vector4 valueProperty)
    {
        _vfx.SetVector4(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, bool valueProperty)
    {
        _vfx.SetBool(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, int valueProperty)
    {
        _vfx.SetInt(nameProperty, valueProperty);
    }

    public void SetVFXParameter(string nameProperty, Texture2D valueProperty)
    {
        _vfx.SetTexture(nameProperty, valueProperty);
    }
    #endregion
}