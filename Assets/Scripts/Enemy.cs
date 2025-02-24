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
    [SerializeField] private float _stuckDetectionTime = 2f;
    [SerializeField] private float _stuckResetDistance = 1f;

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

    private Vector3 _lastPosition;
    private float _stuckTimer = 0f;

    [SerializeField] private Transform[] _patrolPoints;
    private int _currentPatrolIndex = 0;

    private enum State { Patrol, Chase, LookAround, Surprised }

    private State _currentState;

    private void Awake()
    {
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

        HandleStuckDetection();

        switch (_currentState)
        {
            case State.Patrol:
                HandlePatrol();
                break;

            case State.Chase:
                HandleChase();
                break;
        }

        isWalking = _agent.velocity.sqrMagnitude > 0;
        PlayFootstepSound();
    }

    private void PlayFootstepSound()
    {
        if (isWalking)
        {
            SoundManager.Instance.PlaySound(_footstepSound, transform, 1f, true);
        }
    }

    private void HandleStuckDetection()
    {
        if (Vector3.Distance(transform.position, _lastPosition) < _stuckResetDistance)
        {
            _stuckTimer += Time.deltaTime;
        }
        else
        {
            _stuckTimer = 0f;
            _lastPosition = transform.position;
        }

        if (_stuckTimer >= _stuckDetectionTime)
        {
            AdjustEnemyPosition();
            _stuckTimer = 0f;
        }

        if (_agent.velocity.sqrMagnitude < Mathf.Epsilon && !_agent.pathPending)
        {
            _stuckTimer += Time.deltaTime;
            if (_stuckTimer >= _stuckDetectionTime)
            {
                AdjustEnemyPosition();
                _stuckTimer = 0f;
            }
        }
        else
        {
            _stuckTimer = 0f;
        }
    }

    private void AdjustEnemyPosition()
    {
        if (_currentState == State.Patrol)
        {
            PatrolNextPoint();
        }
        else if (_currentState == State.Chase)
        {
            _agent.SetDestination(_lastKnownPlayerPosition);
            if (Vector3.Distance(transform.position, _lastKnownPlayerPosition) < _stuckResetDistance)
            {
                Debug.Log("Enemy is stuck while chasing! Trying a random nearby position.");
                TryRandomPositionNearby();
            }
        }
    }

    private void TryRandomPositionNearby()
    {
        Vector3 randomDirection = Random.insideUnitSphere * _stuckResetDistance;
        randomDirection += transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, _stuckResetDistance, 1))
        {
            _agent.SetDestination(hit.position);
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
        _currentPatrolIndex = Random.Range(0, _patrolPoints.Length);
        _agent.SetDestination(_patrolPoints[_currentPatrolIndex].position);
    }

    private IEnumerator LookAround()
    {
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
                    HandleChase();
                    yield break;
                }
            }
        }

        _animator.SetBool("IsLook", false);

        _agent.isStopped = false;

        if (_isPlayerDetected)
        {
            _currentState = State.Chase;
        }
        else
        {
            _agent.speed = _patrolSpeed;
            _currentState = State.Patrol;
            PatrolNextPoint();
        }
    }

    private void HandleInteractions()
    {
        if (_isInteracting || !_canAttack) return;  

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
        _animator.SetTrigger("Interact");

        yield return new WaitForSeconds(_interactDuration);

        _animator.SetTrigger("Interact");

        interactive.Interact();

        _isInteracting = false;
        _agent.isStopped = false;
    }

    private void CheckForPlayerDetection()
    {
        _isPlayerDetected = IsPlayerInDetectionCone(transform.forward, _frontDetectionRadius, _frontDetectionAngle) || IsPlayerInDetectionCone(-transform.forward, _detectionRadius, 180f);

        if (_currentState == State.Chase)
        {
            _isPlayerDetected = IsPlayerInDetectionCone(transform.forward, _chaseDetectionRadius, 180f);
        }
    }

    private bool IsPlayerInDetectionCone(Vector3 direction, float radius, float angle)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out PlayerController player))
            {
                Vector3 playerDirection = (player.transform.position - transform.position).normalized;

                if (Vector3.Angle(direction, playerDirection) < angle)
                {
                    _currentState = State.Chase;
                    return true;
                }
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

            LookAtPlayer();

            _animator.SetTrigger("Surprised");

            SoundManager.Instance.PlaySound(_surprisedSound, transform, 1f, false);
            StartCoroutine(Surprised());
        }

        if (_isPlayerDetected)
        {
            _agent.stoppingDistance = _interactRadius;

            float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);
            if (distanceToPlayer <= _interactRadius && _canAttack && !_player.IsDead)
            {
                StartCoroutine(AttackPlayer());
            }
            else
            {
                _animator.SetBool("IsRun", true);
                _agent.speed = _chaseSpeed;
                _lastKnownPlayerPosition = _player.transform.position;
                _agent.SetDestination(_lastKnownPlayerPosition);
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

        SoundManager.Instance.PlaySound(_attackSound, transform, 1f, false);

        yield return new WaitForSeconds(_endAttackCooldown);

        _animator.SetTrigger("Attack");

        if (_player.IsDead)
            Win();

        _agent.isStopped = false;
        _canAttack = true;
    }

    public void LookAtPlayer()
    {
        Vector3 directionToPlayer = (_player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        transform.rotation = lookRotation;
    }

    private void Win()
    {
        _agent.isStopped = true;
        _animator.SetTrigger("Win");
    }

    private IEnumerator Surprised()
    {
        _agent.isStopped = true;

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
        yield return new WaitForSeconds(_lostPlayerCooldown);
        _lostPlayer = false;
        _animator.SetBool("IsRun", false);
        StartCoroutine(LookAround());
    }

    private IEnumerator Waiting() {
        yield return new WaitForSeconds(_timeWarning);
        _washerMachine.IsWashing = false;
        yield return new WaitForSeconds(_timeFree);
        Debug.Log("Free time is over");
        ActivateEnemy();
    }

    private void ActivateEnemy()
    {
        _agent.enabled = true;
        _currentState = State.Patrol;
        PatrolNextPoint();
    }

    private void OnDrawGizmos()
    {
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
