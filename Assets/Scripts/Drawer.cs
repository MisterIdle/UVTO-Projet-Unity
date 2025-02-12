using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(MeshCollider), typeof(NavMeshObstacle))]
public class Drawer : Interactive
{
    public float OpenDuration = 1.0f;
    public float MoveDistance = 0f;
    private Vector3 _initialPosition;
    private MeshCollider _meshCollider;
    private NavMeshObstacle _navMeshObstacle;

    private GameObject _objectsParent;

    private Coroutine _currentCoroutine;

    private void Start()
    {
        _initialPosition = transform.localPosition;
        
        _meshCollider = GetComponent<MeshCollider>();
        _navMeshObstacle = GetComponent<NavMeshObstacle>();

        _objectsParent = GameObject.Find("Objects");
    }

    public override void Interact()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }

        IsActivated = !IsActivated;
        _currentCoroutine = StartCoroutine(ToggleDrawer(IsActivated));
    }

    private IEnumerator ToggleDrawer(bool open)
    {
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
        _currentCoroutine = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Entity"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Entity"))
        {
            collision.transform.SetParent(_objectsParent.transform);
        }
    }
}
