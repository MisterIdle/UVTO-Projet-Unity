using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent _agent;
    private PlayerController _player;
    private Vector3 _lastKnownPlayerPosition;
    private Animator _animator;
    private bool _isChasing = false;
    private bool _lostPlayer = false;
    private bool _isLookingAround = false;
    private bool _isInteracting = false;

    [Header("Detection Settings")]
    [SerializeField] private float _frontDetectionRadius = 7f;
    [SerializeField] private float _frontDetectionAngle = 30f;
    [SerializeField] private float _detectionRadius = 4f;
    [SerializeField] private float _chaseDetectionRadius = 10f;

    [Header("Movement Settings")]
    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private float _chaseSpeed = 2f;
    [SerializeField] private float _lookAroundDuration = 3f;
    [SerializeField] private float _lookAroundAngle = 90f;

    [Header("Interaction Settings")]
    [SerializeField] private float _interactRadius = 2f;
    [SerializeField] private float _lostPlayerCooldown = 3f;
    [SerializeField] private float _interactDuration = 2f;

    [SerializeField] private Transform[] _patrolPoints;
    private int currentPatrolIndex = 0;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = FindFirstObjectByType<PlayerController>();
        _animator = GetComponentInChildren<Animator>();
        _agent.speed = _patrolSpeed;

        PatrolNextPoint();
    }

    private void Update()
    {
        HandleInteractions();

        if (_isChasing)
            HandleChasing();

        else if (!_isLookingAround)
            HandlePatrolOrLookAround();


        CheckForPlayerDetection();
    }

    private void HandleChasing()
    {
        _agent.SetDestination(_lastKnownPlayerPosition);

        if (Vector3.Distance(transform.position, _player.transform.position) <= _chaseDetectionRadius)
        {
            _lastKnownPlayerPosition = _player.transform.position;
            _lostPlayer = false;
            _agent.speed = _chaseSpeed;
            _animator.SetBool("Turn", false);
            _animator.speed = 1.2f;
        }
        else if (!_lostPlayer)
        {
            _lostPlayer = true;
            _agent.speed = _patrolSpeed;
            _animator.speed = 1f;
            StartCoroutine(ResetDetection());
        }
    }

    private void HandlePatrolOrLookAround()
    {
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            StartCoroutine(LookAround());
        }
    }

    private void CheckForPlayerDetection()
    {
        if (_isChasing) return;

        bool playerDetected = IsPlayerInDetectionCone(transform.forward, _frontDetectionRadius, _frontDetectionAngle) || IsPlayerInDetectionCone(-transform.forward, _detectionRadius, 180f);

        if (playerDetected)
        {
            _lastKnownPlayerPosition = _player.transform.position;
            _isChasing = true;
            _lostPlayer = false;

            if (_isLookingAround)
            {
                StopAllCoroutines();
                _isLookingAround = false;
                _agent.isStopped = false;
            }
        }
    }

    private bool IsPlayerInDetectionCone(Vector3 direction, float radius, float angle)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out PlayerController player))
            {
                Vector3 playerDirection = player.transform.position - transform.position;
                float playerAngle = Vector3.Angle(direction, playerDirection);

                if (playerAngle < angle)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void PatrolNextPoint()
    {
        if (_patrolPoints.Length == 0)
        {
            Debug.LogError("No patrol points assigned to the enemy!");
            return;
        }

        int nextPatrolIndex = Random.Range(0, _patrolPoints.Length);

        while (nextPatrolIndex == currentPatrolIndex)
        {
            nextPatrolIndex = Random.Range(0, _patrolPoints.Length);
        }

        currentPatrolIndex = nextPatrolIndex;
        _agent.SetDestination(_patrolPoints[currentPatrolIndex].position);
    }

    private IEnumerator LookAround()
    {
        if (_isLookingAround) yield break;

        _isLookingAround = true;
        _agent.isStopped = true;

        _animator.SetBool("Turn", true);

        for (int i = 0; i < 4; i++)
        {
            float direction = Random.value > 0.5f ? 1f : -1f;
            Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + direction * _lookAroundAngle, 0);
            float elapsedTime = 0f;

            while (elapsedTime < _lookAroundDuration)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsedTime / _lookAroundDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        _animator.SetBool("Turn", false);

        _agent.isStopped = false;
        _isLookingAround = false;

        PatrolNextPoint();
    }

    private IEnumerator ResetDetection()
    {
        yield return new WaitForSeconds(_lostPlayerCooldown);
        _isChasing = false;

        StartCoroutine(LookAround());
    }

    private void HandleInteractions()
    {
        if (_isInteracting) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, _detectionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out Interactive interactive))
            {
                if (interactive.IsActivated)
                    continue;

                _agent.SetDestination(interactive.transform.position);

                if (Vector3.Distance(transform.position, interactive.transform.position) <= _interactRadius)
                {
                    StartCoroutine(InteractWithObject(interactive));
                }
            }
        }
    }

    private IEnumerator InteractWithObject(Interactive interactive)
    {
        if (_isChasing && interactive is Switch)
            yield break;

        if (interactive.IgnoreBot)
            yield break;

        _isInteracting = true;
        _agent.isStopped = true;
        _animator.SetBool("Interact", true);

        float elapsedTime = 0f;

        while (elapsedTime < _interactDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        HandleInteractiveObject(interactive);

        _agent.isStopped = false;
        _isInteracting = false;
        _animator.SetBool("Interact", false);

        if (_isChasing)
            _agent.SetDestination(_lastKnownPlayerPosition);

        else
            PatrolNextPoint();
    }


    private void HandleInteractiveObject(Interactive interactive)
    {
        switch (interactive)
        {
            case Switch s when !s.IgnoreBot:
                s.Interact();
                break;
            case Door d when !d.IgnoreBot:
                d.Interact();
                break;
        }

        if (_isChasing)
            _agent.SetDestination(_lastKnownPlayerPosition);

        else if (!_isChasing && _patrolPoints.Length > 0)
            PatrolNextPoint();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _frontDetectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _interactRadius);

        DrawDetectionCone(transform.forward, _frontDetectionRadius, _frontDetectionAngle, Color.red);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _chaseDetectionRadius);
    }

    private void DrawDetectionCone(Vector3 direction, float radius, float angle, Color color)
    {
        Gizmos.color = color;

        Vector3 leftLimit = Quaternion.Euler(0, -angle, 0) * direction * radius;
        Vector3 rightLimit = Quaternion.Euler(0, angle, 0) * direction * radius;

        Gizmos.DrawLine(transform.position, transform.position + leftLimit);
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);
        Gizmos.DrawLine(transform.position + leftLimit, transform.position + rightLimit);
    }
}
