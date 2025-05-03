using UnityEngine;
using UnityEngine.UI;
using Zenject;


public class InteractionController : MonoBehaviour
{
    [SerializeField] private float _interactionDistance = 10f;
    [SerializeField] private float _holdInteractionTime = 2f;
    [SerializeField] private LayerMask _interactionLayer;
    [SerializeField] private Slider _slider;

    [Inject] private Player _player;

    private PlayerInputs _inputs => _player.PlayerInputs;
    private Camera _mainCamera => _player.MainCamera;

    private IInteractible _currentInteractible;
    private bool _isInteracting;
    private float _holdTimer;

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
        if (!Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out RaycastHit hitInfo, _interactionDistance, _interactionLayer))
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

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out RaycastHit hitInfo, 100f))
        {
            Rect rect = new Rect(Screen.width - 200, 20, 200, 40);
            GUI.Label(rect, $"Наблюдаемый объект: {hitInfo.transform.name}", new GUIStyle()
            {
                fontSize = 32,
                alignment = TextAnchor.UpperRight,
                normal = { textColor = Color.red }
            });

            Rect rect2 = new Rect(Screen.width - 200, 60, 200, 40);
            if (hitInfo.transform.parent)
                GUI.Label(rect2, $"Родитель: {hitInfo.transform.parent.name}", new GUIStyle()
                {
                    fontSize = 24,
                    alignment = TextAnchor.UpperRight,
                    normal = { textColor = Color.red }
                });
        }
    }
#endif
}
