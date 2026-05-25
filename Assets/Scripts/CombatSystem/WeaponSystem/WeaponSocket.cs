using UnityEngine;

/// <summary>
/// Defines an attachment point on the player where a weapon can be equipped.
/// Handles positioning, rotating, and notifying the CombatController
/// when a weapon is equipped or unequipped.
/// </summary>
public class WeaponSocket : MonoBehaviour
{
    [Header("Socket Settings")]
    [Tooltip("Name of this socket (e.g. RightHand, LeftHand, Back).")]
    public string socketName = "RightHand";

    [Tooltip("The currently equipped weapon. Assign a weapon prefab here to equip at start.")]
    public WeaponBase equippedWeapon;

    // Reference to the CombatController on the same character
    private CombatController combatController;

    void Awake()
    {
        combatController = GetComponentInParent<CombatController>();
    }

    // void Start()
    // {
    //     // Equip the default weapon if one is assigned
    //     if (equippedWeapon != null)
    //         EquipWeapon(equippedWeapon);
    // }

    /// <summary>
    /// Equips a weapon to this socket.
    /// Instantiates the weapon, positions it, and notifies the CombatController.
    /// </summary>
    // public void EquipWeapon(WeaponBase weapon)
    // {
    //     // If equippedWeapon is a prefab asset rather than an instance, clear it
    //     if (equippedWeapon != null && !equippedWeapon.gameObject.scene.IsValid())
    //         equippedWeapon = null;

    //     // Unequip current weapon first
    //     if (equippedWeapon != null)
    //         UnequipWeapon();

    //     // Instantiate and attach weapon to this socket
    //     WeaponBase instance = Instantiate(weapon, transform);
    //     instance.transform.localPosition = weapon.positionOffset;
    //     instance.transform.localRotation = Quaternion.Euler(weapon.rotationOffset);
    //     instance.gameObject.name = weapon.weaponName;

    //     equippedWeapon = instance;

    //     // Assign weapon transform to WeaponAnimator
    //     WeaponAnimator weaponAnimator = GetComponent<WeaponAnimator>();
    //     if (weaponAnimator != null)
    //         weaponAnimator.WeaponTransform = instance.transform;

    //     // Apply stat modifiers
    //     equippedWeapon.ApplyModifiers();

    //     // Notify CombatController of new attack
    //     if (combatController != null && equippedWeapon.attack != null)
    //         combatController.currentAttack = equippedWeapon.attack;

    //     Debug.Log($"WeaponSocket [{socketName}]: Equipped {weapon.weaponName}");
    // }
    public void EquipWeapon(WeaponBase weapon)
    {
        // Clear prefab reference
        if (equippedWeapon != null && !equippedWeapon.gameObject.scene.IsValid())
            equippedWeapon = null;

        if (equippedWeapon != null)
            UnequipWeapon();

        WeaponBase instance = Instantiate(weapon, transform);
        instance.transform.localPosition = weapon.positionOffset;
        instance.transform.localRotation = Quaternion.Euler(weapon.rotationOffset);
        instance.gameObject.name = weapon.weaponName;
        equippedWeapon = instance;

        // Assign weapon transform to WeaponAnimator
        WeaponAnimator weaponAnimator = GetComponent<WeaponAnimator>();
        if (weaponAnimator != null)
            weaponAnimator.WeaponTransform = instance.transform;

        // Apply stat modifiers to the attack on the player
        if (combatController != null)
            equippedWeapon.ApplyModifiers(combatController.currentAttack);

        Debug.Log($"WeaponSocket [{socketName}]: Equipped {weapon.weaponName}");
    }

    /// <summary>
    /// Unequips the current weapon from this socket.
    /// </summary>
    // public void UnequipWeapon()
    // {
    //     if (equippedWeapon == null) return;

    //     equippedWeapon.RemoveModifiers();

    //     // Clear attack from CombatController
    //     if (combatController != null)
    //         combatController.currentAttack = null;

    //     Destroy(equippedWeapon.gameObject);

    //     WeaponAnimator weaponAnimator = GetComponent<WeaponAnimator>();
    //     if (weaponAnimator != null)
    //         weaponAnimator.WeaponTransform = null;
    //     equippedWeapon = null;

    //     Debug.Log($"WeaponSocket [{socketName}]: Unequipped weapon.");
    // }
    public void UnequipWeapon()
    {
        if (equippedWeapon == null) return;

        if (combatController != null)
            equippedWeapon.RemoveModifiers(combatController.currentAttack);

        WeaponAnimator weaponAnimator = GetComponent<WeaponAnimator>();
        if (weaponAnimator != null)
            weaponAnimator.WeaponTransform = null;

        Destroy(equippedWeapon.gameObject);
        equippedWeapon = null;

        Debug.Log($"WeaponSocket [{socketName}]: Unequipped weapon.");
    }
}