using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;


public class PlayerInputs : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool sprint;
    public bool crouch;
    public bool jump;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    public bool isInteract = false;

    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook)
        {
            LookInput(value.Get<Vector2>());
        }
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

    private void OnApplicationFocus(bool hasFocus)
    {
        //SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}

