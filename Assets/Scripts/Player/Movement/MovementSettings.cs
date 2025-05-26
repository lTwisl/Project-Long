using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementSettings", menuName = "Scriptable Objects/MovementSettings")]
public class MovementSettings : ScriptableObject
{
    [field: Header("Movement")]
    [field: SerializeField, Min(0)] public float WalkSpeed { get; private set; } = 4f;
    [field: SerializeField, Min(0)] public float RunSpeed { get; private set; } = 6f;
    [field: SerializeField, Min(0)] public float MoveAcceleration { get; private set; } = 50f;
    [field: SerializeField, Min(0)] public float GroundDamping { get; private set; } = 5;

    [field: Header("Jump")]
    [field: SerializeField, Min(0)] public float JumpForce { get; private set; } = 5f;
    [field: SerializeField, Min(0)] public float JumpCooldown { get; private set; } = 0.25f;
    [field: SerializeField, Min(0)] public float AirAcceleration { get; private set; } = 0f;

    [field: Header("Crouch")]
    [field: SerializeField, Min(0)] public float CrouchSpeed { get; private set; } = 2f;
    [field: SerializeField, Min(0)] public float CrouchHeight { get; private set; } = 1.2f;
    [field: SerializeField, Min(0)] public float SpeedTransitionCrouch { get; private set; } = 1f;

    [field: Header("Slide")]
    [field: SerializeField, Min(0)] public float MaxSlideSpeed { get; private set; } = 20;
    [field: SerializeField, Min(0)] public float FreeSlideAngle { get; private set; } = 55;

    [field: Header("Wind")]
    [field: SerializeField] public bool UseWind { get; private set; } = true;
    [field: ShowField(nameof(UseWind)), Range(0f, 1f)]
    [field: SerializeField] public float EffectWindOnMaxSpeed { get; private set; } = 0.5f;
}
