using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Scriptable Objects/PlayerMovementConfig")]
public class PlayerMovementConfig : ScriptableObject
{
    [Header("Player")]
    [field: SerializeField] public float MoveSpeed { get; private set; } = 4.0f;
    [field: SerializeField] public float SprintSpeed { get; private set; } = 6.0f;
    [field: SerializeField] public float RotationSpeed { get; private set; } = 1.0f;
    [field: SerializeField] public float SpeedChangeRate { get; private set; } = 10.0f;
    [field: SerializeField] public AnimationCurve SpeedChangeRateCurve { get; private set; }

    [field: Header("Player Crouch")]
    [field: SerializeField] public float SpeedCrouch { get; private set; } = 1.0f;
    [field: SerializeField] public float CrouchTransitionSpeed { get; private set; } = 2.0f;
    [field: SerializeField] public float CrouchHeight { get; private set; } = 1.0f;

    [field: Header("Player Sliding")]
    [field: SerializeField] public float SlidingSpeed { get; private set; } = 2.0f;
    [field: SerializeField] public AnimationCurve SlidingSpeedCurve { get; private set; }
    [field: SerializeField] public float SlidingAngle { get; private set; } = 20.0f;
    [field: SerializeField] public AnimationCurve SlidingAngleCurve { get; private set; }
    [field: SerializeField] public float SlidingChangeRate { get; private set; } = 5.0f;

    [field: Space(10)]
    [field: SerializeField] public float Gravity { get; private set; } = -15.0f;

    [field: Header("Player Grounded")]
    [field: SerializeField] public float GroundedOffset { get; private set; } = -0.14f;
    [field: SerializeField] public float GroundedRadius { get; private set; } = 0.5f;
    [field: SerializeField] public LayerMask GroundLayers { get; private set; }

    [field: Header("Player Ceiling")]
    [field: SerializeField] public float CeilingedRadius { get; private set; } = 0.5f;
    [field: SerializeField] public float CeilingedOffset { get; private set; } = 0.2f;
    [field: SerializeField] public LayerMask CeilingedLayers { get; private set; }
}
