using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(MeshCollider), typeof(NavMeshObstacle))]
public class Drawer : Interactive
{
    public float OpenDuration = 1.0f;
    public float MoveDistance = 0f;
    private bool _isAnimating = false;

    private Vector3 _initialPosition;

    private MeshCollider _meshCollider;
    private NavMeshObstacle _navMeshObstacle;

    private HashSet<Transform> _borrowables = new HashSet<Transform>();

    private void Start()
    {
        _initialPosition = transform.localPosition;
        
        _meshCollider = GetComponent<MeshCollider>();
        _navMeshObstacle = GetComponent<NavMeshObstacle>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Borrowable borrowable = collision.collider.GetComponent<Borrowable>();
        if (borrowable != null)
        {
            _borrowables.Add(borrowable.transform);
            borrowable.transform.SetParent(transform, true); 
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Borrowable borrowable = collision.collider.GetComponent<Borrowable>();
        if (borrowable != null && _borrowables.Contains(borrowable.transform))
        {
            _borrowables.Remove(borrowable.transform);
            borrowable.transform.SetParent(null, true);
        }
    }

    public override void Interact()
    {
        if (_isAnimating || IsActivated == !IsActivated)
            return;

        IsActivated = !IsActivated;
        StartCoroutine(ToggleDrawer(IsActivated));
    }

    private IEnumerator ToggleDrawer(bool open)
    {
        _isAnimating = true;

        Vector3 targetPosition = open ? _initialPosition + transform.forward * MoveDistance : _initialPosition;
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;
        
        while (elapsedTime < OpenDuration)
        {
            float t = elapsedTime / OpenDuration;
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = targetPosition;
        _isAnimating = false;
    }
}
