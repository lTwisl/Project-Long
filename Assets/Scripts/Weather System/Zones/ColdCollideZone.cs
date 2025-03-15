using UnityEngine;

[ExecuteAlways]
public class ColdCollideZone : MonoBehaviour
{
    [field: SerializeField, Tooltip("����������� ���� ������")] private float _tempRatio = -10f;
    public float TempRatio
    {
        get => _tempRatio;
        private set => _tempRatio = value;
    }

    [SerializeField] private Collider _collider;

    // ������� ��� �������� ��������� ������������
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
        // ���������, ��� � ���� ����� �����
        if (other.CompareTag("Player"))
        {
            // �������� ������� � �������� �������� �����������
            OnTemperatureChanged?.Invoke(_tempRatio);
            Debug.Log($"����� ����� � ���� ������. ������������� �����������: {_tempRatio}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ���������, ��� �� ���� ����� �����
        if (other.CompareTag("Player"))
        {
            // �������� ������� � �������� 0 (��� ������ �������� �� ���������)
            OnTemperatureChanged?.Invoke(0);
            Debug.Log("����� ����� �� ���� ������. ������������� ����������� �������.");
        }
    }
}