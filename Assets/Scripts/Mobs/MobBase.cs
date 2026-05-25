using UnityEngine;
using UnityEngine.AI;
using System;

/// <summary>
/// Base class for all mob types.
/// Handles the state machine, perception, movement via NavMeshAgent, and lifecycle.
/// Specific mob types inherit from this and define their own states and stats.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MobBase : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Display name of this mob type.")]
    public string mobName = "Mob";

    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }

    [Header("Movement")]
    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 3f;

    [Tooltip("How fast the mob rotates to face its movement direction.")]
    public float rotationSpeed = 5f;

    [Header("Separation")]
    [Tooltip("Radius within which this mob will steer away from other mobs.")]
    public float separationRadius = 2f;

    [Tooltip("How strongly separation steers away from other mobs. Higher = more spread out.")]
    public float separationStrength = 1.5f;

    [Header("Perception")]
    [Tooltip("How far the mob can see the player.")]
    public float detectionRange = 15f;

    [Tooltip("How far the mob can hear the player (no line of sight needed).")]
    public float hearingRange = 5f;

    /// <summary>
    /// Fired when this mob is destroyed. Used by MobSpawner to track active counts.
    /// </summary>
    public event Action OnMobDestroyed;

    // State machine
    private MobState currentState;

    // Components
    protected NavMeshAgent agent;

    // References
    protected Transform player;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Apply movement settings to agent
        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed * 100f;
        agent.stoppingDistance = 0.5f;

        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"{mobName}: No GameObject tagged 'Player' found.");
    }

    protected virtual void Start()
    {
        MobManager.Instance?.RegisterMob(this);
        TransitionTo(new IdleState(this));
    }

    protected virtual void Update()
    {
        CombatController combat = GetComponent<CombatController>();
        if (combat != null && !combat.IsAlive) return;

        currentState?.OnUpdate();
        AlignToTerrain();
    }

    /// <summary>
    /// Transitions the mob to a new state.
    /// </summary>
    public void TransitionTo(MobState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState?.OnEnter();
    }

    /// <summary>
    /// Returns the name of the current state for debugging.
    /// </summary>
    public string GetCurrentStateName()
    {
        return currentState?.GetType().Name ?? "None";
    }

    // ─── Movement ────────────────────────────────────────────────────────────

    /// <summary>
    /// Moves the mob toward a world-space target position using NavMeshAgent.
    /// Returns true when within stopping distance.
    /// </summary>
    // public bool MoveToward(Vector3 target, float stoppingDistance = 0.5f)
    // {
    //     agent.stoppingDistance = stoppingDistance;
    //     agent.SetDestination(target);
    //     return !agent.pathPending && agent.remainingDistance <= stoppingDistance;
    // }
    public bool MoveToward(Vector3 target, float stoppingDistance = 0.5f)
    {
        if (!agent.isOnNavMesh) return false;
        
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(target);
        return !agent.pathPending && agent.remainingDistance <= stoppingDistance;
    }

    /// <summary>
    /// Moves the mob in a pre-calculated direction using NavMeshAgent.
    /// Used by AttackState for blended chase + separation steering.
    /// </summary>
    
    void AlignToTerrain()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 5f))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * 
                                        Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // public void MoveInDirection(Vector3 direction)
    // {
    //     direction.y = 0f;
    //     if (direction.magnitude < 0.001f) return;
    //     direction.Normalize();

    //     Vector3 targetPosition = transform.position + direction * moveSpeed;
    //     agent.SetDestination(targetPosition);
    // }
    public void MoveInDirection(Vector3 direction)
    {
        if (!agent.isOnNavMesh) return;
        
        direction.y = 0f;
        if (direction.magnitude < 0.001f) return;
        direction.Normalize();

        Vector3 targetPosition = transform.position + direction * moveSpeed;
        agent.SetDestination(targetPosition);
    }

    /// <summary>
    /// Stops the mob's movement immediately.
    /// </summary>
    public void StopMovement()
    {
        if (agent.isOnNavMesh)
            agent.ResetPath();
    }

    // ─── Perception ──────────────────────────────────────────────────────────

    public bool CanSeePlayer()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }

    public bool CanHearPlayer()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= hearingRange;
    }

    public float DistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }

    // ─── Lifecycle ───────────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnDamaged(amount);

        if (currentHealth <= 0f)
            OnDeath();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    protected virtual void OnDamaged(float amount) { }

    protected virtual void OnDeath()
    {
        MobManager.Instance?.UnregisterMob(this);
        StopMovement();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        OnMobDestroyed?.Invoke();
        MobManager.Instance?.UnregisterMob(this);
    }

    // ─── Debug ───────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}