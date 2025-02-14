using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("General Settings")]
    public int Health = 4;
    public bool IsDead;

    [Header("Movement Settings")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 10f;

    [Header("Crouch Settings")]
    [SerializeField] private float _normalHeight = 1.6f;
    [SerializeField] private float _crouchHeight = 1f;
    [SerializeField] private float _crouchSpeed = 2.5f;
    [SerializeField] private float _crouchDuration = 2f;

    [Header("Interaction Settings")]
    public Transform GrabPoint;
    [SerializeField] private float _interactionDistance = 2f;
    [SerializeField] private float _grabMaxDistance = 2f;

    [Header("Camera Settings")]
    public Transform CameraTransform;
    public Transform CameraHeadTransform;
    public Transform HeadTransform;
    public Transform DeathCameraTransform;

    [Header("Character Model")]
    public GameObject CharacterModel;
    public Animator CharacterAnimator;

    private Rigidbody _rb;
    private Vector2 _movementInput;
    private Vector3 _velocity;
    public Grabbable CurrentGrabbable;
    private bool _isCrouching;
    public bool IsGrabbing;

    private UIManager _uiManager;
    private ListPanel _listPanel;

    private void Start()
    {
        if (!TryGetComponent(out _rb))
        {
            Debug.LogError("Rigidbody component missing from the player.");
            enabled = false;
            return;
        }

        _uiManager = FindFirstObjectByType<UIManager>();
        _listPanel = FindFirstObjectByType<ListPanel>();
        CameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (IsDead) return;

        UpdateHeadPosition();
        UpdateCharacterModelDirection();
    }

    private void LateUpdate()
    {
        if (IsDead) return;

        CheckGrabbableDistance();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        UpdateInteractionUI();
        MovePlayer();
    }

    private void UpdateHeadPosition()
    {  
        HeadTransform.position = CameraHeadTransform.position;
    }

    private void UpdateCharacterModelDirection()
    {
        Vector3 targetDirection = new Vector3(CameraTransform.forward.x, 0, CameraTransform.forward.z);
        CharacterModel.transform.forward = Vector3.Lerp(CharacterModel.transform.forward, targetDirection, Time.deltaTime * _acceleration);
    }

    private void MovePlayer()
    {
        Vector3 direction = new Vector3(_movementInput.x, 0f, _movementInput.y).normalized;
        float currentSpeed = _isCrouching ? _crouchSpeed : _speed;
        Vector3 targetVelocity = direction * currentSpeed;
        _velocity = Vector3.Lerp(_velocity, targetVelocity, Time.deltaTime * (_velocity == Vector3.zero ? _acceleration : _deceleration));

        CharacterAnimator.SetFloat("Speed", _velocity.magnitude);

        Vector3 moveDirection = CameraTransform.TransformDirection(_velocity);
        moveDirection.y = _rb.linearVelocity.y;

        _rb.linearVelocity = moveDirection;

        HeadTransform.rotation = Quaternion.Lerp(HeadTransform.rotation, CameraTransform.rotation, Time.deltaTime * _acceleration);
    }

    private void Crouch()
    {
        _isCrouching = !_isCrouching;
        CharacterAnimator.SetBool("IsCrouch", _isCrouching);

        StopAllCoroutines();
        StartCoroutine(LerpCrouch(_isCrouching ? _crouchHeight : _normalHeight));
    }

    private IEnumerator LerpCrouch(float targetHeight)
    {
        float startHeight = CameraHeadTransform.localPosition.y;
        float elapsedTime = 0f;

        while (elapsedTime < _crouchDuration)
        {
            float newY = Mathf.Lerp(startHeight, targetHeight, elapsedTime / _crouchDuration);
            CameraHeadTransform.localPosition = new Vector3(0, newY, 0.6f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        CameraHeadTransform.localPosition = new Vector3(0, targetHeight, 0.6f);
    }

    private void Interact()
    {
        if (Physics.Raycast(CameraTransform.position, CameraTransform.forward, out RaycastHit hit, _interactionDistance))
        {
            if (hit.collider.TryGetComponent(out Interactive interactive))
            {
                interactive.Interact();
            }
            else if (hit.collider.TryGetComponent(out Collectible collectible))
            {
                collectible.Collect();
            }
        }
    }

    private void DropAll()
    {
        CurrentGrabbable?.Drop();
        IsGrabbing = false;
    }

    private void UpdateInteractionUI()
    {
        if (IsGrabbing)
        {
            _uiManager.SetCrosshair(true);
            _uiManager.SetInteractionText("(E) Drop");
            return;
        }   

        if (Physics.Raycast(CameraTransform.position, CameraTransform.forward, out RaycastHit hit, _interactionDistance))
        {
            if (hit.collider.TryGetComponent<Grabbable>(out _))
                SetUI("(E) Grab");
            else if (hit.collider.TryGetComponent<Borrowable>(out _))
                SetUI("(E) Borrow");
            else if (hit.collider.TryGetComponent<Interactive>(out _))
                SetUI("(E) Interact");
            else
                ClearUI();
        }

        else
            ClearUI();
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

    public void AddScore(float score)
    {
        GameManager.Instance.Score += score;
        _uiManager.UpdateScore(GameManager.Instance.Score);
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;

        Debug.Log($"Player took {damage} damage. Health: {Health}");

        if (Health <= 0 && !IsDead) {
            CharacterAnimator.applyRootMotion = true;
            CharacterAnimator.SetTrigger("Die");

            IsDead = true;

            if (IsGrabbing)
                DropAll();
        }
    }

    private void CheckGrabbableDistance()
    {
        if (IsGrabbing && CurrentGrabbable != null && Vector3.Distance(GrabPoint.position, CurrentGrabbable.transform.position) > _grabMaxDistance)
        {
            DropAll();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsDead)
            _movementInput = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started && !IsDead)
        {
            if (IsGrabbing)
                DropAll();
            else
                Interact();
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.started && !IsDead)
            Crouch();
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        if (context.started && IsDead)
            GameManager.Instance.RestartGame();
    }
}
