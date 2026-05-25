/// <summary>
/// Interface that any entity capable of combat must implement.
/// Ensures a consistent contract for players, mobs, and anything else that fights.
/// </summary>
public interface ICombatant
{
    /// <summary>The display name of this combatant.</summary>
    string CombatantName { get; }

    /// <summary>Current health.</summary>
    float CurrentHealth { get; }

    /// <summary>Maximum health.</summary>
    float MaxHealth { get; }

    /// <summary>Whether this combatant is still alive.</summary>
    bool IsAlive { get; }

    /// <summary>
    /// Apply damage to this combatant.
    /// impactPosition is the world position of the attack sphere at moment of impact,
    /// used to calculate knockback direction.
    /// </summary>
    void TakeDamage(float amount, ICombatant source, UnityEngine.Vector3 impactPosition);

    /// <summary>Trigger this combatant's death.</summary>
    void Die();
}