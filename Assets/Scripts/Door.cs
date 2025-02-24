using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Audio;

[RequireComponent(typeof(NavMeshObstacle), typeof(MeshCollider))]
public class Door : Interactive
{
    [SerializeField] private float _openDuration = 1.0f;
    [SerializeField] private float _moveDistance = 0f;
    [SerializeField] private float _openAngle = 90f;
    [SerializeField] private bool _openToRight = true;
    [SerializeField] private bool _isLocked = false;
    [SerializeField] private float _lockedAnimationDuration = 0.5f;
    [SerializeField] private float _lockedAnimationAngle = 10f;
    [SerializeField] private AudioClip _openSound;
    [SerializeField] private AudioClip _lockedSound;

    private Quaternion _initialRotation;
    private Vector3 _initialPosition;
    private NavMeshObstacle _navMeshObstacle;
    private Coroutine _currentCoroutine;
    private bool _isOpen = false;

    private void Start()
    {
        _initialRotation = transform.localRotation;
        _initialPosition = transform.localPosition;
        _navMeshObstacle = GetComponent<NavMeshObstacle>();
        _navMeshObstacle.carving = true;
    }

    public override void Interact()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }

        if (_isLocked)
        {
            _currentCoroutine = StartCoroutine(AnimateLockedDoor());
            return;
        }

        _isOpen = !_isOpen;
        _currentCoroutine = StartCoroutine(AnimateDoor(_isOpen));

        SoundManager.Instance.PlaySound(_isOpen ? _openSound : _lockedSound, transform, 0.5f);

        IsActivated = _isOpen;
    }

    private IEnumerator AnimateDoor(bool open)
    {
        _navMeshObstacle.enabled = false;
        float elapsedTime = 0f;

        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = open ? _initialRotation * Quaternion.Euler(0, _openToRight ? _openAngle : -_openAngle, 0) : _initialRotation;

        Vector3 startPosition = transform.localPosition;
        Vector3 direction = Quaternion.Euler(0, _openToRight ? _openAngle : -_openAngle, 0) * Vector3.forward;
        Vector3 targetPosition = open ? _initialPosition + direction * _moveDistance : _initialPosition;

        while (elapsedTime < _openDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / _openDuration);
            
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = targetRotation;
        transform.localPosition = targetPosition;
        _navMeshObstacle.enabled = true;
        _currentCoroutine = null;
    }

    private IEnumerator AnimateLockedDoor()
    {
        float elapsedTime = 0f;
        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotationPositive = _initialRotation * Quaternion.Euler(0, _lockedAnimationAngle, 0);
        Quaternion targetRotationNegative = _initialRotation * Quaternion.Euler(0, -_lockedAnimationAngle, 0);

        while (elapsedTime < _lockedAnimationDuration)
        {
            float t = Mathf.PingPong(elapsedTime / _lockedAnimationDuration, 1f);

            transform.localRotation = t < 0.5f
                ? Quaternion.Slerp(startRotation, targetRotationPositive, t * 2f)
                : Quaternion.Slerp(targetRotationPositive, targetRotationNegative, (t - 0.5f) * 2f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < _lockedAnimationDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / _lockedAnimationDuration);

            transform.localRotation = Quaternion.Slerp(transform.localRotation, _initialRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = _initialRotation;
        _currentCoroutine = null;
    }
}
