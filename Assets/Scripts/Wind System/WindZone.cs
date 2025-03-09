using UnityEngine;

[System.Serializable]
public class WindZone
{
    [Tooltip("Идентификатор зоны")]
    public string name = "Unnamed Zone";

    [Tooltip("Центр зоны влияния")]
    public Transform transform;

    [Tooltip("Радиус воздействия в метрах"), Min(0.1f)]
    public float radius = 5f;

    [Tooltip("Сила влияния (1.0 = нейтральное)"), Range(0.1f, 10f)]
    public float intensityMultiplier = 1.5f;

    [Tooltip("Кривая затухания влияния")]
    public AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Tooltip("Надо ли переименовать gameObject по Transform")]
    public bool renameTransform = false;
}