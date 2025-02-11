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

    private bool _isAnimating = false;
    private Quaternion _initialRotation;
    private Vector3 _initialPosition;
    private MeshCollider _meshCollider;
    private NavMeshObstacle _navMeshObstacle;

    private void Start()
    {
        _initialRotation = transform.localRotation;
        _initialPosition = transform.localPosition;
        
        _meshCollider = GetComponent<MeshCollider>();
        _navMeshObstacle = GetComponent<NavMeshObstacle>();
    }

    public override void Interact()
    {
        if (_isAnimating || IsActivated == !IsActivated)
            return;

        IsActivated = !IsActivated;
        StartCoroutine(ToggleDoor(IsActivated));
    }

    private IEnumerator ToggleDoor(bool open)
    {
        _isAnimating = true;
        _meshCollider.enabled = false;
        _navMeshObstacle.enabled = false;

        Quaternion targetRotation = open ? _initialRotation * Quaternion.Euler(0, OpenToRight ? OpenAngle : -OpenAngle, 0) : _initialRotation;
        Vector3 targetPosition = open ? _initialPosition + (OpenToRight ? transform.right : -transform.right) * MoveDistance : _initialPosition;
        
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
        _meshCollider.enabled = true;
        _isAnimating = false;
    }
}