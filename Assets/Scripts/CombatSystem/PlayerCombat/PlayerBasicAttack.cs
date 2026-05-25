// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// /// <summary>
// /// Player melee attack implementation.
// /// A weapon swings through a configurable arc, with hit detection
// /// running along the full blade length from hilt to tip each frame.
// /// </summary>
// public class PlayerBasicAttack : PlayerAttackBase
// {
//     // ─── Arc Settings ─────────────────────────────────────────────────────────

//     [Header("Arc Settings")]

//     [Tooltip("Total degrees the weapon travels through during the attack.")]
//     [Range(10f, 360f)]
//     public float attackArc = 180f;

//     [Tooltip("The plane the arc follows.\n" +
//              "Vertical = overhead chop\n" +
//              "Horizontal = side sweep\n" +
//              "Diagonal = tilted between the two")]
//     public ArcPlane arcPlane = ArcPlane.Vertical;

//     [Tooltip("When ArcPlane is Diagonal, this controls the tilt in degrees (0 = Horizontal, 90 = Vertical).")]
//     [Range(0f, 90f)]
//     public float diagonalTilt = 45f;

//     [Tooltip("How fast the weapon travels along the arc in degrees per second.")]
//     public float attackSpeed = 300f;

//     [Tooltip("How far the weapon orbits from the attacker's center.")]
//     public float attackDistance = 1.5f;

//     [Tooltip("How much the attack eases in at the start of the arc. 0 = no ease, 1 = full ease.")]
//     [Range(0f, 1f)]
//     public float attackEaseIn = 0.3f;

//     [Tooltip("How much the attack eases out at the end of the arc. 0 = no ease, 1 = full ease.")]
//     [Range(0f, 1f)]
//     public float attackEaseOut = 0.3f;

//     // ─── Weapon Blade Settings ────────────────────────────────────────────────

//     [Header("Weapon Blade Settings")]

//     [Tooltip("Offset from the attacker's position to the hilt (base) of the weapon blade.")]
//     public Vector3 hiltOffset = new Vector3(0f, 0f, 0.5f);

//     [Tooltip("Offset from the attacker's position to the tip of the weapon blade.")]
//     public Vector3 tipOffset = new Vector3(0f, 0f, 1.5f);

//     [Tooltip("Radius of the hit detection capsule along the blade.")]
//     public float bladeRadius = 0.1f;

//     // ─── Sphere Settings ──────────────────────────────────────────────────────

//     [Header("Visual Indicator Settings")]

//     [Tooltip("Material used for the attack visual indicator.")]
//     public Material attackIndicatorMaterial;

//     [Tooltip("Color of the attack indicator.")]
//     public Color indicatorColor = new Color(1f, 0.2f, 0.2f, 0.8f);

//     [Tooltip("Fraction of the arc used to fade in (0.0 - 0.5).")]
//     [Range(0f, 0.5f)]
//     public float fadeInFraction = 0.2f;

//     [Tooltip("Fraction of the arc used to fade out (0.0 - 0.5).")]
//     [Range(0f, 0.5f)]
//     public float fadeOutFraction = 0.3f;

//     // ─── Hit Settings ─────────────────────────────────────────────────────────

//     [Header("Hit Settings")]

//     [Tooltip("Layer mask for detecting valid targets.")]
//     public LayerMask targetLayers;

//     [Tooltip("Color the attacker flashes when the attack begins.")]
//     public Color attackerFlashColor = new Color(1f, 0.5f, 0.5f, 1f);

//     [Tooltip("How long the attacker color flash lasts.")]
//     public float flashDuration = 0.1f;

//     // ─── Arc Plane Enum ───────────────────────────────────────────────────────

//     public enum ArcPlane
//     {
//         Vertical,
//         Horizontal,
//         Diagonal
//     }

//     // ─── Attack Execution ─────────────────────────────────────────────────────

//     protected override void PerformAttack(ICombatant attacker)
//     {
//         StartCoroutine(ArcRoutine(attacker));
//     }

//     IEnumerator ArcRoutine(ICombatant attacker)
//     {
//         // --- Setup visual indicator (capsule between hilt and tip) ---
//         GameObject indicator = CreateIndicator();
//         Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
//         Material indicatorMat = indicatorRenderer.material;

//         // --- Flash attacker ---
//         StartCoroutine(FlashAttacker());

