using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _grabPoint;

    private UIManager _uiManager;
    private Rigidbody _rigidbody;
    private Vector2 _movementInput;
    private Grabbable _grabbable;
    private bool _isGrabbing;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _cameraTransform = Camera.main.transform;
        _uiManager = FindFirstObjectByType<UIManager>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {        
        UpdateInteractionUI();

        if (_isGrabbing && _grabbable != null)
        {
            _grabbable.Grab(_grabPoint);
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void UpdateInteractionUI()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, 5f))
        {
            Grabbable targetGrabbable = hit.collider.GetComponent<Grabbable>();
            if (targetGrabbable != null)
            {
                _uiManager.SetCrosshair(true);
                _uiManager.SetInteractionText(_isGrabbing ? "(E) Drop" : "(E) Grab");
                return;
            }
        }
        
        _uiManager.SetCrosshair(false);
        _uiManager.SetInteractionText("");
    }

    private void MovePlayer()
    {
        Vector3 movement = new Vector3(_movementInput.x, 0f, _movementInput.y);
        Vector3 moveDirection = _cameraTransform.TransformDirection(movement);
        moveDirection.y = 0f;

        Vector3 targetPosition = _rigidbody.position + moveDirection * (_speed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(Vector3.Lerp(_rigidbody.position, targetPosition, 0.1f));
    }

    private void Interact()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, 5f))
        {
            Grabbable targetGrabbable = hit.collider.GetComponent<Grabbable>();
            if (targetGrabbable != null)
            {
                if (_isGrabbing && _grabbable == targetGrabbable)
                {
                    ReleaseGrabbable();
                }
                else if (!_isGrabbing)
                {
                    GrabGrabbable(targetGrabbable);
                }
            }
        }
        else if (_isGrabbing)
        {
            ReleaseGrabbable();
        }
    }

    private void GrabGrabbable(Grabbable targetGrabbable)
    {
        _grabbable = targetGrabbable;
        _isGrabbing = true;
    }

    private void ReleaseGrabbable()
    {
        _grabbable.Release();
        _grabbable = null;
        _isGrabbing = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Interact();
        }
    }
}
