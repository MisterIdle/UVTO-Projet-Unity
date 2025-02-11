using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private float grabDistance = 2f;
    [SerializeField] private float interactionDistance = 5f;

    public Transform cameraTransform;
    public float Score { get; private set; }

    private UIManager uiManager;
    private Rigidbody rb;
    private Vector2 movementInput;
    private Vector3 velocity;
    private Grabbable grabbable;
    private bool isGrabbing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        uiManager = FindFirstObjectByType<UIManager>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        uiManager.SetScoreText(Score);
    }

    private void LateUpdate()
    {
        UpdateInteractionUI();

        if (isGrabbing && grabbable != null)
        {
            grabbable.Grab(grabPoint);
            if (Vector3.Distance(grabPoint.position, grabbable.transform.position) > grabDistance)
            {
                ReleaseGrabbable();
            }
        }
    }

    private void MovePlayer()
    {
        Vector3 direction = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        Vector3 targetVelocity = direction * speed;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * (velocity == Vector3.zero ? acceleration : deceleration));

        Vector3 moveDirection = cameraTransform.TransformDirection(velocity);
        moveDirection.y = rb.linearVelocity.y;

        rb.linearVelocity = moveDirection;
    }

    private void UpdateInteractionUI()
    {
        if (isGrabbing)
        {
            uiManager.SetCrosshair(true);
            uiManager.SetInteractionText("(E) Drop");
            return;
        }

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactionDistance))
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
        uiManager.SetCrosshair(true);
        uiManager.SetInteractionText(text);
    }

    private void ClearUI()
    {
        uiManager.SetCrosshair(false);
        uiManager.SetInteractionText("");
    }

    private void Interact()
    {
        if (isGrabbing)
        {
            ReleaseGrabbable();
            return;
        }

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.TryGetComponent(out Grabbable g))
                GrabGrabbable(g);
            else if (hit.collider.TryGetComponent(out Interactive i))
                i.Interact();
            else if (hit.collider.TryGetComponent(out Borrowable b))
                b.Borrow();
        }
    }

    private void GrabGrabbable(Grabbable target)
    {
        grabbable = target;
        isGrabbing = true;
    }

    private void ReleaseGrabbable()
    {
        grabbable?.Release();
        grabbable = null;
        isGrabbing = false;
    }

    public void AddScore(float score)
    {
        Score += score;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
            Interact();
    }
}
