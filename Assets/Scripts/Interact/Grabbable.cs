using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody), typeof(MeshCollider), typeof(NavMeshObstacle))]
public class Grabbable : Collectible
{
    private Rigidbody _rigidbody;
    private Transform _grabPoint;
    private PlayerController _playerController;
    private MeshCollider _meshCollider;
    private NavMeshObstacle _navMeshObstacle;
    public float ScoreValue;

    [SerializeField] private float _lerpSpeed = 10f;
    [SerializeField] private float _releaseForceMultiplier = 2f;

    private Vector3 _velocity = Vector3.zero;

    private void Awake()
    {
        // Get required components
        _meshCollider = GetComponent<MeshCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _playerController = FindFirstObjectByType<PlayerController>();
        _navMeshObstacle = GetComponent<NavMeshObstacle>();

        // Configure components
        _meshCollider.convex = true;
        _navMeshObstacle.carving = true;

        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void FixedUpdate()
    {
        // Move object towards grab point if grabbed
        if (_grabPoint != null)
        {
            Vector3 targetPosition = _grabPoint.position;
            float adjustedLerpSpeed = _lerpSpeed / _rigidbody.mass;
            _rigidbody.MovePosition(Vector3.SmoothDamp(_rigidbody.position, targetPosition, ref _velocity, 1f / adjustedLerpSpeed));
        }
    }

    public override void Collect()
    {
        // Toggle grabbing state
        _playerController.IsGrabbing = _grabPoint == null;

        if (_playerController.IsGrabbing)
            Grab(_playerController.GrabPoint);
        else
            Drop();
    }

    public void Grab(Transform grabPoint)
    {
        // Set grab point and disable gravity
        _grabPoint = grabPoint;
        _rigidbody.useGravity = false;

        _playerController.CurrentGrabbable = this;

        // Reset velocities
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void Drop()
    {
        // Clear grab point and enable gravity
        _grabPoint = null;
        _rigidbody.useGravity = true;

        _playerController.CurrentGrabbable = null;

        // Apply release force
        Vector3 releaseForce = _playerController.CameraTransform.forward * _releaseForceMultiplier + _velocity;
        _rigidbody.AddForce(releaseForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Play collision sound
        SoundManager.Instance.PlaySound(_playerController.CollisionSound, transform, 0.1f);
    }
}
