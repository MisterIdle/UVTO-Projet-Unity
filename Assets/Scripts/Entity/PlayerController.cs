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
    public bool isWalking;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 10f;

    [Header("Crouch Settings")]
    [SerializeField] private float _normalHeight = 1.6f;
    [SerializeField] private float _crouchHeight = 1f;
    [SerializeField] private float _crouchSpeed = 2.5f;
    [SerializeField] private float _crouchDuration = 2f;
    public bool IsCrouching;
    public Grabbable CurrentGrabbable;

    [Header("Interaction Settings")]
    public Transform GrabPoint;
    [SerializeField] private float _interactionDistance = 2f;
    [SerializeField] private float _grabMaxDistance = 2f;
    public bool IsGrabbing;

    [Header("Camera Settings")]
    public Transform CameraTransform;
    public Transform CameraHeadTransform;
    public Transform HeadTransform;
    public Transform Model;

    [Header("Damage Settings")]
    [SerializeField] private Volume _vignette;
    [SerializeField] private float _damageCooldown = 1f;

    [Header("Enemy Settings")]
    [SerializeField] public Enemy _enemy;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip _footstepSound;
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] public AudioClip CollisionSound;
    [SerializeField] public AudioClip BorrowSound;

    private Rigidbody _rb;
    private Vector2 _movementInput;
    private Vector3 _velocity;
    private Coroutine _quitCoroutine;

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
        _listPanel.UpdateList(GameManager.Instance.BorrowedObjectsList);

        UpdateCharacterModelDirection();
        UpdateHeadPosition();
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
        Vector3 targetDirection = new Vector3(CameraTransform.forward.x, 0, CameraTransform.forward.z).normalized;
        Model.transform.forward = Vector3.Slerp(Model.transform.forward, targetDirection, Time.deltaTime * _acceleration);
    }

    private void MovePlayer()
    {
        Vector3 direction = new Vector3(_movementInput.x, 0f, _movementInput.y).normalized;
        float currentSpeed = IsCrouching ? _crouchSpeed : _speed;
        Vector3 targetVelocity = direction * currentSpeed;
        _velocity = Vector3.Lerp(_velocity, targetVelocity, Time.deltaTime * (_velocity == Vector3.zero ? _acceleration : _deceleration));  

        isWalking = _velocity.magnitude > 0.1f;  

        Vector3 moveDirection = CameraTransform.TransformDirection(_velocity);
        moveDirection.y = _rb.linearVelocity.y; 

        if (_rb.linearVelocity.y < 0)
        {
            moveDirection.y += Physics.gravity.y * 2f * Time.deltaTime;
        }

        _rb.linearVelocity = moveDirection; 

        HeadTransform.rotation = Quaternion.Lerp(HeadTransform.rotation, CameraTransform.rotation, Time.deltaTime * _acceleration); 

        PlayFootstepSound();
    }

    private void PlayFootstepSound()
    {
        if (isWalking && !IsCrouching)
            SoundManager.Instance.PlayOneTimeSound(_footstepSound, transform, 0.2f);
    }

    private void Crouch()
    {
        IsCrouching = !IsCrouching;

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
                SetUI("(E) Borrow " + hit.collider.name);
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
        SoundManager.Instance.PlaySound(_hitSound, transform, 1f);

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

        if (IsGrabbing)
            DropAll();

        _listPanel.Hide();

        _uiManager.SetGameOverText("Game Over");
        _uiManager.ShowEndGameText();
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

    public void OnQuit(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _quitCoroutine = StartCoroutine(QuitCountdown());
        }
        else if (context.canceled)
        {
            if (_quitCoroutine != null)
            {
                StopCoroutine(_quitCoroutine);
                _quitCoroutine = null;
                _uiManager.SetQuitText("");
            }
        }
    }

    private IEnumerator QuitCountdown()
    {
        for (int i = 3; i > 0; i--)
        {
            _uiManager.SetQuitText("Quitting in " + i);
            yield return new WaitForSeconds(1f);
        }

        GameManager.Instance.StopGame();
    }
}
