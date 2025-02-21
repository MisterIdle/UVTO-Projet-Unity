using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

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

    [Header("Damage Settings")]
    [SerializeField] private Volume _vignette;
    [SerializeField] private float _damageCooldown = 1f;

    [Header("Character Model")]
    [SerializeField] public GameObject _model;
    [SerializeField] public Animator _animator;

    [Header("Enemy Settings")]
    [SerializeField] public Enemy _enemy;

    private Rigidbody _rb;
    private Vector2 _movementInput;
    private Vector3 _velocity;
    public Grabbable CurrentGrabbable;
    public bool IsCrouching;
    public bool IsGrabbing;

    private UIManager _uiManager;
    private ListPanel _listPanel;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!TryGetComponent(out _rb))
        {
            Debug.LogError("Rigidbody component missing from the player.");
            enabled = false;
            return;
        }

        _uiManager = FindFirstObjectByType<UIManager>();
        _listPanel = FindFirstObjectByType<ListPanel>();
        _enemy = FindFirstObjectByType<Enemy>();

        CameraTransform = Camera.main.transform;
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
        _model.transform.forward = Vector3.Lerp(_model.transform.forward, targetDirection, Time.deltaTime * _acceleration);
    }

    private void MovePlayer()
    {
        Vector3 direction = new Vector3(_movementInput.x, 0f, _movementInput.y).normalized;
        float currentSpeed = IsCrouching ? _crouchSpeed : _speed;
        Vector3 targetVelocity = direction * currentSpeed;
        _velocity = Vector3.Lerp(_velocity, targetVelocity, Time.deltaTime * (_velocity == Vector3.zero ? _acceleration : _deceleration));

        _animator.SetFloat("Speed", _velocity.magnitude);

        Vector3 moveDirection = CameraTransform.TransformDirection(_velocity);
        moveDirection.y = _rb.linearVelocity.y;

        if (_rb.linearVelocity.y < 0)
        {
            moveDirection.y += Physics.gravity.y * 2f * Time.deltaTime;
        }

        _rb.linearVelocity = moveDirection;

        HeadTransform.rotation = Quaternion.Lerp(HeadTransform.rotation, CameraTransform.rotation, Time.deltaTime * _acceleration);
    }

    private void Crouch()
    {
        IsCrouching = !IsCrouching;
        _animator.SetBool("IsCrouch", IsCrouching);

        StopAllCoroutines();
        StartCoroutine(LerpCrouch(IsCrouching ? _crouchHeight : _normalHeight));
    }

    private IEnumerator LerpCrouch(float targetHeight)
    {
        float startHeight = CameraHeadTransform.localPosition.y;
        float elapsedTime = 0f;

        while (elapsedTime < _crouchDuration)
        {
            float newY = Mathf.Lerp(startHeight, targetHeight, elapsedTime / _crouchDuration);
            CameraHeadTransform.localPosition = new Vector3(0, newY, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        CameraHeadTransform.localPosition = new Vector3(0, targetHeight, 0);
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

    public void AddScore(float score)
    {
        GameManager.Instance.Score += score;
        _uiManager.UpdateScore(GameManager.Instance.Score);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        Health -= damage;
        HitEffect();

        if (Health <= 0)
            Die();
    }

    private void HitEffect()
    {
        if (_vignette.profile.TryGet(out Vignette vignette))
        {
            StartCoroutine(LerpVignetteIntensity(vignette, 1, 0, _damageCooldown));
        }
    }

    private IEnumerator LerpVignetteIntensity(Vignette vignette, float startIntensity, float endIntensity, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            vignette.intensity.value = Mathf.Lerp(startIntensity, endIntensity, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        vignette.intensity.value = endIntensity;
    }

    private void Die()
    {
        if (IsDead) return;

        IsDead = true;
        _animator.applyRootMotion = true;
        _animator.SetTrigger("Die");

        if (IsGrabbing)
            DropAll();

        _listPanel.Hide();

        //_uiManager.ShowDeathScreen();
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
