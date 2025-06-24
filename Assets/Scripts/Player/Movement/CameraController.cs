using EditorAttributes;
using ModestTree.Util;
using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    [SerializeField, Required] private GameObject _cinemachineCameraTarget;
    [SerializeField] private float _topClamp = 89.0f;
    [SerializeField] private float _bottomClamp = -89.0f;
    [SerializeField] private float _rotationSpeed = 1.0f;

    public Vector3 InitCameraPos { get; private set; }
    private float _cinemachineTargetPitch;
    private float _rotationVelocity;
    private const float _threshold = 0.01f;

    private InputReader _input;


    private void Awake()
    {
        _input = GetComponent<InputReader>();

        InitCameraPos = _cinemachineCameraTarget.transform.localPosition;
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
        _cinemachineCameraTarget.transform.localPosition = InitCameraPos + offset;
    }

    public void SmoothMove(Vector3 offset, float maxDistanceDelta, Action onGoal = null)
    {
        StartCoroutine(SmoothToTargetCoroutine(_cinemachineCameraTarget.transform.localPosition + offset, maxDistanceDelta, onGoal));
    }

    public void SmoothToTarget(Vector3 localTarget, float maxDistanceDelta, Action onGoal = null)
    {
        StartCoroutine(SmoothToTargetCoroutine(localTarget, maxDistanceDelta, onGoal));
    }

    private IEnumerator SmoothToTargetCoroutine(Vector3 localTarget, float maxDistanceDelta, Action onGoal)
    {
        while (_cinemachineCameraTarget.transform.localPosition != localTarget)
        {
            _cinemachineCameraTarget.transform.localPosition = Vector3.MoveTowards(_cinemachineCameraTarget.transform.localPosition, localTarget, maxDistanceDelta);
            yield return null;
        }

        onGoal?.Invoke();
    }
}

