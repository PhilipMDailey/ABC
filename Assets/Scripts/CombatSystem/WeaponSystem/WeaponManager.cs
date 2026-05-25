using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the player's weapon inventory and equipped weapons.
/// Handles switching between weapons and tracking what's available.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Sockets")]
    [Tooltip("The primary weapon socket (e.g. right hand).")]
    public WeaponSocket primarySocket;

    [Header("Inventory")]
    [Tooltip("All weapons available to the player.")]
    public List<WeaponBase> weaponInventory = new List<WeaponBase>();

    [Tooltip("Index of the weapon to equip at start.")]
    public int startingWeaponIndex = 0;

    private int currentWeaponIndex = 0;

    void Start()
    {
        // Equip starting weapon if inventory is populated
        if (weaponInventory.Count > 0 && primarySocket != null)
        {
            currentWeaponIndex = Mathf.Clamp(startingWeaponIndex, 0, weaponInventory.Count - 1);
            primarySocket.EquipWeapon(weaponInventory[currentWeaponIndex]);
        }
    }

    /// <summary>
    /// Equips a specific weapon from the inventory by index.
    /// </summary>
    public void EquipWeaponByIndex(int index)
    {
        if (index < 0 || index >= weaponInventory.Count)
        {
            Debug.LogWarning($"WeaponManager: Invalid weapon index {index}.");
            return;
        }

        currentWeaponIndex = index;
        primarySocket?.EquipWeapon(weaponInventory[index]);
    }

    /// <summary>
    /// Cycles to the next weapon in the inventory.
    /// </summary>
    public void CycleNextWeapon()
    {
        if (weaponInventory.Count == 0) return;
        currentWeaponIndex = (currentWeaponIndex + 1) % weaponInventory.Count;
        primarySocket?.EquipWeapon(weaponInventory[currentWeaponIndex]);
    }

    /// <summary>
    /// Cycles to the previous weapon in the inventory.
    /// </summary>
    public void CyclePreviousWeapon()
    {
        if (weaponInventory.Count == 0) return;
        currentWeaponIndex = (currentWeaponIndex - 1 + weaponInventory.Count) % weaponInventory.Count;
        primarySocket?.EquipWeapon(weaponInventory[currentWeaponIndex]);
    }

    /// <summary>
    /// Adds a weapon to the inventory.
    /// </summary>
    public void AddWeapon(WeaponBase weapon)
    {
        if (!weaponInventory.Contains(weapon))
            weaponInventory.Add(weapon);
    }

    /// <summary>
    /// Removes a weapon from the inventory.
    /// </summary>
    public void RemoveWeapon(WeaponBase weapon)
    {
        weaponInventory.Remove(weapon);
    }

    /// <summary>
    /// Returns the currently equipped weapon.
    /// </summary>
    public WeaponBase GetEquippedWeapon()
    {
        return primarySocket?.equippedWeapon;
    }
}