using UnityEngine;

/// <summary>
/// Abstract base class for all attack types.
/// Inherit from this to create new attack types (melee, ranged, AOE, etc.)
/// </summary>
public abstract class AttackBase : MonoBehaviour
{
    [Header("Attack Base Settings")]
    [Tooltip("Damage dealt per hit.")]
    public float damage = 10f;

    [Tooltip("Seconds between attacks.")]
    public float cooldown = 1f;

    // Internal cooldown tracking
    protected float lastAttackTime = -999f;

    /// <summary>Whether the attack is off cooldown and ready to use.</summary>
    public bool IsReady => Time.time >= lastAttackTime + cooldown;

    /// <summary>
    /// Executes the attack. Called by CombatController.
    /// Returns true if the attack was successfully triggered.
    /// </summary>
    public bool TryAttack(ICombatant attacker)
    {
        if (!IsReady) return false;

        lastAttackTime = Time.time;
        PerformAttack(attacker);
        return true;
    }

    /// <summary>
    /// Implement the actual attack logic in subclasses.
    /// </summary>
    protected abstract void PerformAttack(ICombatant attacker);
}