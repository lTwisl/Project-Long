using UnityEngine;
using UnityEngine.UI;


public class InteractionController
{
    private float _holdInteractionTime = 2f;

    private Player _player;
    private PlayerInputs _inputs;
    private GameObject _mainCamera;
    private Slider _slider;

    private IInteractible _currentInteractible;
    private bool _isInteracting;
    private float _holdTimer;

    public InteractionController(Player player, Slider slider, float holdInteractionTime)
    {
        _player = player;
        _inputs = player.PlayerInputs;
        _mainCamera = player.MainCamera;
        _slider = slider;

        _holdInteractionTime = holdInteractionTime;
    }

    public void Update(float deltaTime)
    {
        if (_inputs.isInteract)
        {
            if (!_isInteracting)
                StartInteraction();

            UpdateHoldProgress(deltaTime);
        }
        else
        {
            if (_isInteracting)
                CancelInteraction();
        }
    }

    private void CancelInteraction()
    {
        if (!_isInteracting)
            return;
        ResetInteraction();
    }

    private void StartInteraction()
    {
        if (!Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out RaycastHit hitInfo, 100f))
            return;

        if (!hitInfo.collider.TryGetComponent(out _currentInteractible))
            return;

        _isInteracting = true;
        
        if (!_currentInteractible.IsCanInteract)
            return;

        switch (_currentInteractible.InteractionType)
        {
            case InteractionType.Instant:

                _currentInteractible.Interact(_player);

                break;

            case InteractionType.Hold:

                _slider.gameObject.SetActive(true);
                _holdTimer = 0;
                _slider.value = 0;

                break;
        }
    }

    private void UpdateHoldProgress(float deltaTime)
    {
        if (!_isInteracting)
            return;

        _holdTimer += deltaTime;

        _slider.value = _holdTimer / _holdInteractionTime;

        if (_holdTimer >= _holdInteractionTime)
        {
            _currentInteractible.Interact(_player);
            ResetInteraction();
        }
    }

    private void ResetInteraction()
    {
        _isInteracting = false;
        _holdTimer = 0;
        _currentInteractible = null;
        _slider.value = 0;

        _slider.gameObject.SetActive(false);
    }
}
