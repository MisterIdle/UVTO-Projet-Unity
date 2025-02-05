using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Transform _grabPoint;
    private Vector3 _lastVelocity;

    public bool CanBeBorrowed = false;
    public bool IsMandatory = false;
    public int Score = 0;

    private PlayerController _playerController;

    [SerializeField] private float _lerpSpeed = 10f;

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
            _lastVelocity = _rigidbody.linearVelocity;
            float adjustedLerpSpeed = _lerpSpeed / _rigidbody.mass;
            _rigidbody.MovePosition(Vector3.Lerp(_rigidbody.position, _grabPoint.position, Time.fixedDeltaTime * adjustedLerpSpeed));
        }
    }

    public void Grab(Transform grabPoint)
    {
        _grabPoint = grabPoint;
        _rigidbody.useGravity = false;
        //_rigidbody.isKinematic = true;
    }

    public void Borrow()
    {
        _playerController.Score += Score;
        Destroy(gameObject);
    }

    public void Release()
    {
        _grabPoint = null;
        _rigidbody.useGravity = true;
        //_rigidbody.isKinematic = false;
        
        _rigidbody.linearVelocity = _lastVelocity;
        _rigidbody.AddTorque(Random.insideUnitSphere);
    }
}
