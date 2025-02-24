using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private bool _lostPlayer = false;
    [SerializeField] private bool _isSurprised = false;
    [SerializeField] private bool _isInteracting = false;
    public bool isWalking;

    [Header("Detection Settings")]
    [SerializeField] private bool _isPlayerDetected = false;
    [SerializeField] private float _frontDetectionRadius = 7f;
    [SerializeField] private float _frontDetectionAngle = 30f;
    [SerializeField] private float _detectionRadius = 4f;
    [SerializeField] private float _chaseDetectionRadius = 10f;
    [SerializeField] private float _lostPlayerCooldown = 3f;
    [SerializeField] private float _surprisedDuration = 1.5f;

    [Header("Movement Settings")]
    [SerializeField] private Transform _globalPatrolPoints;
    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private float _chaseSpeed = 2f;
    [SerializeField] private float _lookAroundDuration = 3f;
    [SerializeField] private float _lookAroundAngle = 90f;
    [SerializeField] private float _minLookAroundTurn = 1f;
    [SerializeField] private float _maxLookAroundTurn = 6f;

    [Header("Interaction Settings")]
    [SerializeField] private float _interactRadius = 2f;
    [SerializeField] private float _interactDuration = 1f;

    [Header("Attack Settings")]
    [SerializeField] private float _attackRadius = 1.5f;
    [SerializeField] private float _attackCooldown = 1.5f;
    [SerializeField] private float _endAttackCooldown = 1;
    [SerializeField] private int _attackDamage = 1;

    [Header("Activated Settings")]
    [SerializeField] private int _timeWarning = 5;
    [SerializeField] private int _timeFree = 10;
    private bool _canAttack = true;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip _attackSound;
    [SerializeField] private AudioClip _surprisedSound;
    [SerializeField] private AudioClip _footstepSound;
    private NavMeshAgent _agent;
    private PlayerController _player;
    private WasherMachine _washerMachine;
    private Vector3 _lastKnownPlayerPosition;
    private Animator _animator;
    [SerializeField] private Transform[] _patrolPoints;

    private enum State { Patrol, Chase, LookAround, Surprised }

    private State _currentState;

    private void Awake()
    {
        // Initialize components and variables
        _agent = GetComponent<NavMeshAgent>();
        _player = FindFirstObjectByType<PlayerController>();
        _animator = GetComponentInChildren<Animator>();
        _washerMachine = FindFirstObjectByType<WasherMachine>();
        _globalPatrolPoints = GameObject.Find("GlobalPatrolPoints").transform;

        _agent.speed = _patrolSpeed;
        _currentState = State.Patrol;

        _agent.enabled = false;

        GetAllPoints();

        StartCoroutine(Waiting());
    }

    private void Update()
    {
        if (_isInteracting) return;

        HandleInteractions();
        CheckForPlayerDetection();

        _agent.stoppingDistance = 0;

        switch (_currentState)
        {
            case State.Patrol:
                HandlePatrol();
                break;

            case State.Chase:
                HandleChase();
                break;
        }

        isWalking = _agent.velocity.sqrMagnitude > 0.1;
        PlayFootstepSound();

        _agent.updatePosition = true;
        _agent.updateRotation = true;
    }

    private void PlayFootstepSound()
    {
        if (isWalking)
        {
            SoundManager.Instance.PlayOneTimeSound(_footstepSound, transform, 1f);
        }
    }

    private void HandlePatrol()
    {
        if (_agent.enabled && !_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            StartCoroutine(LookAround());
        }
    }

    private void GetAllPoints()
    {
        // Get all patrol points from the global patrol points object
        _patrolPoints = new Transform[_globalPatrolPoints.childCount];
        for (int i = 0; i < _globalPatrolPoints.childCount; i++)
        {
            _patrolPoints[i] = _globalPatrolPoints.GetChild(i);
            Debug.Log(_patrolPoints[i]);
        }
    }

    private void PatrolNextPoint()
    {
        if (!_agent.enabled) return;

        // Set a random patrol point as the next destination
        int randomIndex = Random.Range(0, _patrolPoints.Length);
        _agent.SetDestination(_patrolPoints[randomIndex].position);
    }

    private IEnumerator LookAround()
    {
        if (_isPlayerDetected) yield break;

        _agent.isStopped = true;
        _currentState = State.LookAround;
        _animator.SetBool("IsLook", true);

        for (int i = 0; i < Random.Range(_minLookAroundTurn, _maxLookAroundTurn); i++)
        {
            float direction = Random.value > 0.5f ? 1f : -1f;
            Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + direction * _lookAroundAngle, 0);
            float elapsedTime = 0f;

            while (elapsedTime < _lookAroundDuration)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsedTime / _lookAroundDuration);
                elapsedTime += Time.deltaTime;
                yield return null;

                CheckForPlayerDetection();
                if (_isPlayerDetected)
                {
                    _animator.SetBool("IsLook", false);
                    _currentState = State.Chase;
                    yield break;
                }
            }
        }

        _animator.SetBool("IsLook", false);
        _agent.isStopped = false;
        _currentState = State.Patrol;
        PatrolNextPoint();
    }

    private void HandleInteractions()
    {
        if (_isInteracting) return;

        // Check for nearby interactable objects
        Collider[] hits = Physics.OverlapSphere(transform.position, _detectionRadius);
        Interactive closestInteractive = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out Interactive interactive) && !interactive.IsActivated && !interactive.IgnoreBot)
            {
                float distance = Vector3.Distance(transform.position, interactive.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractive = interactive;
                }
            }
        }

        if (closestInteractive != null && closestDistance <= _interactRadius)
        {
            if (closestInteractive is Switch && _isPlayerDetected) return;
            StartCoroutine(InteractWithObject(closestInteractive));
        }
    }

    private IEnumerator InteractWithObject(Interactive interactive)
    {
        if (_isInteracting || !_canAttack) yield break;

        _isInteracting = true;
        _agent.isStopped = true;
        _animator.SetBool("IsInteract", true);

        yield return new WaitForSeconds(_interactDuration);

        interactive.Interact();

        _animator.SetBool("IsInteract", false);

        _isInteracting = false;
        _agent.isStopped = false;
    }

    private void CheckForPlayerDetection()
    {
        // Check if the player is within detection range
        bool isPlayerInCone = IsPlayerInDetectionCone(transform.forward, _frontDetectionRadius, _frontDetectionAngle) ||
                              IsPlayerInDetectionCone(-transform.forward, _detectionRadius, 180f);

        if (_currentState == State.Chase)
        {
            isPlayerInCone = IsPlayerInDetectionCone(transform.forward, _chaseDetectionRadius, 180f);
        }

        if (isPlayerInCone)
        {
            _isPlayerDetected = true;
            _lastKnownPlayerPosition = _player.transform.position;
        }
        else
        {
            _isPlayerDetected = false;
        }
    }

    private bool IsPlayerInDetectionCone(Vector3 direction, float radius, float angle)
    {
        // Check if the player is within the detection cone
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out PlayerController player))
            {
                Vector3 playerDirection = (player.transform.position - transform.position).normalized;

                if (Vector3.Angle(direction, playerDirection) < angle)
                {
                    if (IsPlayerVisible(player))
                    {
                        _currentState = State.Chase;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool IsPlayerVisible(PlayerController player)
    {
        // Check if the player is visible (not obstructed by obstacles)
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, _chaseDetectionRadius))
        {
            if (hit.collider.GetComponent<PlayerController>() != null)
            {
                return true;
            }
        }
        return false;
    }

    private void HandleChase()
    {
        if (!_isSurprised)
        {
            _isSurprised = true;
            _currentState = State.Surprised;
            _animator.SetTrigger("Surprised");
            SoundManager.Instance.PlaySound(_surprisedSound, transform, 1f);
            StartCoroutine(Surprised());
        }

        if (_isPlayerDetected)
        {
            _agent.stoppingDistance = _interactRadius;
            _agent.speed = _chaseSpeed;

            float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);
            if (distanceToPlayer <= _interactRadius && _canAttack && !_player.IsDead)
            {
                StartCoroutine(AttackPlayer());
            }
            else
            {
                _animator.SetBool("IsRun", true);
                _agent.SetDestination(_player.transform.position);
            }
        }
        else
        {
            if (!_lostPlayer)
            {
                _lostPlayer = true;
                StartCoroutine(LostPlayer());
            }
            _agent.SetDestination(_lastKnownPlayerPosition);
        }
    }

    private IEnumerator AttackPlayer()
    {
        _canAttack = false;
        _agent.isStopped = true;
        _animator.SetTrigger("Attack");

        LookAtPlayer();

        yield return new WaitForSeconds(_attackCooldown);

        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);
        if (distanceToPlayer <= _attackRadius && !_player.IsDead)
        {
            _player.TakeDamage(_attackDamage);
        }

        SoundManager.Instance.PlaySound(_attackSound, transform, 1f);

        yield return new WaitForSeconds(_endAttackCooldown);

        _animator.SetTrigger("Attack");

        if (_player.IsDead)
            Win();

        _agent.isStopped = false;
        _canAttack = true;
    }

    public void LookAtPlayer()
    {
        // Rotate to face the player
        Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        transform.rotation = lookRotation;
    }

    private void Win()
    {
        // Trigger win animation
        _agent.isStopped = true;
        _animator.SetTrigger("Win");
    }

    private IEnumerator Surprised()
    {
        // Handle surprised state
        _agent.isStopped = true;
        _animator.SetBool("IsRun", false);
        _animator.SetBool("IsLook", false);

        Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        float elapsedTime = 0f;

        while (elapsedTime < _surprisedDuration)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, elapsedTime / _surprisedDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _agent.isStopped = false;
        _animator.SetTrigger("Surprised");
        _currentState = State.Chase;
    }

    private IEnumerator LostPlayer()
    {
        // Handle lost player state
        _lostPlayer = true;
        yield return new WaitForSeconds(_lostPlayerCooldown);
        _agent.SetDestination(_lastKnownPlayerPosition);
        _currentState = State.Patrol;
    }

    private IEnumerator Waiting()
    {
        // Wait for a certain time before activating the enemy
        yield return new WaitForSeconds(_timeWarning);
        _washerMachine.IsWashing = false;
        yield return new WaitForSeconds(_timeFree);
        Debug.Log("Free time is over");
        ActivateEnemy();
    }

    private void ActivateEnemy()
    {
        // Activate the enemy and start patrolling
        _agent.enabled = true;
        _currentState = State.Patrol;
        PatrolNextPoint();
    }

    private void OnDrawGizmos()
    {
        // Draw gizmos for debugging detection ranges
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _frontDetectionRadius);

        Vector3 leftRayDirection = Quaternion.Euler(0, -_frontDetectionAngle, 0) * transform.forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, _frontDetectionAngle, 0) * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + leftRayDirection * _frontDetectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightRayDirection * _frontDetectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _chaseDetectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _interactRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }
}
