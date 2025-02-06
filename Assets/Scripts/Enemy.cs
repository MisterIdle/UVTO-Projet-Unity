using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Transform _player;
    public float PlayerDetectionRadius = 10f;
    public float SwitchDetectionRadius = 5f;
    public float DoorDetectionRadius = 5f;
    public float PatrolRadius = 20f;
    public float MinDistancePatrol = 5f;
    public int ActionPerSecond = 2;
    public float ZoneActivationRadius = 3f;
    private bool _isPatrolling;
    private bool _isActionInProgress;
    private Vector3 _lastPatrolPoint;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = FindFirstObjectByType<PlayerController>().transform;
        _isPatrolling = true;
        GoToNextPatrolPoint();
        _isActionInProgress = false;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if (distanceToPlayer <= PlayerDetectionRadius)
            PursuePlayer();

        else
            HandleNearbyObjects();

        if (_isPatrolling)
            Patrol();

        TryInteractWithObjects();
    }

    private void PursuePlayer()
    {
        _agent.SetDestination(_player.position);
        _isPatrolling = false;
    }

    private void HandleNearbyObjects()
    {
        Transform nearestInactiveSwitch = GetNearestInactiveSwitch();
        Transform nearestInactiveDoor = GetNearestInactiveDoor();

        if (nearestInactiveSwitch != null && Vector3.Distance(transform.position, nearestInactiveSwitch.position) <= SwitchDetectionRadius)
        {
            MoveToTarget(nearestInactiveSwitch);
        }
        else if (nearestInactiveDoor != null && Vector3.Distance(transform.position, nearestInactiveDoor.position) <= DoorDetectionRadius)
        {
            MoveToTarget(nearestInactiveDoor);
        }
        else if (!_isPatrolling)
        {
            _isPatrolling = true;
            GoToNextPatrolPoint();
        }
    }

    private void MoveToTarget(Transform target)
    {
        _agent.SetDestination(target.position);
        _isPatrolling = false;
        FaceTarget(target);
    }

    private void GoToNextPatrolPoint()
    {
        Vector3 randomDirection;
        NavMeshHit hit;

        bool foundValidPoint = false;

        while (!foundValidPoint)
        {
            randomDirection = Random.insideUnitSphere * PatrolRadius + transform.position;

            if (NavMesh.SamplePosition(randomDirection, out hit, PatrolRadius, 1))
            {
                if (Vector3.Distance(hit.position, _lastPatrolPoint) >= MinDistancePatrol)
                {
                    foundValidPoint = true;
                    _lastPatrolPoint = hit.position;
                    _agent.destination = hit.position;
                }
            }
        }
    }

    private void Patrol()
    {
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            if (Vector3.Distance(_agent.destination, _lastPatrolPoint) > 1f)
            {
                GoToNextPatrolPoint();
            }
            else
            {
                GoToNextPatrolPoint();
            }
        }
    }

    private void TryInteractWithObjects()
    {
        if (!_isActionInProgress)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, ZoneActivationRadius);
            foreach (var hitCollider in hitColliders)
            {
                Interactive targetInteractive = hitCollider.GetComponent<Interactive>();
                if (targetInteractive != null)
                {
                    Door doorObj = targetInteractive as Door;
                    if (doorObj != null && !doorObj.IsOpen)
                    {
                        StartCoroutine(InteractWithDelay(targetInteractive));
                        break;
                    }

                    Switch switchObj = targetInteractive as Switch;
                    if (switchObj != null && !switchObj.IsOn)
                    {
                        StartCoroutine(InteractWithDelay(targetInteractive));
                        break;
                    }
                }
            }
        }
    }

    private IEnumerator InteractWithDelay(Interactive targetInteractive)
    {
        _isActionInProgress = true;
        targetInteractive.Interact();
        yield return new WaitForSeconds(ActionPerSecond);
        _isActionInProgress = false;
        _agent.SetDestination(_lastPatrolPoint);
    }

    private Transform GetNearestInactiveSwitch()
    {
        Transform nearestSwitch = null;
        float closestDistance = float.MaxValue;

        foreach (Switch switchTransform in FindObjectsByType<Switch>(FindObjectsSortMode.None))
        {
            Switch switchObj = switchTransform.GetComponent<Switch>();
            if (switchObj != null && !switchObj.IsOn)
            {
                float distance = Vector3.Distance(transform.position, switchTransform.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestSwitch = switchTransform.transform;
                }
            }
        }

        return nearestSwitch;
    }

    private Transform GetNearestInactiveDoor()
    {
        Transform nearestDoor = null;
        float closestDistance = float.MaxValue;

        foreach (Door doorTransform in FindObjectsByType<Door>(FindObjectsSortMode.None))
        {
            Door doorObj = doorTransform.GetComponent<Door>();
            if (doorObj != null && !doorObj.IsOpen)
            {
                float distance = Vector3.Distance(transform.position, doorTransform.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestDoor = doorTransform.transform;
                }
            }
        }

        return nearestDoor;
    }

    private void FaceTarget(Transform target)
    {
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }
}
