#if UNITY_EDITOR
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

    /// <summary>
    /// ������� ������������ ������� � ������� ������ Handles �� 2� ������
    /// </summary>
    /// <param name="bottomPoint">����� ������ ��������� �������</param>
    /// <param name="upperPoint">����� ������� ��������� �������</param>
    /// <param name="radius">������ �������</param>
    /// <param name="color">���� �������</param>
    public static void DrawWireCapsule(Vector3 bottomPoint, Vector3 upperPoint, float radius, Color color)
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

    /// <summary>
    /// ������� ������������ 2 ������� � ������� ������ Handles �� 2� ������ ������ � ������� �����
    /// </summary>
    /// <param name="bottomPoint">����� ������ ��������� �������</param>
    /// <param name="upperPoint">����� ������� ��������� �������</param>
    /// <param name="radius">������ �������</param>
    /// <param name="color">���� �������</param>
    /// <param name="castVector">��������� ������ �����</param>
    public static void DrawWireCapsule(Vector3 bottomPoint, Vector3 upperPoint, float radius, Color color, Vector3 castVector)
    {
        DrawWireCapsule(bottomPoint, upperPoint, radius, color);

        Vector3 newBottomPoint = bottomPoint + castVector;
        Vector3 newUpperPoint = upperPoint + castVector;
        DrawWireCapsule(newBottomPoint, newUpperPoint, radius, color);

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

    /// <summary>
    /// ������� ������������ ������� � ������� ������ Handles
    /// </summary>
    /// <param name="position">������� ������� (��������)</param>
    /// <param name="rotation">���������� �������</param>
    /// <param name="length">����� ����� �������</param>
    /// <param name="arrowheadSize">������ ����������� (�� 0 �� 1, ������������ �����)</param>
    /// <param name="color">���� �������</param>
    public static void DrawArrow(Vector3 position, Quaternion rotation, float length, float arrowheadSize, Color color)
    {
        using (new Handles.DrawingScope(color))
        {
            // 1. ����������� ������ �����������
            arrowheadSize = Mathf.Clamp01(arrowheadSize);

            // 2. ��������� ����� �������
            Vector3 forward = rotation * Vector3.forward;
            Vector3 tail = position - forward * length * 0.5f;
            Vector3 tip = position + forward * length * 0.5f;
            float wingLength = length * arrowheadSize;

            // 3. ������ �������� ����� �������
            Handles.DrawLine(tail, tip);

            // 4. ������������ ����� � ������ ����� �����������
            Vector3 right = rotation * Vector3.right * wingLength;

            // 5. ������ ���������� � ���� ������������
            Handles.DrawLine(tip, tip - forward * wingLength + right);
            Handles.DrawLine(tip, tip - forward * wingLength - right);
            Handles.DrawLine(tip - forward * wingLength - right, tip - forward * wingLength + right);
        }
    }

    /// <summary>
    /// ������� ������������ ������� � ������� ������ Handles �� 2� ������
    /// </summary>
    /// <param name="start">����� ������ �������</param>
    /// <param name="end">����� ����� �������</param>
    /// <param name="arrowheadSize">������ ����������� (�� 0 �� 1)</param>
    /// <param name="color">���� �������</param>
    public static void DrawArrow(Vector3 start, Vector3 end, float arrowheadSize, Color color)
    {
        Vector3 direction = end - start;
        float length = direction.magnitude;
        if (length == 0f) return;

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        Vector3 position = start + direction * 0.5f;

        DrawArrow(position, rotation, length, arrowheadSize, color);
    }

    /// <summary>
    /// ������� ������������ ����� �������� ����������� � ������� ������ Handles
    /// </summary>
    /// <param name="position">������� ������ �����</param>
    /// <param name="cellSize">������ ����� ������</param>
    /// <param name="gridSize">���������� ����� �� X/Z</param>
    /// <param name="color">���� �����</param>
    public static void DrawGrid(Vector3 position, float cellSize, int gridSize, Color color)
    {
        using (new Handles.DrawingScope(color))
        {
            float totalSize = cellSize * gridSize;
            Vector3 start = position - new Vector3(totalSize / 2f, 0, totalSize / 2f);

            // �������������� �����
            for (int x = 0; x <= gridSize; x++)
            {
                Vector3 startX = start + Vector3.right * x * cellSize;
                Vector3 endX = startX + Vector3.forward * totalSize;
                Handles.DrawLine(startX, endX);
            }

            // ������������ �����
            for (int z = 0; z <= gridSize; z++)
            {
                Vector3 startZ = start + Vector3.forward * z * cellSize;
                Vector3 endZ = startZ + Vector3.right * totalSize;
                Handles.DrawLine(startZ, endZ);
            }
        }
    }

    private static void CalculateLocalAxis(Vector3 worldAxis, Quaternion rotation, out Vector3 localAxis)
    {
        localAxis = rotation * worldAxis;
    }
}
#endif