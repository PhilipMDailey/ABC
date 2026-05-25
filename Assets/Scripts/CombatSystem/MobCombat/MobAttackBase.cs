using UnityEngine;

/// <summary>
/// Base class for all mob attacks.
/// Inherit from this to create new mob attack types.
/// Provides common mob-specific attack properties and utilities.
/// </summary>
public abstract class MobAttackBase : AttackBase
{
    [Header("Mob Attack Settings")]
    [Tooltip("How close the mob needs to be to the target to trigger the attack.")]
    public float attackRange = 2f;

    /// <summary>
    /// Returns true if the target is within attack range.
    /// Used by mob AI to decide when to trigger an attack.
    /// </summary>
    public bool IsTargetInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) <= attackRange;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}