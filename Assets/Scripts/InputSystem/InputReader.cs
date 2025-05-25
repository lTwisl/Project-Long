using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, GameInput.IGameplayActions, GameInput.IUIActions
{
    private GameInput _gameInput;

    private void Awake()
    {
        _gameInput = new GameInput();
        _gameInput.Gameplay.SetCallbacks(this);
        _gameInput.UI.SetCallbacks(this);
        _gameInput.Enable();
    }

    private void OnEnable()
    {
        SetGameplay();
    }

    private void OnDisable()
    {
        _gameInput.Gameplay.Disable();
        _gameInput.UI.Disable();
    }

    private void OnDestroy()
    {
        _gameInput.Dispose();
    }

    public void SetGameplay()
    {
        _gameInput.Gameplay.Enable();
        _gameInput.UI.Disable();
    }

    public void SetUI()
    {
        _gameInput.Gameplay.Disable();
        _gameInput.UI.Enable();
    }

    public Vector2 Move;
    public Vector2 Look;
    public bool IsRunning;
    public bool IsCrouching;
    public bool IsInteracting;

    public event Action<bool> OnChangedVisibilityUiPlayer;

    public bool isVisibilityUiPlayer = false;

    public event Action<bool> Jump;

    // === IGameplayActions ===
    public void OnCrouch(InputAction.CallbackContext context)
    {
        IsCrouching = context.phase != InputActionPhase.Canceled;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        IsInteracting = context.phase != InputActionPhase.Canceled;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump.Invoke(context.phase == InputActionPhase.Performed);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look = context.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move = context.ReadValue<Vector2>();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        // noop
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        IsRunning = context.phase == InputActionPhase.Performed;
    }

    public void OnShowUi(InputAction.CallbackContext context)
    {
        isVisibilityUiPlayer = true;
        OnChangedVisibilityUiPlayer?.Invoke(true);
        SetUI();
    }

    // === IUIActions ===
    public void OnHideUi(InputAction.CallbackContext context)
    {
        isVisibilityUiPlayer = false;
        OnChangedVisibilityUiPlayer?.Invoke(false);
        SetGameplay();
    }

    public void OnResume(InputAction.CallbackContext context)
    {
        // noop
    }
}
