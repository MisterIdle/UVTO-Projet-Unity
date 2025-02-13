using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 4f;
    [SerializeField] private float _crouchSpeed = 2.5f;
    [SerializeField] private float _runSpeed = 6f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 10f;
    [SerializeField] private float _crouchHeight = 0.5f;

    [Header("Interaction Settings")]
    [SerializeField] private float _grabDistance = 2f;
    public Transform GrabPoint;
    [SerializeField] private float _interactionDistance = 2f;
    [SerializeField] private float _interactionAngle = 45f;

    [Header("List Settings")]
    [SerializeField] private GameObject _ObjectsList;
    [SerializeField] private List<Borrowable> _borrowedObjects = new List<Borrowable>();

    [Header("Camera Settings")]
    public Transform CameraTransform;
    public Transform HeadTransform;

    [Header("Character Model")]
    public GameObject CharacterModel;
    public Animator CharacterAnimator;
    
    public float Score;
    private Rigidbody _rb;
    private Vector2 _movementInput;
    private Vector3 _velocity;
    public Grabbable CurrentGrabbable;
    private bool _isCrouching;
    private bool _isRunning;
    public bool IsGrabbing;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HeadTransform.position = CameraTransform.position;
        CharacterModel.transform.forward = Vector3.Lerp(CharacterModel.transform.forward, new Vector3(CameraTransform.forward.x, 0, CameraTransform.forward.z), Time.deltaTime * _acceleration);
    }

    private void LateUpdate()
    {
        if (IsGrabbing && CurrentGrabbable != null)
            if (Vector3.Distance(GrabPoint.position, CurrentGrabbable.transform.position) > _grabDistance)
                DropAll();
    }

    private void FixedUpdate()
    {
        MovePlayer();
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

    private void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(CameraTransform.position, CameraTransform.forward, out hit, _interactionDistance))
        {
            if (hit.collider.TryGetComponent(out Interactive interactive))
                interactive.Interact();

            if (hit.collider.TryGetComponent(out Collectible collectible))
                collectible.Collect();
        }
    }

    private void DropAll()
    {
        if (CurrentGrabbable != null)
            CurrentGrabbable.Drop();

        IsGrabbing = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (IsGrabbing)
                DropAll();
            else
                Interact();
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
    }

    public void OnRun(InputAction.CallbackContext context)
    {
    }
}
