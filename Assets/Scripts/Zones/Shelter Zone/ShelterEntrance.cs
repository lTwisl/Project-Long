using UnityEngine;

[RequireComponent(typeof(Collider))]
[ExecuteAlways]
public class ShelterEntrance : MonoBehaviour
{
    public enum EntranceType
    {
        Entrance,
        Exit
    }

    [Header("Параметры обьекта:")]
    [SerializeField] private string _entranceID = "Door01";
    [DisableEdit, SerializeField] private EntranceType _currentType;
    [DisableEdit, SerializeField] private ShelterSystem _shelterSystem;
    [DisableEdit, SerializeField] private Collider _collider;

    public string EntranceID => _entranceID;
    public EntranceType CurrentType => _currentType;
    public ShelterSystem ShelterSystem => _shelterSystem;
    public bool IsEntrance => _currentType == EntranceType.Entrance;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_shelterSystem != null)
            {
                // Определяем направление выхода из коллайдера
                Vector3 playerDirection = (other.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(playerDirection, transform.forward);

                // Если дверь — вход, и игрок движется против направления двери, то он заходит
                if (IsEntrance && dotProduct < 0)
                {
                    _shelterSystem.PlayerEntered(true);
                }
                // Если дверь — выход, и игрок движется по направлению двери, то он выходит
                else if (!IsEntrance && dotProduct > 0)
                {
                    _shelterSystem.PlayerEntered(false);
                }
            }
        }
    }

    public void SetEntranceStatus(bool isEntrance)
    {
        _currentType = isEntrance ? EntranceType.Entrance : EntranceType.Exit;
        ChangeNaming();

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Change Entrance Status");
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void ChangeNaming()
    {
        gameObject.name = $"[{_currentType}] {_entranceID}";
    }

    #region ВИЗУАЛИЗАЦИЯ
    private void OnDrawGizmos()
    {
        // Визуализация для "потерянной" двери
        if (_shelterSystem == null)
        {
            Gizmos.color = Color.red;
            if (_collider != null)
                Gizmos.DrawWireCube(transform.position, _collider.bounds.size);
            Gizmos.DrawIcon(transform.position, "console.erroricon");

            GUIStyle _guiStyle = new GUIStyle
            {
                normal = { textColor = Color.red },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            UnityEditor.Handles.Label
            (
                transform.position + Vector3.up * 1.0f,
                "Дверь не относится\nк укрытию!",
                _guiStyle
            );
        }
    }

    public void DrawEntranceGizmo()
    {
        if (_shelterSystem == null) return;

        var direction = Vector3.zero;
        if (_currentType == EntranceType.Entrance)
            direction = transform.forward;
        if (_currentType == EntranceType.Exit)
            direction = -transform.forward;

        var color = _currentType == EntranceType.Entrance ? Color.green : Color.red;

        // Отрисовка направления входа/выхода
        Gizmos.color = color;
        Gizmos.DrawRay(transform.position, direction);
        Gizmos.DrawWireSphere(transform.position + direction, 0.15f);

        // Отрисовка текстовой метки
        GUIStyle _guiStyle = new GUIStyle
        {
            normal = { textColor = color },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        UnityEditor.Handles.Label
        (
            transform.position + Vector3.up * 0.75f,
            $"{_currentType}\nID: {_entranceID}",
            _guiStyle
        );

        // Отрисовка значка двери
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, 0.1f);
    }
    #endregion

    #region EDITOR
    private void OnValidate()
    {
        ChangeNaming();
        CacheCollider();
    }

    private void CacheCollider()
    {
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }
    }

    public void SetShelterSystem(ShelterSystem parentShelter)
    {
        _shelterSystem = parentShelter;

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Set Parent Shelter");
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Функция переопределения родительского обьекта при его изменении в иерархии
    /// </summary>
    private void OnTransformParentChanged()
    {
        UnityEditor.Undo.RecordObject(this, "Change Parent Shelter");
        if (transform.parent != null)
        {
            var newParentShelter = transform.parent.GetComponentInParent<ShelterSystem>();
            if (newParentShelter != null && newParentShelter != _shelterSystem)
            {
                _shelterSystem = newParentShelter;
                _shelterSystem.AddEntrance(this);
            }
        }
        ChangeNaming();
        CacheCollider();
    }
    #endif
    #endregion
}