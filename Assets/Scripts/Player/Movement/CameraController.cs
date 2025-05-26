using EditorAttributes;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    [SerializeField, Required] private GameObject _cinemachineCameraTarget;
    [SerializeField] private float _topClamp = 89.0f;
    [SerializeField] private float _bottomClamp = -89.0f;
    [SerializeField] private float _rotationSpeed = 1.0f;

    private Vector3 _initCameraPos;
    private float _cinemachineTargetPitch;
    private float _rotationVelocity;
    private const float _threshold = 0.01f;

    private InputReader _input;


    private void Awake()
    {
        _input = GetComponent<InputReader>();

        _initCameraPos = _cinemachineCameraTarget.transform.localPosition;
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (_input.Look.sqrMagnitude < _threshold)
            return;

        _cinemachineTargetPitch += _input.Look.y * _rotationSpeed;
        _rotationVelocity = _input.Look.x * _rotationSpeed;

        _cinemachineTargetPitch = Utility.ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        _cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

        transform.Rotate(Vector3.up * _rotationVelocity);
    }

    public void SetCameraOffset(Vector3 offset)
    {
        _cinemachineCameraTarget.transform.localPosition = _initCameraPos + offset;
    }
}