//         // --- Track already-hit targets to prevent multi-hit ---
//         HashSet<ICombatant> alreadyHit = new HashSet<ICombatant>();

//         // --- Arc traversal ---
//         float totalDegrees = attackArc;
//         float degreesTraversed = 0f;

//         while (degreesTraversed < totalDegrees)
//         {
//             float progress = degreesTraversed / totalDegrees;

//             // Calculate hilt and tip world positions along arc
//             Vector3 hiltWorld = GetArcPosition(degreesTraversed, hiltOffset);
//             Vector3 tipWorld = GetArcPosition(degreesTraversed, tipOffset);

//             // Update indicator position and orientation to follow blade
//             UpdateIndicator(indicator, hiltWorld, tipWorld);

//             // Fade in / out alpha
//             float alpha = CalculateAlpha(progress);
//             Color c = indicatorColor;
//             c.a = alpha;
//             indicatorMat.SetColor("_BaseColor", c);

//             // Hit detection along full blade length
//             DetectHits(hiltWorld, tipWorld, attacker, alreadyHit);

//             // Advance arc with easing
//             float easedSpeed = attackSpeed * CalculateEase(progress);
//             degreesTraversed += easedSpeed * Time.deltaTime;

//             yield return null;
//         }

//         // --- Clean up ---
//         Destroy(indicator);
//     }

//     // ─── Arc Math ─────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Returns the world position of an offset point along the arc at a given degree.
//     /// Used to calculate both hilt and tip positions independently.
//     /// </summary>
//     Vector3 GetArcPosition(float degrees, Vector3 offset)
//     {
//         float halfArc = attackArc / 2f;
//         float angle = degrees - halfArc;
//         float rad = angle * Mathf.Deg2Rad;
//         Vector3 localOffset = Vector3.zero;

//         switch (arcPlane)
//         {
//             case ArcPlane.Vertical:
//                 localOffset = new Vector3(0f, Mathf.Cos(rad), Mathf.Sin(rad)) * attackDistance;
//                 break;

//             case ArcPlane.Horizontal:
//                 localOffset = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * attackDistance;
//                 break;

//             case ArcPlane.Diagonal:
//                 Vector3 horizontal = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
//                 Vector3 vertical = new Vector3(0f, Mathf.Cos(rad), Mathf.Sin(rad));
//                 localOffset = Vector3.Lerp(horizontal, vertical, diagonalTilt / 90f) * attackDistance;
//                 break;
//         }

//         // Apply the blade offset in local space relative to attacker orientation
//         Vector3 bladeOffset = transform.TransformDirection(offset);
//         return transform.position + transform.TransformDirection(localOffset) + bladeOffset;
//     }

//     // ─── Alpha Calculation ────────────────────────────────────────────────────

//     float CalculateAlpha(float progress)
//     {
//         if (progress < fadeInFraction)
//             return Mathf.InverseLerp(0f, fadeInFraction, progress);

//         if (progress > 1f - fadeOutFraction)
//             return Mathf.InverseLerp(1f, 1f - fadeOutFraction, progress);

//         return 1f;
//     }

//     // ─── Ease Calculation ─────────────────────────────────────────────────────

//     float CalculateEase(float progress)
//     {
//         float speedMultiplier = 1f;

//         if (progress < attackEaseIn && attackEaseIn > 0f)
//             speedMultiplier = Mathf.Lerp(0.1f, 1f, progress / attackEaseIn);
//         else if (progress > 1f - attackEaseOut && attackEaseOut > 0f)
//             speedMultiplier = Mathf.Lerp(0.1f, 1f, (1f - progress) / attackEaseOut);

//         return speedMultiplier;
//     }

//     // ─── Hit Detection ────────────────────────────────────────────────────────

//     void DetectHits(Vector3 hiltWorld, Vector3 tipWorld, ICombatant attacker, HashSet<ICombatant> alreadyHit)
//     {
//         // OverlapCapsule detects hits along the full blade length
//         Collider[] hits = Physics.OverlapCapsule(hiltWorld, tipWorld, bladeRadius, targetLayers);

//         foreach (var hit in hits)
//         {
//             ICombatant target = hit.GetComponent<ICombatant>();
//             if (target == null) continue;
//             if (target == attacker) continue;
//             if (!target.IsAlive) continue;
//             if (alreadyHit.Contains(target)) continue;

