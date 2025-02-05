using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Transform _grabPoint;

    public bool CanBeBorrowed = false;
    public bool IsMandatory = false;
    public int Score = 0;

    private PlayerController _playerController;

    [SerializeField] private float _lerpSpeed = 5f;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerController = FindFirstObjectByType<PlayerController>();
    }

    private void Update()
    {
        if (_grabPoint != null)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            transform.position = Vector3.Lerp(transform.position, _grabPoint.position, Time.deltaTime * _lerpSpeed);
        }
    }

    public void Grab(Transform grabPoint)
    {
        _grabPoint = grabPoint;
        _rigidbody.useGravity = false;
    }

    public void Borrow()
    {
        _playerController.Score += Score;
        Destroy(gameObject);
    }

    public void Release()
    {
        _grabPoint = null;
        _rigidbody.AddForce(Camera.main.transform.forward * 2f, ForceMode.Impulse);
        _rigidbody.useGravity = true;
    }
}
