using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshCollider))]
public class Door : Interactive
{
    public float OpenDuration = 1.0f;
    public float MoveDistance = 0f;
    public float OpenAngle = 90f;
    public bool OpenToRight = true;

    private bool _isOpen = false;
    private bool _isAnimating = false;
    private Coroutine _currentCoroutine;
    private Quaternion _initialLocalRotation;
    private Vector3 _initialLocalPosition;

    private void Start()
    {
        _initialLocalRotation = transform.localRotation;
        _initialLocalPosition = transform.localPosition;
    }

    public override void Interact()
    {
        if (_isAnimating)
        {
            StopCoroutine(_currentCoroutine);
            _isAnimating = false;
        }

        Quaternion targetRotation = _isOpen 
            ? _initialLocalRotation 
            : _initialLocalRotation * Quaternion.Euler(0, OpenToRight ? OpenAngle : -OpenAngle, 0);
        
        Vector3 targetPosition = _isOpen 
            ? _initialLocalPosition 
            : _initialLocalPosition + (OpenToRight ? transform.right : -transform.right) * MoveDistance;
        
        _currentCoroutine = StartCoroutine(MoveAndRotateDoor(targetRotation, targetPosition));
        _isOpen = !_isOpen;
    }

    private IEnumerator MoveAndRotateDoor(Quaternion targetRotation, Vector3 targetPosition)
    {
        _isAnimating = true;
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
        _isAnimating = false;
    }
}