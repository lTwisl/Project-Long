using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class PlayerProbeData
{
    public enum ProbeState
    {
        Standing,
        Crouching,
        Jumping
    }

    public Vector3 Position;
    public ProbeState State { get; private set; }
    public bool IsPassable { get; private set; }
    public Color VisualizeColor { get; private set; }
    public int ProbeIndex { get; set; } = -1;

    private PlayerDimensions _dimensions;
    public LayerMask ObstacleLayers = ~0;

    public List<string> CollidingObjects = new List<string>();

    public PlayerProbeData(Vector3 position, ProbeState state, PlayerDimensions dimensions)
    {
        Position = position;
        State = state;
        _dimensions = dimensions;
        UpdateColor();
        CheckProbePassability();
    }

    public void UpdateProbe(Vector3 newPosition, ProbeState newState, PlayerDimensions dimensions)
    {
        Position = newPosition;
        SetState(newState);
        _dimensions = dimensions;
        CheckProbePassability();
    }

    public void SetState(ProbeState newState)
    {
        if (newState == State) return;

        State = newState;
        UpdateColor();
        CheckProbePassability();
    }

    private void UpdateColor()
    {
        switch (State)
        {
            case ProbeState.Standing:
                VisualizeColor = IsPassable ? Color.green : Color.red;
                break;

            case ProbeState.Crouching:
                VisualizeColor = IsPassable ? Color.yellow : Color.red;
                break;

            case ProbeState.Jumping:
                VisualizeColor = IsPassable ? new Color(0, 1, 1) : Color.red;
                break;
        }
    }

    public void CheckProbePassability()
    {
        CollidingObjects.Clear();
        switch (State)
        {
            case ProbeState.Standing:
                IsPassable = !HasProbeCollision(Position + Vector3.up * (_dimensions.groundOffset + _dimensions.heightStanding / 2), _dimensions.capsuleRadius, _dimensions.heightStanding);
                break;

            case ProbeState.Crouching:
                IsPassable = !HasProbeCollision(Position + Vector3.up * (_dimensions.groundOffset + _dimensions.heightCrouching / 2), _dimensions.capsuleRadius, _dimensions.heightCrouching);
                break;

            case ProbeState.Jumping:
                IsPassable = !HasProbeCollision(Position + Vector3.up * (_dimensions.groundOffset + _dimensions.heightJumping + _dimensions.heightStanding / 2), _dimensions.capsuleRadius, _dimensions.heightStanding);
                break;
        }

        UpdateColor();
    }

    private bool HasProbeCollision(Vector3 center, float radius, float totalHeight)
    {
        if (radius <= 0 || totalHeight < 2 * radius)
            return false;

        float halfTotalHeight = totalHeight / 2f;
        Vector3 bottomSphereCenter = center - Vector3.up * (halfTotalHeight - radius);
        Vector3 topSphereCenter = center + Vector3.up * (halfTotalHeight - radius);

        Collider[] colliders = Physics.OverlapCapsule(bottomSphereCenter, topSphereCenter, radius, ObstacleLayers, QueryTriggerInteraction.Ignore);

        foreach (var col in colliders)
        {
            CollidingObjects.Add(col.name);
        }

        return colliders.Length > 0;
    }

    public void DrawProbe()
    {
        // Основной стиль для заголовка пробы
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.UpperCenter,
            normal = { textColor = VisualizeColor }
        };

        string probeName = $"Probe №{ProbeIndex}";
        Vector3 labelPosition = Vector3.zero;
        Vector3 capsuleCenter = Vector3.zero;

        switch (State)
        {
            case ProbeState.Standing:
                capsuleCenter = Position + Vector3.up * (_dimensions.groundOffset + _dimensions.heightStanding / 2);
                labelPosition = capsuleCenter + Vector3.up * (_dimensions.heightStanding / 2 + 0.5f);
                GeometryShapesDrawer.DrawWireCapsule(capsuleCenter, _dimensions.capsuleRadius, _dimensions.heightStanding, Quaternion.identity, VisualizeColor, Position);
                Handles.Label(labelPosition, $"{probeName}", headerStyle);
                break;

            case ProbeState.Crouching:
                capsuleCenter = Position + Vector3.up * (_dimensions.groundOffset + _dimensions.heightCrouching / 2);
                labelPosition = capsuleCenter + Vector3.up * (_dimensions.heightCrouching / 2 + 0.5f);
                GeometryShapesDrawer.DrawWireCapsule(capsuleCenter, _dimensions.capsuleRadius, _dimensions.heightCrouching, Quaternion.identity, VisualizeColor, Position);
                Handles.Label(labelPosition, $"{probeName}", headerStyle);
                break;

            case ProbeState.Jumping:
                capsuleCenter = Position + Vector3.up * (_dimensions.groundOffset + _dimensions.heightJumping + _dimensions.heightStanding / 2);
                labelPosition = capsuleCenter + Vector3.up * (_dimensions.heightStanding / 2 + 0.5f);
                GeometryShapesDrawer.DrawWireCapsule(capsuleCenter, _dimensions.capsuleRadius, _dimensions.heightStanding, Quaternion.identity, VisualizeColor, Position);
                Handles.Label(labelPosition, $"{probeName}", headerStyle);
                break;
        }

        string collisionStatus = $"{State}: {(IsPassable ? "✔ Pass" : "❌ Blocked")}\n";

        // Отображение названий обьектов столкновений коллизий
        if (!IsPassable && CollidingObjects.Count > 0)
        {
            collisionStatus += "- - - - -\n";
            for (int i = 0; i < CollidingObjects.Count; i++)
                collisionStatus += $"{CollidingObjects[i]}\n";
        }

        // Статус проходимости
        Handles.Label(Position + Vector3.down * 0.3f, collisionStatus, headerStyle);
    }
}


[System.Serializable]
public class PlayerDimensions
{
    public float heightStanding = 2f;
    public float heightCrouching = 1.2f;
    public float heightJumping = 1.5f;
    public float capsuleRadius = 0.5f;
    public float groundOffset = 0.1f;
}