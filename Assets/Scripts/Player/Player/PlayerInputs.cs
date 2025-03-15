using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputs : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool sprint;
    public bool crouch;

    public bool isInteract = false;

    public event Action<bool> OnChangeVisibilityUiPlayer; 

    public bool isEnableUiPlayer = false;

    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value)
    {

        LookInput(value.Get<Vector2>());
    }

    public void OnSprint(InputValue value)
    {
        SprintInput(value.isPressed);
    }

    public void OnCrouch(InputValue value)
    {
        CrouchInput(value.isPressed);
    }

    public void OnInteract(InputValue value)
    {
        InteractInput(value.isPressed);
    }

    public void OnToggleUiPlayer(InputValue value)
    {
        ToggleUiPlayer();
        OnChangeVisibilityUiPlayer?.Invoke(isEnableUiPlayer);
    }

    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    public void CrouchInput(bool newCrouchState)
    {
        crouch = newCrouchState;
    }

    public void InteractInput(bool b)
    {
        isInteract = b;
    }

    public void ToggleUiPlayer()
    {
        isEnableUiPlayer = !isEnableUiPlayer;
    }
}

