using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _crouchSpeed = 2.5f;
    [SerializeField] private float _runSpeed = 10f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 10f;
    [SerializeField] private float _crouchHeight = 0.5f;

    [Header("Interaction Settings")]
    [SerializeField] private Transform _grabPoint;
    [SerializeField] private float _grabDistance = 1f;
    [SerializeField] private float _interactionDistance = 2f;

    [Header("List Settings")]
    [SerializeField] private GameObject _ObjectsList;
    [SerializeField] private List<Borrowable> _borrowedObjects = new List<Borrowable>();

    [Header("Camera Settings")]
    public Transform CameraTransform;
    public Transform HeadTransform;

    [Header("Character Model")]
    public GameObject CharacterModel;
    public Animator CharacterAnimator;
    
    private float _originalHeight;
    public float Score { get; private set; }

    private UIManager _uiManager;
    private UIList _uilist;
    private Rigidbody _rb;
    private Vector2 _movementInput;
    private Vector3 _velocity;
    private Grabbable _grabbable;
    private bool _isGrabbing;
    private bool _isCrouching;
    private bool _isRunning;
    private CapsuleCollider _playerCollider;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _uiManager = FindFirstObjectByType<UIManager>();
        _uilist = GetComponentInChildren<UIList>();
        _playerCollider = GetComponent<CapsuleCollider>();
        _originalHeight = _playerCollider.height;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetList();
    }

    void Update()
    {
        CharacterModel.transform.forward = Vector3.Lerp(CharacterModel.transform.forward, new Vector3(CameraTransform.forward.x, 0, CameraTransform.forward.z), Time.deltaTime * _acceleration);
    }

    private void FixedUpdate()
    {
        MovePlayer();
        _uilist.UpdateList(_borrowedObjects);
    }

    private void LateUpdate()
    {
        UpdateInteractionUI();

        if (_isGrabbing && _grabbable != null)
        {
            _grabbable.Grab(_grabPoint);
            if (Vector3.Distance(_grabPoint.position, _grabbable.transform.position) > _grabDistance)
            {
                ReleaseGrabbable();
            }
        }
    }

    private void MovePlayer()
    {
        Vector3 direction = new Vector3(_movementInput.x, 0f, _movementInput.y).normalized;
        float currentSpeed = _isCrouching ? _crouchSpeed : (_isRunning ? _runSpeed : _speed);
        Vector3 targetVelocity = direction * currentSpeed;
        _velocity = Vector3.Lerp(_velocity, targetVelocity, Time.deltaTime * (_velocity == Vector3.zero ? _acceleration : _deceleration));

        CharacterAnimator.SetFloat("Speed", _velocity.magnitude);

        Vector3 moveDirection = CameraTransform.TransformDirection(_velocity);
        moveDirection.y = _rb.linearVelocity.y;

        _rb.linearVelocity = moveDirection;

        HeadTransform.rotation = Quaternion.Lerp(HeadTransform.rotation, CameraTransform.rotation, Time.deltaTime * _acceleration);
    }

    private void UpdateInteractionUI()
    {
        if (_isGrabbing)
        {
            _uiManager.SetCrosshair(true);
            _uiManager.SetInteractionText("(E) Drop");
            return;
        }

        if (Physics.Raycast(CameraTransform.position, CameraTransform.forward, out RaycastHit hit, _interactionDistance))
        {
            if (hit.collider.TryGetComponent(out Grabbable g))
                SetUI("(E) Grab");
            else if (hit.collider.TryGetComponent(out Borrowable b))
                SetUI("(E) Borrow");
            else if (hit.collider.TryGetComponent(out Interactive i))
                SetUI("(E) Interact");
            else
                ClearUI();
        }
        else
        {
            ClearUI();
        }
    }

    private void SetUI(string text)
    {
        _uiManager.SetCrosshair(true);
        _uiManager.SetInteractionText(text);
    }

    private void ClearUI()
    {
        _uiManager.SetCrosshair(false);
        _uiManager.SetInteractionText("");
    }

    private void Interact()
    {
        if (_isGrabbing)
        {
            ReleaseGrabbable();
            return;
        }

        if (Physics.Raycast(CameraTransform.position, CameraTransform.forward, out RaycastHit hit, _interactionDistance))
        {
            if (hit.collider.TryGetComponent(out Grabbable g))
                GrabGrabbable(g);
            else if (hit.collider.TryGetComponent(out Interactive i))
                Interactive(i);
            else if (hit.collider.TryGetComponent(out Borrowable b))
                Borrow(b);
        }
    }

    private void Couch()
    {
        _isCrouching = !_isCrouching;
        _playerCollider.height = _isCrouching ? _crouchHeight : _originalHeight;
    }

    private void Run()
    {
        _isRunning = !_isRunning;
    }

    private void SetList() 
    {
        foreach (Borrowable obj in _ObjectsList.GetComponentsInChildren<Borrowable>())
        {
            Debug.Log(obj);
            _borrowedObjects.Add(obj);
        }
    }

    private void GrabGrabbable(Grabbable target)
    {
        _grabbable = target;
        _isGrabbing = true;
    }

    private void ReleaseGrabbable()
    {
        _grabbable?.Release();
        _grabbable = null;
        _isGrabbing = false;
    }

    private void Interactive(Interactive target)
    {
        target.Interact();
    }

    private void Borrow(Borrowable target)
    {
        target.Borrow();

        _uiManager.SetScoreText(Score);
    }


    public void AddScore(float score)
    {
        Score += score;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
            Interact();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.started)
            Couch();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
            Run();
        else if (context.canceled)
            Run();
    }
}
