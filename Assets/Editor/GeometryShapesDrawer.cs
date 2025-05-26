using UnityEditor;
using UnityEngine;

public static class GeometryShapesDrawer
{
    /// <summary>
    /// Функция отрисовывает капсулу с помощью класса Handles
    /// </summary>
    /// <param name="center">Центр объема капсулы</param>
    /// <param name="radius">Радиус сферической части капсулы</param>
    /// <param name="height">Общая высота капсулы</param>
    /// <param name="rotation">Ориентация капсулы</param>
    public static void DrawWireCapsule(Vector3 center, float radius, float height, Quaternion rotation)
    {
        // 1. Корректируем высоту по радиусу
        height = Mathf.Max(height, radius * 2);

        // 2. Считаем основные направления локальных осей
        CalculateLocalAxis(Vector3.up, rotation, out Vector3 up);
        CalculateLocalAxis(Vector3.forward, rotation, out Vector3 forward);
        CalculateLocalAxis(Vector3.right, rotation, out Vector3 right);

        // 3. Считаем точки верхней и нижней оконечностей капсулы
        Vector3 topPoint = center + up * (height / 2);
        Vector3 bottomPoint = center - up * (height / 2);

        // 4. Считаем центры сферических частей
        Vector3 topSphereCenter = topPoint - up * radius;
        Vector3 bottomSphereCenter = bottomPoint + up * radius;

        // 5. Рисуем цилиндрическую часть (диски + линии)
        Handles.DrawLine(bottomSphereCenter + forward * radius, topSphereCenter + forward * radius);
        Handles.DrawLine(bottomSphereCenter + (-forward) * radius, topSphereCenter + (-forward) * radius);
        Handles.DrawLine(bottomSphereCenter + right * radius, topSphereCenter + right * radius);
        Handles.DrawLine(bottomSphereCenter + (-right) * radius, topSphereCenter + (-right) * radius);

        // 6. Рисуем полусферы
        DrawWireHemisphere(topSphereCenter, radius, rotation, true);
        DrawWireHemisphere(bottomSphereCenter, radius, rotation, false);
    }

    /// <summary>
    /// Функция отрисовывает капсулу с помощью класса Handles
    /// </summary>
    /// <param name="center">Центр объема капсулы</param>
    /// <param name="radius">Радиус сферической части капсулы</param>
    /// <param name="height">Общая высота капсулы</param>
    /// <param name="rotation">Ориентация капсулы</param>
    /// <param name="color">Цвет капсулы</param>
    public static void DrawWireCapsule(Vector3 center, float radius, float height, Quaternion rotation, Color color)
    {
        using (new Handles.DrawingScope(color))
        {
            DrawWireCapsule(center, radius, height, rotation);
        }
    }

    /// <summary>
    /// Функция отрисовывает капсулу с помощью класса Handles
    /// </summary>
    /// <param name="center">Центр объема капсулы</param>
    /// <param name="radius">Радиус сферической части капсулы</param>
    /// <param name="height">Общая высота капсулы</param>
    /// <param name="rotation">Ориентация капсулы</param>
    /// <param name="color">Цвет капсулы</param>
    /// <param name="groundPos">Позиция земли капсулы</param>
    public static void DrawWireCapsule(Vector3 center, float radius, float height, Quaternion rotation, Color color, Vector3 groundPos)
    {
        using (new Handles.DrawingScope(color))
        {
            DrawWireCapsule(center, radius, height, rotation);
            Handles.DrawLine(center - rotation * Vector3.up * (height / 2), groundPos);
        }
    }

    public static void DrawWireCapsuleTwoPoints(Vector3 bottomPoint, Vector3 upperPoint, float radius, Color color)
    {
        // 1. Вычисляем направление локальных осей капсулы (от нижней точки к верхней)
        Vector3 direction = (upperPoint - bottomPoint);

        // 2. Вычисляем высоту капсулы
        float height = direction.magnitude + radius * 2;

        // 3. Находим центр капсулы
        Vector3 center = (bottomPoint + upperPoint) * 0.5f;

        // 4. Рассчитываем вращение капсулы
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);

        using (new Handles.DrawingScope(color))
        {
            DrawWireCapsule(center, radius, height, rotation);
        }
    }

    public static void DrawWireCapsuleTwoPoints(Vector3 bottomPoint, Vector3 upperPoint, float radius, Color color, Vector3 castVector)
    {
        DrawWireCapsuleTwoPoints(bottomPoint, upperPoint, radius, color);

        Vector3 newBottomPoint = bottomPoint + castVector;
        Vector3 newUpperPoint = upperPoint + castVector;
        DrawWireCapsuleTwoPoints(newBottomPoint, newUpperPoint, radius, color);

        using (new Handles.DrawingScope(color))
        {
            Handles.DrawLine(bottomPoint, newBottomPoint);
            Handles.DrawLine(upperPoint, newUpperPoint);
        }
    }

    /// <summary>
    /// Функция отрисовывает полусферу с помощью класса Handles
    /// </summary>
    /// <param name="center">Центр полусферы</param>
    /// <param name="radius">Радиус полусферы</param>
    /// <param name="rotation">Ориентация полусферы</param>
    public static void DrawWireHemisphere(Vector3 center, float radius, Quaternion rotation, bool isUpperHemisphere)
    {
        // 1. Считаем основные направления локальных осей
        CalculateLocalAxis(Vector3.up, rotation, out Vector3 up);
        CalculateLocalAxis(Vector3.forward, rotation, out Vector3 forward);
        CalculateLocalAxis(Vector3.right, rotation, out Vector3 right);

        // 2. Рисуем окружность обрезанной чисти сферы
        Handles.DrawWireDisc(center, up, radius);

        // 3. Рисуем дуги полусферы
        if (isUpperHemisphere)
        {
            Handles.DrawWireArc(center, forward, right, 180, radius);
            Handles.DrawWireArc(center, -right, forward, 180, radius);
        }
        else
        {
            Handles.DrawWireArc(center, forward, right, -180, radius);
            Handles.DrawWireArc(center, -right, forward, -180, radius);
        }
    }

    /// <summary>
    /// Функция отрисовывает полусферу с помощью класса Handles
    /// </summary>
    /// <param name="center">Центр полусферы</param>
    /// <param name="radius">Радиус полусферы</param>
    /// <param name="rotation">Ориентация полусферы</param>
    /// <param name="color">Цвет полусферы</param>
    public static void DrawWireHemisphere(Vector3 center, float radius, Quaternion rotation, bool isUpperHemisphere, Color color)
    {
        using (new Handles.DrawingScope(color))
        {
            DrawWireHemisphere(center, radius, rotation, isUpperHemisphere);
        }
    }

    private static void CalculateLocalAxis(Vector3 worldAxis, Quaternion rotation, out Vector3 localAxis)
    {
        localAxis = rotation * worldAxis;
    }
}