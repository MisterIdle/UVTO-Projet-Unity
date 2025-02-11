using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(MeshCollider))]
public class Grabbable : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Transform _grabPoint;
    private PlayerController _playerController;

    [SerializeField] private float _lerpSpeed = 10f;
    [SerializeField] private float _releaseForceMultiplier = 2f;

    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerController = FindFirstObjectByType<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (_grabPoint != null)
        {
            // Mouvement fluide vers le point de grab
            Vector3 targetPosition = _grabPoint.position;
            _rigidbody.MovePosition(Vector3.SmoothDamp(_rigidbody.position, targetPosition, ref _velocity, 1f / _lerpSpeed));
        }
    }

    public void Grab(Transform grabPoint)
    {
        _grabPoint = grabPoint;
        _rigidbody.useGravity = false;

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void Release()
    {
        _grabPoint = null;
        _rigidbody.useGravity = true;

        Vector3 releaseForce = _playerController.cameraTransform.forward * _releaseForceMultiplier + _velocity;
        _rigidbody.AddForce(releaseForce, ForceMode.Impulse);
    }
}
