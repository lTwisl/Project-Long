using UnityEngine;
using UnityEngine.UI;
using Zenject;


public class InteractionController : MonoBehaviour
{
    [SerializeField] private float _holdInteractionTime = 2f;
    [SerializeField] private LayerMask _interactionLayer;
    [SerializeField] private Slider _slider;

    [Inject] private Player _player;

    private PlayerInputs _inputs => _player.PlayerInputs;
    private Camera _mainCamera => _player.MainCamera;

    private IInteractible _currentInteractible;
    private bool _isInteracting;
    private float _holdTimer;

    [SerializeField] private float _distRaycast = 100f;

    public void Update()
    {
        if (_inputs.isInteract)
        {
            if (!_isInteracting)
                StartInteraction();

            UpdateHoldProgress(Time.deltaTime);
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
//#if UNITY_EDITOR
//        if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out RaycastHit hitInfo1, _distRaycast))
//            Debug.Log($"Попытка провзаимодействовать с {hitInfo1.transform.name}");
//#endif

        if (!Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out RaycastHit hitInfo, _distRaycast, _interactionLayer))
            return;

        _currentInteractible = hitInfo.collider.GetComponentInParent<IInteractible>();
        if (_currentInteractible == null)
            return;

        _isInteracting = true;

        if (!_currentInteractible.IsCanInteract)
            return;

        switch (_currentInteractible.InteractionType)
        {
            case InteractionType.Click:

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