//             alreadyHit.Add(target);
//             // Pass midpoint of blade as impact position for knockback direction
//             Vector3 impactPosition = (hiltWorld + tipWorld) / 2f;
//             target.TakeDamage(damage, attacker, impactPosition);
//         }
//     }

//     // ─── Helpers ─────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Creates a capsule visual indicator representing the blade.
//     /// </summary>
//     GameObject CreateIndicator()
//     {
//         GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Capsule);
//         indicator.name = "BladeIndicator";

//         Destroy(indicator.GetComponent<Collider>());

//         Renderer r = indicator.GetComponent<Renderer>();
//         if (attackIndicatorMaterial != null)
//             r.material = new Material(attackIndicatorMaterial);
//         else
//             Debug.LogWarning("PlayerBasicAttack: No attackIndicatorMaterial assigned.");

//         return indicator;
//     }

//     /// <summary>
//     /// Positions and orients the indicator capsule to match the current blade hilt and tip.
//     /// </summary>
//     void UpdateIndicator(GameObject indicator, Vector3 hiltWorld, Vector3 tipWorld)
//     {
//         // Position at midpoint between hilt and tip
//         indicator.transform.position = (hiltWorld + tipWorld) / 2f;

//         // Orient along blade direction
//         Vector3 bladeDirection = (tipWorld - hiltWorld).normalized;
//         if (bladeDirection != Vector3.zero)
//             indicator.transform.rotation = Quaternion.LookRotation(bladeDirection) * Quaternion.Euler(90f, 0f, 0f);

//         // Scale to match blade length
//         float bladeLength = Vector3.Distance(hiltWorld, tipWorld);
//         indicator.transform.localScale = new Vector3(bladeRadius * 2f, bladeLength / 2f, bladeRadius * 2f);
//     }

//     IEnumerator FlashAttacker()
//     {
//         Renderer r = GetComponentInChildren<Renderer>();
//         if (r == null) yield break;

//         Color original = r.material.color;
//         r.material.color = attackerFlashColor;
//         yield return new WaitForSeconds(flashDuration);
//         r.material.color = original;
//     }

//     // ─── Gizmos ───────────────────────────────────────────────────────────────

//     void OnDrawGizmosSelected()
//     {
//         int steps = 20;
//         for (int i = 0; i <= steps; i++)
//         {
//             float degrees = (attackArc / steps) * i;
//             Vector3 hiltPos = GetArcPosition(degrees, hiltOffset);
//             Vector3 tipPos = GetArcPosition(degrees, tipOffset);

//             // Draw blade at each step
//             Gizmos.color = Color.yellow;
//             Gizmos.DrawLine(hiltPos, tipPos);

//             // Draw hilt and tip spheres
//             Gizmos.color = Color.red;
//             Gizmos.DrawWireSphere(hiltPos, bladeRadius);
//             Gizmos.color = Color.green;
//             Gizmos.DrawWireSphere(tipPos, bladeRadius);
//         }
//     }
// }











// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// /// <summary>
// /// Player melee attack implementation.
// /// A weapon swings through a configurable arc, with hit detection
// /// running along the full blade length from hilt to tip each frame.
// /// </summary>
// public class PlayerBasicAttack : PlayerAttackBase
// {
//     // ─── Arc Settings ─────────────────────────────────────────────────────────

//     [Header("Arc Settings")]

//     [Tooltip("Total degrees the weapon travels through during the attack.")]
//     [Range(10f, 360f)]
//     public float attackArc = 180f;

//     [Tooltip("The plane the arc follows.\n" +
//              "Vertical = overhead chop\n" +
//              "Horizontal = side sweep\n" +
//              "Diagonal = tilted between the two")]
//     public ArcPlane arcPlane = ArcPlane.Vertical;

//     [Tooltip("When ArcPlane is Diagonal, this controls the tilt in degrees (0 = Horizontal, 90 = Vertical).")]
//     [Range(0f, 90f)]
//     public float diagonalTilt = 45f;

//     [Tooltip("How fast the weapon travels along the arc in degrees per second.")]
//     public float attackSpeed = 300f;

//     [Tooltip("How far the weapon orbits from the attacker's center.")]
//     public float attackDistance = 1.5f;

