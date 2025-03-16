using UnityEngine;

[ExecuteAlways]
public class ColdCollideZone : MonoBehaviour
{
    [field: SerializeField, Tooltip("Коэффициент зоны холода")] private float _tempRatio = -10f;
    public float TempRatio
    {
        get => _tempRatio;
        private set => _tempRatio = value;
    }

    [SerializeField] private Collider _collider;

    // Событие для передачи теплового коэффициента
    public delegate void TemperatureChangeHandler(float temperature);
    public static event TemperatureChangeHandler OnTemperatureChanged;

    private void Update()
    {
        if (Application.isEditor)
        {
            if (_collider == null && TryGetComponent<Collider>(out Collider collider))
            {
                _collider = collider;
                collider.isTrigger = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в зону вошел игрок
        if (other.CompareTag("Player"))
        {
            // Вызываем событие и передаем тепловой коэффициент
            OnTemperatureChanged?.Invoke(_tempRatio);
            Debug.Log($"Игрок вошел в зону холода. Температурный коэффициент: {_tempRatio}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Проверяем, что из зоны вышел игрок
        if (other.CompareTag("Player"))
        {
            // Вызываем событие и передаем 0 (или другое значение по умолчанию)
            OnTemperatureChanged?.Invoke(0);
            Debug.Log("Игрок вышел из зоны холода. Температурный коэффициент сброшен.");
        }
    }
}