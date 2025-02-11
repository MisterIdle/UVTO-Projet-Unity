using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Transform _grabPoint;

    private PlayerController _playerController;

    [SerializeField] private float _lerpSpeed = 2f;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerController = FindFirstObjectByType<PlayerController>();

        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void FixedUpdate()
    {
        if (_grabPoint != null)
        {
            _rigidbody.MovePosition(Vector3.Lerp(_rigidbody.position, _grabPoint.position, Time.deltaTime * _lerpSpeed));
        }
    }

    public void Grab(Transform grabPoint)
    {
        _grabPoint = grabPoint;
        _rigidbody.useGravity = false;
    }

    public void Release()
    {
        _grabPoint = null;
        _rigidbody.useGravity = true;

        _rigidbody.AddForce(_playerController.cameraTransform.forward * 2f, ForceMode.Impulse);
    }
}