//     [Tooltip("How much the attack eases in at the start of the arc. 0 = no ease, 1 = full ease.")]
//     [Range(0f, 1f)]
//     public float attackEaseIn = 0.3f;

//     [Tooltip("How much the attack eases out at the end of the arc. 0 = no ease, 1 = full ease.")]
//     [Range(0f, 1f)]
//     public float attackEaseOut = 0.3f;

//     // ─── Weapon Blade Settings ────────────────────────────────────────────────

//     [Header("Weapon Blade Settings")]

//     [Tooltip("Offset from the attacker's position to the hilt (base) of the weapon blade.")]
//     public Vector3 hiltOffset = new Vector3(0f, 0f, 0.5f);

//     [Tooltip("Offset from the attacker's position to the tip of the weapon blade.")]
//     public Vector3 tipOffset = new Vector3(0f, 0f, 1.5f);

//     [Tooltip("Radius of the hit detection capsule along the blade.")]
//     public float bladeRadius = 0.1f;

//     // ─── Sphere Settings ──────────────────────────────────────────────────────

//     [Header("Visual Indicator Settings")]

//     [Tooltip("Material used for the attack visual indicator.")]
//     public Material attackIndicatorMaterial;

//     [Tooltip("Color of the attack indicator.")]
//     public Color indicatorColor = new Color(1f, 0.2f, 0.2f, 0.8f);

//     [Tooltip("Fraction of the arc used to fade in (0.0 - 0.5).")]
//     [Range(0f, 0.5f)]
//     public float fadeInFraction = 0.2f;

//     [Tooltip("Fraction of the arc used to fade out (0.0 - 0.5).")]
//     [Range(0f, 0.5f)]
//     public float fadeOutFraction = 0.3f;

//     // ─── Hit Settings ─────────────────────────────────────────────────────────

//     [Header("Hit Settings")]

//     [Tooltip("Layer mask for detecting valid targets.")]
//     public LayerMask targetLayers;

//     [Tooltip("Color the attacker flashes when the attack begins.")]
//     public Color attackerFlashColor = new Color(1f, 0.5f, 0.5f, 1f);

//     [Tooltip("How long the attacker color flash lasts.")]
//     public float flashDuration = 0.1f;

//     // ─── Weapon Animator ──────────────────────────────────────────────────────────

//     [Header("Weapon Animator")]
//     [Tooltip("Reference to the WeaponAnimator on the weapon socket. Assigned automatically if on the same GameObject.")]
//     public WeaponAnimator weaponAnimator;

//     // ─── Arc Plane Enum ───────────────────────────────────────────────────────

//     public enum ArcPlane
//     {
//         Vertical,
//         Horizontal,
//         Diagonal
//     }

//     void Start()
//     {
//         if (weaponAnimator == null)
//             weaponAnimator = GetComponentInParent<WeaponAnimator>();

//         if (weaponAnimator == null)
//             weaponAnimator = FindAnyObjectByType<WeaponAnimator>();

//         Debug.Log($"PlayerBasicAttack: WeaponAnimator found: {weaponAnimator != null}, {weaponAnimator?.gameObject.name}");
//     }

//     // ─── Attack Execution ─────────────────────────────────────────────────────

//     protected override void PerformAttack(ICombatant attacker)
//     {
//         StartCoroutine(ArcRoutine(attacker));
//     }

//     IEnumerator ArcRoutine(ICombatant attacker)
//     {
//         // --- Setup visual indicator (capsule between hilt and tip) ---
//         GameObject indicator = CreateIndicator();
//         Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
//         Material indicatorMat = indicatorRenderer.material;

//         // --- Trigger weapon animation state ---
//         WeaponAttackState weaponAttackState = weaponAnimator != null ? weaponAnimator.TriggerAttack() : null;

//         // --- Flash attacker ---
//         StartCoroutine(FlashAttacker());

//         // --- Track already-hit targets to prevent multi-hit ---
//         HashSet<ICombatant> alreadyHit = new HashSet<ICombatant>();

//         // --- Arc traversal ---
//         float totalDegrees = attackArc;
//         float degreesTraversed = 0f;

//         while (degreesTraversed < totalDegrees)
//         {
//             float progress = degreesTraversed / totalDegrees;

