using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXController : MonoBehaviour
{
    protected enum RepositionType
    {
        /// <summary> VFXGraph следует за целью, сохран€€ высоту в мировых координатах </summary>
        FollowTargetWithWorldHeight,
        /// <summary> VFXGraph следует за целью, сохран€€ высоту в его локальных координатах </summary>
        FollowTargetWithLocalHeight,
        /// <summary> VFXGraph не перепозиционируетс€ </summary>
        Static
    }

    protected enum DestroyType
    {
        /// <summary> ћгновенное уничтожение </summary>
        Immediately,
        /// <summary> ”ничтожение после уничтожени€ всех частиц </summary>
        OnAllParticlesDead
    }

    [field: Header("- - VFX Controller Parameters:")]
    [field: SerializeField] public VisualEffect VFXGraph { get; protected set; }
    [SerializeField] protected DestroyType _destroyType;
    [SerializeField] protected RepositionType _repositionType;
    [SerializeField, HideIf(nameof(_repositionType), RepositionType.Static)] protected Transform _targetTransform;
    [SerializeField, HideIf(nameof(_repositionType), RepositionType.Static)] protected float _height = 0;


    /// <summary>
    /// ѕроверка: валиден ли VFX Controller?
    /// </summary>
    public virtual bool IsControllerValid()
    {
        return VFXGraph;
    }

    /// <summary>
    /// ќбновить позицию VFX Controller
    /// </summary>
    public void UpdatePosition()
    {
        if (_repositionType == RepositionType.Static || !_targetTransform) return;

        switch (_repositionType)
        {
            case (RepositionType.FollowTargetWithWorldHeight):
                transform.position = new Vector3(_targetTransform.position.x, _height, _targetTransform.position.z);
                break;

            case (RepositionType.FollowTargetWithLocalHeight):
                transform.position = new Vector3(_targetTransform.position.x, _targetTransform.position.y + _height, _targetTransform.position.z);
                break;

            default:
                _repositionType = RepositionType.Static;
                transform.localPosition = Vector3.zero;
                return;
        }
    }

    /// <summary>
    /// ѕередача в VFX Graph параметров требующих посто€нной актуализации 
    /// </summary>
    public virtual void UpdateRealtimePropertys()
    { 

    }

    /// <summary>
    /// ”ничтожить префаб, VFX Graph и VFX Controller
    /// </summary>
    public void Destroy()
    {
        switch (_destroyType)
        {
            case (DestroyType.Immediately):
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
                break;

            case (DestroyType.OnAllParticlesDead):
                if (Application.isPlaying)
                    StartCoroutine(WaitDestroyVFX());
                else
                    DestroyImmediate(gameObject);
                break;

            default:
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
                break;
        }
    }

    private IEnumerator WaitDestroyVFX()
    {
        while (VFXGraph && VFXGraph.aliveParticleCount > 0)
            yield return null;

        Destroy(gameObject);
    }
}