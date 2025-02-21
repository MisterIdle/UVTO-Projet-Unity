using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class Door : Interactive
{
    public float OpenDuration = 1.0f;
    public float MoveDistance = 0f;
    public float OpenAngle = 90f;
    public bool OpenToRight = true;

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

        _isOpen = !_isOpen;
        _currentCoroutine = StartCoroutine(AnimateDoor(_isOpen));
    }

    private IEnumerator AnimateDoor(bool open)
    {
        _navMeshObstacle.enabled = false;
        float elapsedTime = 0f;

        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = open ? _initialRotation * Quaternion.Euler(0, OpenToRight ? OpenAngle : -OpenAngle, 0) : _initialRotation;

        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition = open ? _initialPosition + (OpenToRight ? transform.right : -transform.right) * MoveDistance : _initialPosition;

        while (elapsedTime < OpenDuration)
        {
            float t = elapsedTime / OpenDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            
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
}