//             // Calculate hilt and tip world positions along arc
//             Vector3 hiltWorld = GetArcPosition(degreesTraversed, hiltOffset);
//             Vector3 tipWorld = GetArcPosition(degreesTraversed, tipOffset);

//             // Update indicator position and orientation to follow blade
//             UpdateIndicator(indicator, hiltWorld, tipWorld);

//             // Drive weapon model to follow arc
//             weaponAttackState?.SetArcTransform(hiltWorld, tipWorld);

//             // Fade in / out alpha
//             float alpha = CalculateAlpha(progress);
//             Color c = indicatorColor;
//             c.a = alpha;
//             indicatorMat.SetColor("_BaseColor", c);

//             // Hit detection along full blade length
//             DetectHits(hiltWorld, tipWorld, attacker, alreadyHit);

//             // Advance arc with easing
//             float easedSpeed = attackSpeed * CalculateEase(progress);
//             degreesTraversed += easedSpeed * Time.deltaTime;

//             yield return null;
//         }

//         // --- Notify weapon attack complete so it returns to idle ---
//         weaponAttackState?.CompleteAttack();

//         // --- Clean up ---
//         Destroy(indicator);
//     }

//     // ─── Arc Math ─────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Returns the world position of an offset point along the arc at a given degree.
//     /// Used to calculate both hilt and tip positions independently.
//     /// </summary>
//     Vector3 GetArcPosition(float degrees, Vector3 offset)
//     {
//         float halfArc = attackArc / 2f;
//         float angle = degrees - halfArc;
//         float rad = angle * Mathf.Deg2Rad;
//         Vector3 localOffset = Vector3.zero;

//         switch (arcPlane)
//         {
//             case ArcPlane.Vertical:
//                 localOffset = new Vector3(0f, Mathf.Cos(rad), Mathf.Sin(rad)) * attackDistance;
//                 break;

//             case ArcPlane.Horizontal:
//                 localOffset = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * attackDistance;
//                 break;

//             case ArcPlane.Diagonal:
//                 Vector3 horizontal = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
//                 Vector3 vertical = new Vector3(0f, Mathf.Cos(rad), Mathf.Sin(rad));
//                 localOffset = Vector3.Lerp(horizontal, vertical, diagonalTilt / 90f) * attackDistance;
//                 break;
//         }

//         // Apply the blade offset in local space relative to attacker orientation
//         Vector3 bladeOffset = transform.TransformDirection(offset);
//         return transform.position + transform.TransformDirection(localOffset) + bladeOffset;
//     }

//     // ─── Alpha Calculation ────────────────────────────────────────────────────

//     float CalculateAlpha(float progress)
//     {
//         if (progress < fadeInFraction)
//             return Mathf.InverseLerp(0f, fadeInFraction, progress);

//         if (progress > 1f - fadeOutFraction)
//             return Mathf.InverseLerp(1f, 1f - fadeOutFraction, progress);

//         return 1f;
//     }

//     // ─── Ease Calculation ─────────────────────────────────────────────────────

//     float CalculateEase(float progress)
//     {
//         float speedMultiplier = 1f;

//         if (progress < attackEaseIn && attackEaseIn > 0f)
//             speedMultiplier = Mathf.Lerp(0.1f, 1f, progress / attackEaseIn);
//         else if (progress > 1f - attackEaseOut && attackEaseOut > 0f)
//             speedMultiplier = Mathf.Lerp(0.1f, 1f, (1f - progress) / attackEaseOut);

//         return speedMultiplier;
//     }

//     // ─── Hit Detection ────────────────────────────────────────────────────────

//     void DetectHits(Vector3 hiltWorld, Vector3 tipWorld, ICombatant attacker, HashSet<ICombatant> alreadyHit)
//     {
//         // OverlapCapsule detects hits along the full blade length
//         Collider[] hits = Physics.OverlapCapsule(hiltWorld, tipWorld, bladeRadius, targetLayers);

//         foreach (var hit in hits)
//         {
//             ICombatant target = hit.GetComponent<ICombatant>();
//             if (target == null) continue;
//             if (target == attacker) continue;
//             if (!target.IsAlive) continue;
//             if (alreadyHit.Contains(target)) continue;

//             alreadyHit.Add(target);
//             // Pass midpoint of blade as impact position for knockback direction
//             Vector3 impactPosition = (hiltWorld + tipWorld) / 2f;
//             target.TakeDamage(damage, attacker, impactPosition);
//         }
//     }

