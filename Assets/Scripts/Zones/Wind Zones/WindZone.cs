using UnityEngine;

[System.Serializable]
public class WindZone
{
    [Tooltip("������������� ����")]
    public string name = "Unnamed Zone";

    [Tooltip("����� ���� �������")]
    public Transform transform;

    [Tooltip("������ ����������� � ������"), Min(0.1f)]
    public float radius = 5f;

    [Tooltip("���� ������� (1.0 = �����������)"), Range(0.1f, 10f)]
    public float intensityMultiplier = 1.5f;

    [Tooltip("������ ��������� �������")]
    public AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Tooltip("���� �� ������������� gameObject �� Transform")]
    public bool renameTransform = false;
}