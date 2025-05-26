using UnityEditor;
using UnityEngine;

public static class GeometryShapesDrawer
{
    /// <summary>
    /// ������� ������������ ������� � ������� ������ Handles
    /// </summary>
    /// <param name="center">����� ������ �������</param>
    /// <param name="radius">������ ����������� ����� �������</param>
    /// <param name="height">����� ������ �������</param>
    /// <param name="rotation">���������� �������</param>
    public static void DrawWireCapsule(Vector3 center, float radius, float height, Quaternion rotation)
    {
        // 1. ������������ ������ �� �������
        height = Mathf.Max(height, radius * 2);

        // 2. ������� �������� ����������� ��������� ����
        CalculateLocalAxis(Vector3.up, rotation, out Vector3 up);
        CalculateLocalAxis(Vector3.forward, rotation, out Vector3 forward);
        CalculateLocalAxis(Vector3.right, rotation, out Vector3 right);

        // 3. ������� ����� ������� � ������ ������������ �������
        Vector3 topPoint = center + up * (height / 2);
        Vector3 bottomPoint = center - up * (height / 2);

        // 4. ������� ������ ����������� ������
        Vector3 topSphereCenter = topPoint - up * radius;
        Vector3 bottomSphereCenter = bottomPoint + up * radius;

        // 5. ������ �������������� ����� (����� + �����)
        Handles.DrawLine(bottomSphereCenter + forward * radius, topSphereCenter + forward * radius);
        Handles.DrawLine(bottomSphereCenter + (-forward) * radius, topSphereCenter + (-forward) * radius);
        Handles.DrawLine(bottomSphereCenter + right * radius, topSphereCenter + right * radius);
        Handles.DrawLine(bottomSphereCenter + (-right) * radius, topSphereCenter + (-right) * radius);

        // 6. ������ ���������
        DrawWireHemisphere(topSphereCenter, radius, rotation, true);
        DrawWireHemisphere(bottomSphereCenter, radius, rotation, false);
    }

    /// <summary>
    /// ������� ������������ ������� � ������� ������ Handles
    /// </summary>
    /// <param name="center">����� ������ �������</param>
    /// <param name="radius">������ ����������� ����� �������</param>
    /// <param name="height">����� ������ �������</param>
    /// <param name="rotation">���������� �������</param>
    /// <param name="color">���� �������</param>
    public static void DrawWireCapsule(Vector3 center, float radius, float height, Quaternion rotation, Color color)
    {
        using (new Handles.DrawingScope(color))
        {
            DrawWireCapsule(center, radius, height, rotation);
        }
    }

    /// <summary>
    /// ������� ������������ ������� � ������� ������ Handles
    /// </summary>
    /// <param name="center">����� ������ �������</param>
    /// <param name="radius">������ ����������� ����� �������</param>
    /// <param name="height">����� ������ �������</param>
    /// <param name="rotation">���������� �������</param>
    /// <param name="color">���� �������</param>
    /// <param name="groundPos">������� ����� �������</param>
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
        // 1. ��������� ����������� ��������� ���� ������� (�� ������ ����� � �������)
        Vector3 direction = (upperPoint - bottomPoint);

        // 2. ��������� ������ �������
        float height = direction.magnitude + radius * 2;

        // 3. ������� ����� �������
        Vector3 center = (bottomPoint + upperPoint) * 0.5f;

        // 4. ������������ �������� �������
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
    /// ������� ������������ ��������� � ������� ������ Handles
    /// </summary>
    /// <param name="center">����� ���������</param>
    /// <param name="radius">������ ���������</param>
    /// <param name="rotation">���������� ���������</param>
    public static void DrawWireHemisphere(Vector3 center, float radius, Quaternion rotation, bool isUpperHemisphere)
    {
        // 1. ������� �������� ����������� ��������� ����
        CalculateLocalAxis(Vector3.up, rotation, out Vector3 up);
        CalculateLocalAxis(Vector3.forward, rotation, out Vector3 forward);
        CalculateLocalAxis(Vector3.right, rotation, out Vector3 right);

        // 2. ������ ���������� ���������� ����� �����
        Handles.DrawWireDisc(center, up, radius);

        // 3. ������ ���� ���������
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
    /// ������� ������������ ��������� � ������� ������ Handles
    /// </summary>
    /// <param name="center">����� ���������</param>
    /// <param name="radius">������ ���������</param>
    /// <param name="rotation">���������� ���������</param>
    /// <param name="color">���� ���������</param>
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