using System.Collections.Generic;
using UnityEngine;


public class FreeMovement : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> _disableComponents;
    [SerializeField] private float _speed;

    private Rigidbody _rb;
    private bool _isActive = false;

    private void Awake()
    {
        TryGetComponent(out _rb);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            _isActive = !_isActive;

            if (_isActive)
            {
                foreach (var item in _disableComponents)
                    item.enabled = false;

                if (_rb != null)
                    _rb.isKinematic = true;
            }
            else
            {
                foreach (var item in _disableComponents)
                    item.enabled = true;

                if (_rb != null)
                    _rb.isKinematic = false;
            }
        }

        if (_isActive)
        {
            _speed += Input.GetAxisRaw("Mouse ScrollWheel");
            _speed = Mathf.Max(0.1f, _speed);

            Vector3 move = Camera.main.transform.rotation * new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            if (Input.GetKey(KeyCode.LeftShift))
                move.y = 1;
            else if (Input.GetKey(KeyCode.LeftControl))
                move.y = -1;

            move.Normalize();
            transform.position += move * (_speed * Time.deltaTime);
        }
    }
}
