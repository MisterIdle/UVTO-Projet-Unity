using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 10f;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _grabPoint;
    [SerializeField] private float _grabDistance = 2f;
    [SerializeField] private float _interactionDistance = 5f;

    public int Score;

    private UIManager _uiManager;
    private Rigidbody _rigidbody;
    private Vector2 _movementInput;
    private Vector3 _velocity;
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

    private void FixedUpdate()
    {
        MovePlayer();
        _uiManager.SetScoreText(Score);
    }

    private void LateUpdate()
    {        
        UpdateInteractionUI();

        if (_isGrabbing && _grabbable != null)
        {
            _grabbable.Grab(_grabPoint);
        }

        if (_isGrabbing)
        {
            float distance = Vector3.Distance(_grabPoint.position, _grabbable.transform.position);

            if (distance > _grabDistance)
            {
                ReleaseGrabbable();
            }
        }
    }

    private void MovePlayer()
    {
        Vector3 direction = new Vector3(_movementInput.x, 0f, _movementInput.y);
        direction = Vector3.ClampMagnitude(direction, 1f);

        Vector3 targetVelocity = direction * _speed;
        _velocity = Vector3.Lerp(_velocity, targetVelocity, Time.deltaTime * (_velocity == Vector3.zero ? _acceleration : _deceleration));

        Vector3 moveDirection = _cameraTransform.TransformDirection(_velocity);
        moveDirection.y = 0f;

        _rigidbody.linearVelocity = moveDirection;
    }

    private void UpdateInteractionUI()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _interactionDistance))
        {
            if (hit.collider.TryGetComponent(out Grabbable targetGrabbable))
            {
                _uiManager.SetCrosshair(true);
                
                if(targetGrabbable.CanBeBorrowed)
                {
                    _uiManager.SetInteractionText("(E) Borrow : " + targetGrabbable.name);
                }
                else
                {
                    _uiManager.SetInteractionText("(E) Grab");
                }

                return;
            }

            if (hit.collider.TryGetComponent(out Interactive targetInteractive))
            {
                _uiManager.SetCrosshair(true);
                _uiManager.SetInteractionText("(E) Interact");
                return;
            }
        }
        
        _uiManager.SetCrosshair(false);
        _uiManager.SetInteractionText("");
    }

    private void Interact()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _interactionDistance))
        {
            Grabbable targetGrabbable = hit.collider.GetComponent<Grabbable>();
            if (targetGrabbable != null)
            {               
                if (targetGrabbable.CanBeBorrowed)
                {
                    targetGrabbable.Borrow();
                    return;
                }

                if (_isGrabbing && _grabbable == targetGrabbable)
                {
                    ReleaseGrabbable();
                }
                
                else if (!_isGrabbing)
                {
                    GrabGrabbable(targetGrabbable);
                }

                return;
            }

            Interactive targetInteractive = hit.collider.GetComponent<Interactive>();
            if (targetInteractive != null)
            {
                InteractWith(targetInteractive);
                return;
            }
        }

        if (_isGrabbing)
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

    private void InteractWith(Interactive targetInteractive)
    {
        targetInteractive.Interact();
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
