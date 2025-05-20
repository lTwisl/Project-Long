using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    [field: SerializeField] public VisualEffect VFXGraph { get; private set; }
    protected enum RepositionType
    {
        /// <summary> ������ ������� �� �����, �������� ������������� ������ � ������� ����������� </summary>
        FollowTargetWithWorldHeight,
        /// <summary> ������ ������� �� �����, �������� ������������� ������ ������������ ��� ��������� ��������� </summary>
        FollowTargetWithLocalHeight,
        /// <summary> ������ �� ������������������� (�������� �� �����) </summary>
        Static
    }
    [SerializeField] protected RepositionType _repositionType;
    [SerializeField, HideIf(nameof(_repositionType), RepositionType.Static)] protected Transform _targetTransform;
    [SerializeField, HideIf(nameof(_repositionType), RepositionType.Static)] protected float _height = 0;
    protected enum DestroyType
    {
        /// <summary> ���������� ����������� </summary>
        Immediately,
        /// <summary> ����������� ����� ����������� ���� ��������� </summary>
        OnAllParticlesDead,
        /// <summary> ���������� �������� </summary>
        Delayed
    }
    [SerializeField] protected DestroyType _destroyType;


    /// <summary>
    /// ��������: ������� �� ���������� ����������� �������?
    /// </summary>
    public virtual bool IsVFXControllerValid()
    {
        return VFXGraph == null;
    }

    /// <summary>
    /// ������������������� VFX Graph � ����������
    /// </summary>
    public void RepositionVFX()
    {
        if (_repositionType == RepositionType.Static || _targetTransform == null) return;

        Vector3 newPosition;
        switch (_repositionType)
        {
            case (RepositionType.FollowTargetWithWorldHeight):
                newPosition = new Vector3(_targetTransform.position.x, _height, _targetTransform.position.z);
                break;

            case (RepositionType.FollowTargetWithLocalHeight):
                newPosition = new Vector3(_targetTransform.position.x, _targetTransform.position.y + _height, _targetTransform.position.z);
                break;

            default:
                newPosition = Vector3.zero;
                _repositionType = RepositionType.Static;
                break;
        }
        transform.position = newPosition;
    }

    /// <summary>
    /// ���������� VFX Graph � ����������
    /// </summary>
    public void DestroyVFX()
    {
        switch (_destroyType)
        {
            case (DestroyType.Immediately):
                Destroy(gameObject);
                break;

            case (DestroyType.OnAllParticlesDead):
                StartCoroutine(WaitDestroyVFX());
                break;

            case (DestroyType.Delayed):
                Destroy(gameObject);
                break;

            default:
                Destroy(gameObject);
                break;
        }
    }

    private IEnumerator WaitDestroyVFX()
    {
        // ���� ������� ��� ���, ������� �����
        if (VFXGraph == null)
        {
            Destroy(gameObject);
            yield return null;
        }

        // ����� ���� ���� ��� ������� �����
        while (VFXGraph.aliveParticleCount > 0)
        {
            yield return null;
        }

        // ����� ���� ���������� ������
        Destroy(gameObject);
    }

    /// <summary>
    /// �������� � VFX Graph ���������� ��������� ���������� ������������ 
    /// </summary>
    public virtual void UpdateRealtimeVFXParameters() { }

    #region ������ � ����������� ���������� VFX Graph
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