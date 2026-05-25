using UnityEngine;

/// <summary>
/// Base class for all weapons.
/// Sits on the weapon prefab and defines the weapon's identity,
/// stats, and which attack type it uses.
/// </summary>
public class WeaponBase : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Display name of this weapon.")]
    public string weaponName = "Weapon";

    [Tooltip("Description of this weapon.")]
    public string weaponDescription = "";

    [Header("Stat Modifiers")]
    [Tooltip("Multiplier applied to the attack's base damage. 1 = no change.")]
    public float damageModifier = 1f;

    [Tooltip("Multiplier applied to the attack's cooldown. Values below 1 = faster attacks.")]
    public float attackSpeedModifier = 1f;

    [Tooltip("Added to the attack's base range.")]
    public float rangeModifier = 0f;

    [Header("Socket")]
    [Tooltip("Offset from the socket position where this weapon sits. Adjust to align with the hand.")]
    public Vector3 positionOffset = Vector3.zero;

    [Tooltip("Rotation offset applied when equipped.")]
    public Vector3 rotationOffset = Vector3.zero;

    /// <summary>
    /// Applies this weapon's stat modifiers to its attack component.
    /// Called by WeaponSocket when the weapon is equipped.
    /// </summary>
    public void ApplyModifiers(AttackBase attack)
    {
        if (attack == null) return;

        attack.damage *= damageModifier;
        attack.cooldown *= attackSpeedModifier;

        // Apply range modifier to the attack path if it's an arc
        PlayerBasicAttack basicAttack = attack as PlayerBasicAttack;
        if (basicAttack != null && basicAttack.attackPath != null)
        {
            ArcAttackPath arcPath = basicAttack.attackPath as ArcAttackPath;
            if (arcPath != null)
                arcPath.attackDistance += rangeModifier;
        }
    }


    /// <summary>
    /// Removes this weapon's stat modifiers from its attack component.
    /// Called by WeaponSocket when the weapon is unequipped.
    /// </summary>
    public void RemoveModifiers(AttackBase attack)
    {
        if (attack == null) return;

        attack.damage /= damageModifier;
        attack.cooldown /= attackSpeedModifier;

        PlayerBasicAttack basicAttack = attack as PlayerBasicAttack;
        if (basicAttack != null && basicAttack.attackPath != null)
        {
            ArcAttackPath arcPath = basicAttack.attackPath as ArcAttackPath;
            if (arcPath != null)
                arcPath.attackDistance -= rangeModifier;
        }
    }
}