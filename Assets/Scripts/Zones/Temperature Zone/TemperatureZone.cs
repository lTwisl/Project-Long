using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(SphereCollider))]
public class TemperatureZone : MonoBehaviour
{
    [Inject] private World _world;

    [field: SerializeField, Min(0f)] public float Temperature { get; private set; }
    [field: SerializeField] public float MinRadius { get; private set; }
    [field: SerializeField] public float MaxRadius { get; private set; }

    private SphereCollider _sphereCollider;
    private bool _isIn = false;

    [Header("Settings Grid Raycast")]
    [SerializeField, DrawHandle(handleSpace: Space.Self)] private Vector3 _originRays = Vector3.zero;
    [SerializeField] private Vector2Int _gridSize = new Vector2Int(5, 5); // Количество лучей по осям X и Z
    [SerializeField] private Vector2 _spacing = new Vector2(1, 1); // Расстояние между лучами
    [SerializeField] private LayerMask _layerMask;

    [SerializeField] private bool _drawRays = false;

    private Collider _playerCollider;

    private void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerCollider = other;
        StartCoroutine(CheckHeating());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerCollider = null;

        if (!_isIn)
            return;

        Debug.Log("Exit");
        _isIn = false;
        _world.InvokeOnExitTemperatureZone(this);
    }

    private IEnumerator CheckHeating()
    {
        while (_playerCollider)
        {
            Vector3 startRays = transform.position + _originRays;
            // Основное направление луча
            Vector3 baseDirection = _playerCollider.bounds.center - startRays;

            // Вычисляем перпендикулярные направления
            Vector3 rightOffset = Vector3.Cross(baseDirection.normalized, Vector3.up).normalized * _spacing.x;
            Vector3 upOffset = Vector3.Cross(rightOffset, baseDirection.normalized).normalized * _spacing.y;

            int halfWidth = Mathf.FloorToInt(_gridSize.x / 2);
            int halfHeight = Mathf.FloorToInt(_gridSize.y / 2);

            bool newIsIn = false;
            int counterHits = 0;

            /*// От центральной строки в крайнии 
            for (int _y = 0; _y <= halfHeight; ++_y)
            {
                foreach (int y in _y == 0 ? new int[] { _y } : new int[] { _y , -_y })
                {
                    for (int x = -halfWidth; x <= halfWidth; ++x)
                    {
                        yield return GameTime.YieldNull();

                        if (_playerCollider == null)
                            yield break;

                        // Вычисляем смещение для текущего луча
                        Vector3 positionOffset = rightOffset * x + upOffset * y;

                        // Начальная и конечная точки луча
                        Vector3 start = startRays + positionOffset;
                        Vector3 end = start + baseDirection;

                        Vector3 dir = end - startRays;


                        if (!Physics.Raycast(startRays, dir.normalized, out RaycastHit hitInfo, dir.magnitude, _layerMask))
                        {
                            continue;
                        }

                        if (!hitInfo.collider.CompareTag("Player"))
                        {
                            if (_drawRays)
                                Debug.DrawLine(startRays, end, Color.red);
                            continue;
                        }
                        else
                        {
                            if (_drawRays)
                                Debug.DrawLine(startRays, end, Color.green);
                        }

                        counterHits += 1;
                        if (counterHits >= 3)
                        {
                            newIsIn = true;
                            break;
                        }

                    }
                }
            }*/

            for (int y = halfHeight; y >= -halfHeight; --y) // Сверху вниз
            {
                for (int x = -halfWidth; x <= halfWidth; ++x) // Слева направо 
                {
                    yield return GameTime.YieldNull();

                    if (_playerCollider == null)
                        yield break;

                    // Вычисляем смещение для текущего луча
                    Vector3 positionOffset = rightOffset * x + upOffset * y;

                    // Начальная и конечная точки луча
                    Vector3 start = startRays + positionOffset;
                    Vector3 end = start + baseDirection;

                    Vector3 dir = end - startRays;


                    if (!Physics.Raycast(startRays, dir.normalized, out RaycastHit hitInfo, dir.magnitude, _layerMask))
                    {
                        continue;
                    }

                    if (!hitInfo.collider.CompareTag("Player"))
                    {
                        if (_drawRays)
                            Debug.DrawLine(startRays, end, Color.red);
                        continue;
                    }
                    else
                    {
                        if (_drawRays)
                            Debug.DrawLine(startRays, end, Color.green);
                    }

                    counterHits += 1;
                    if (counterHits >= 3)
                    {
                        newIsIn = true;
                        break;
                    }
                }

                if (newIsIn)
                    break;
            }

            if (_isIn && !newIsIn)
            {
                _isIn = false;
                _world.InvokeOnExitTemperatureZone(this);
                Debug.Log("Exit");
            }
            else if (!_isIn && newIsIn)
            {
                _isIn = true;
                _world.InvokeOnEnterTemperatureZone(this);
                Debug.Log("Enter");
            }
        }


    }




#if UNITY_EDITOR
    private void OnValidate()
    {
        _sphereCollider ??= GetComponent<SphereCollider>();

        _sphereCollider.radius = Mathf.Max(0, MaxRadius);
        MinRadius = Mathf.Clamp(MinRadius, 0f, _sphereCollider.radius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position + _sphereCollider.center, MinRadius);
    }
#endif
}
