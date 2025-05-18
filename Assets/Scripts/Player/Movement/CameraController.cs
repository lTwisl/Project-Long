using EditorAttributes;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    [SerializeField, Required] private GameObject _cinemachineCameraTarget;
    [SerializeField] private float _topClamp = 89.0f;
    [SerializeField] private float _bottomClamp = -89.0f;
    [SerializeField] private float _rotationSpeed = 1.0f;

    private float _cinemachineTargetPitch;
    private float _rotationVelocity;
    private const float _threshold = 0.01f;

    private PlayerInputs _input;

    private void Awake()
    {
        _input = GetComponent<PlayerInputs>();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (_input.look.sqrMagnitude < _threshold)
            return;

        _cinemachineTargetPitch += _input.look.y * _rotationSpeed;
        _rotationVelocity = _input.look.x * _rotationSpeed;

        _cinemachineTargetPitch = Utility.ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        _cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

        transform.Rotate(Vector3.up * _rotationVelocity);
    }
}