//     // ─── Helpers ─────────────────────────────────────────────────────────────

//     /// <summary>
//     /// Creates a capsule visual indicator representing the blade.
//     /// </summary>
//     GameObject CreateIndicator()
//     {
//         GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Capsule);
//         indicator.name = "BladeIndicator";

//         Destroy(indicator.GetComponent<Collider>());

//         Renderer r = indicator.GetComponent<Renderer>();
//         if (attackIndicatorMaterial != null)
//             r.material = new Material(attackIndicatorMaterial);
//         else
//             Debug.LogWarning("PlayerBasicAttack: No attackIndicatorMaterial assigned.");

//         return indicator;
//     }

//     /// <summary>
//     /// Positions and orients the indicator capsule to match the current blade hilt and tip.
//     /// </summary>
//     void UpdateIndicator(GameObject indicator, Vector3 hiltWorld, Vector3 tipWorld)
//     {
//         // Position at midpoint between hilt and tip
//         indicator.transform.position = (hiltWorld + tipWorld) / 2f;

//         // Orient along blade direction
//         Vector3 bladeDirection = (tipWorld - hiltWorld).normalized;
//         if (bladeDirection != Vector3.zero)
//             indicator.transform.rotation = Quaternion.LookRotation(bladeDirection) * Quaternion.Euler(90f, 0f, 0f);

//         // Scale to match blade length
//         float bladeLength = Vector3.Distance(hiltWorld, tipWorld);
//         indicator.transform.localScale = new Vector3(bladeRadius * 2f, bladeLength / 2f, bladeRadius * 2f);
//     }

//     IEnumerator FlashAttacker()
//     {
//         Renderer r = GetComponentInChildren<Renderer>();
//         if (r == null) yield break;

//         Color original = r.material.color;
//         r.material.color = attackerFlashColor;
//         yield return new WaitForSeconds(flashDuration);
//         r.material.color = original;
//     }

//     // ─── Gizmos ───────────────────────────────────────────────────────────────

//     void OnDrawGizmosSelected()
//     {
//         int steps = 20;
//         for (int i = 0; i <= steps; i++)
//         {
//             float degrees = (attackArc / steps) * i;
//             Vector3 hiltPos = GetArcPosition(degrees, hiltOffset);
//             Vector3 tipPos = GetArcPosition(degrees, tipOffset);

//             // Draw blade at each step
//             Gizmos.color = Color.yellow;
//             Gizmos.DrawLine(hiltPos, tipPos);

//             // Draw hilt and tip spheres
//             Gizmos.color = Color.red;
//             Gizmos.DrawWireSphere(hiltPos, bladeRadius);
//             Gizmos.color = Color.green;
//             Gizmos.DrawWireSphere(tipPos, bladeRadius);
//         }
//     }
// }






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

    // IEnumerator AttackRoutine(ICombatant attacker)
    // {
    //     // Trigger weapon animation
    //     WeaponAttackState weaponAttackState = weaponAnimator != null
    //         ? weaponAnimator.TriggerAttack(attackPath)
    //         : null;

    //     // Flash attacker
    //     StartCoroutine(FlashAttacker());

    //     // Track already-hit targets
    //     HashSet<ICombatant> alreadyHit = new HashSet<ICombatant>();

    //     float duration = attackPath.GetDuration(attackSpeed);
    //     float elapsed = 0f;

    //     while (elapsed < duration)
    //     {
    //         float progress = elapsed / duration;

    //         // Get hilt and tip positions from the path
    //         Vector3 hiltWorld = attackPath.GetHiltPosition(progress, transform);
    //         Vector3 tipWorld = attackPath.GetTipPosition(progress, transform);

    //         // Sync progress to weapon animation state
    //         weaponAttackState?.SetProgress(progress);

    //         // Hit detection along full blade length
    //         DetectHits(hiltWorld, tipWorld, attacker, alreadyHit);

    //         // Advance with easing
    //         float easedSpeed = attackPath.GetEasedSpeed(progress, attackSpeed);
    //         elapsed += (easedSpeed / attackSpeed) * Time.deltaTime;

    //         yield return null;
    //     }

    //     // Notify weapon animation that attack is complete
    //     weaponAttackState?.CompleteAttack();
    // }
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