using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(MeshCollider), typeof(NavMeshObstacle))]
public class Door : Interactive
{
    public float OpenDuration = 1.0f;
    public float MoveDistance = 0f;
    public float OpenAngle = 90f;
    public bool OpenToRight = true;

    public bool IsOpen = false;
    private bool _isAnimating = false;
    private Coroutine _currentCoroutine;
    private Quaternion _initialLocalRotation;
    private Vector3 _initialLocalPosition;
    private MeshCollider _meshCollider;
    private NavMeshObstacle _navMeshObstacle;

    private void Start()
    {
        _initialLocalRotation = transform.localRotation;
        _initialLocalPosition = transform.localPosition;
        _meshCollider = GetComponent<MeshCollider>();
        _navMeshObstacle = GetComponent<NavMeshObstacle>();
    }

    public override void Interact()
    {
        if (_isAnimating)
        {
            StopCoroutine(_currentCoroutine);
            _isAnimating = false;
        }

        Quaternion targetRotation = IsOpen ? _initialLocalRotation : _initialLocalRotation * Quaternion.Euler(0, OpenToRight ? OpenAngle : -OpenAngle, 0);
        Vector3 targetPosition = IsOpen ? _initialLocalPosition : _initialLocalPosition + (OpenToRight ? transform.right : -transform.right) * MoveDistance;
        
        _currentCoroutine = StartCoroutine(MoveAndRotateDoor(targetRotation, targetPosition));
        IsOpen = !IsOpen;
    }

    private IEnumerator MoveAndRotateDoor(Quaternion targetRotation, Vector3 targetPosition)
    {
        _isAnimating = true;

        _meshCollider.enabled = false;
        _navMeshObstacle.enabled = false;

        Quaternion startRotation = transform.localRotation;
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < OpenDuration)
        {
            float t = elapsedTime / OpenDuration;
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = targetRotation;
        transform.localPosition = targetPosition;

        if (IsOpen)
        {
            _navMeshObstacle.enabled = false;
        }
        
        _meshCollider.enabled = true;

        _isAnimating = false;
    }
}