using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Player melee attack implementation.
/// Handles combat logic only — damage, hit detection, cooldown.
/// Delegates path geometry to AttackPath and visuals to WeaponAnimator.
/// </summary>
public class PlayerBasicAttack : PlayerAttackBase
{
    [Header("Attack Path")]
    [Tooltip("Defines the geometric path of this attack. Assign an ArcAttackPath or other AttackPath subclass.")]
    public AttackPath attackPath;

    [Header("Attack Settings")]
    [Tooltip("How fast the attack travels along the path in degrees per second.")]
    public float attackSpeed = 300f;

    [Header("Blade Settings")]
    [Tooltip("Radius of the hit detection capsule along the blade.")]
    public float bladeRadius = 0.1f;

    [Header("Hit Settings")]
    [Tooltip("Layer mask for detecting valid targets.")]
    public LayerMask targetLayers;

    [Tooltip("Color the attacker flashes when the attack begins.")]
    public Color attackerFlashColor = new Color(1f, 0.5f, 0.5f, 1f);

    [Tooltip("How long the attacker color flash lasts.")]
    public float flashDuration = 0.1f;

    [Header("Weapon Animator")]
    [Tooltip("Drives the weapon model and visual indicator. Found automatically at start.")]
    public WeaponAnimator weaponAnimator;

    void Start()
    {
        if (weaponAnimator == null)
            weaponAnimator = GetComponentInParent<WeaponAnimator>();

        if (weaponAnimator == null)
            weaponAnimator = FindAnyObjectByType<WeaponAnimator>();
    }

    protected override void PerformAttack(ICombatant attacker)
    {
        if (attackPath == null)
        {
            Debug.LogWarning("PlayerBasicAttack: No AttackPath assigned.");
            return;
        }

        StartCoroutine(AttackRoutine(attacker));
    }

    IEnumerator AttackRoutine(ICombatant attacker)
    {
        WeaponAttackState weaponAttackState = weaponAnimator != null
            ? weaponAnimator.TriggerAttack(attackPath)
            : null;

        StartCoroutine(FlashAttacker());

        HashSet<ICombatant> alreadyHit = new HashSet<ICombatant>();

        // attackSpeed acts as a multiplier — higher speed = shorter duration
        float baseDuration = attackPath.GetDuration(attackSpeed);
        float duration = baseDuration / attackSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;

            Vector3 hiltWorld = attackPath.GetHiltPosition(progress, transform);
            Vector3 tipWorld = attackPath.GetTipPosition(progress, transform);

            weaponAttackState?.SetProgress(progress);

            DetectHits(hiltWorld, tipWorld, attacker, alreadyHit);

            // GetEasedSpeed returns a 0-1 multiplier scaled by easing
            float easedMultiplier = attackPath.GetEasedSpeed(progress, 1f);
            elapsed += easedMultiplier * Time.deltaTime;

            yield return null;
        }

        weaponAttackState?.CompleteAttack();
    }

    void DetectHits(Vector3 hiltWorld, Vector3 tipWorld, ICombatant attacker, HashSet<ICombatant> alreadyHit)
    {
        Collider[] hits = Physics.OverlapCapsule(hiltWorld, tipWorld, bladeRadius, targetLayers);

        foreach (var hit in hits)
        {
            ICombatant target = hit.GetComponent<ICombatant>();
            if (target == null) continue;
            if (target == attacker) continue;
            if (!target.IsAlive) continue;
            if (alreadyHit.Contains(target)) continue;

            alreadyHit.Add(target);
            Vector3 impactPosition = (hiltWorld + tipWorld) / 2f;
            target.TakeDamage(damage, attacker, impactPosition);
        }
    }

    IEnumerator FlashAttacker()
    {
        Renderer r = GetComponentInChildren<Renderer>();
        if (r == null) yield break;

        Color original = r.material.color;
        r.material.color = attackerFlashColor;
        yield return new WaitForSeconds(flashDuration);
        r.material.color = original;
    }
}