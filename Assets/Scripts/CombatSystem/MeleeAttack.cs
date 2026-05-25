using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Melee attack implementation.
/// A sphere arcs in front of the attacker along a configurable plane and angle,
/// fading in and out, detecting hits against any ICombatant along the arc.
/// </summary>
public class MeleeAttack : AttackBase
{
    // ─── Arc Settings ─────────────────────────────────────────────────────────

    [Header("Arc Settings")]

    [Tooltip("Total degrees the sphere travels through during the attack.")]
    [Range(10f, 360f)]
    public float attackArc = 180f;

    [Tooltip("The plane the arc follows.\n" +
             "Vertical = overhead chop\n" +
             "Horizontal = side sweep\n" +
             "Diagonal = tilted between the two")]
    public ArcPlane arcPlane = ArcPlane.Vertical;

    [Tooltip("When ArcPlane is Diagonal, this controls the tilt in degrees (0 = Horizontal, 90 = Vertical).")]
    [Range(0f, 90f)]
    public float diagonalTilt = 45f;

    [Tooltip("How fast the sphere travels along the arc in degrees per second.")]
    public float attackSpeed = 300f;

    [Tooltip("How far the sphere orbits from the attacker's center.")]
    public float attackDistance = 1.5f;

    [Tooltip("How much the attack eases in at the start of the arc. 0 = no ease, 1 = full ease.")]
    [Range(0f, 1f)]
    public float attackEaseIn = 0.3f;

    [Tooltip("How much the attack eases out at the end of the arc. 0 = no ease, 1 = full ease.")]
    [Range(0f, 1f)]
    public float attackEaseOut = 0.3f;

    // ─── Sphere Settings ──────────────────────────────────────────────────────

    [Header("Sphere Settings")]

    [Tooltip("Assign a pre-made transparent URP/Unlit material here.")]
    public Material attackSphereMaterial;

    [Tooltip("Radius of the hit detection sphere.")]
    public float sphereSize = 0.3f;

    [Tooltip("Color of the attack sphere.")]
    public Color sphereColor = new Color(1f, 0.2f, 0.2f, 0.8f);

    [Tooltip("Fraction of the arc used to fade in (0.0 - 0.5).")]
    [Range(0f, 0.5f)]
    public float fadeInFraction = 0.2f;

    [Tooltip("Fraction of the arc used to fade out (0.0 - 0.5).")]
    [Range(0f, 0.5f)]
    public float fadeOutFraction = 0.3f;

    // ─── Hit Settings ─────────────────────────────────────────────────────────

    [Header("Hit Settings")]

    [Tooltip("Layer mask for detecting valid targets.")]
    public LayerMask targetLayers;

    [Tooltip("Color the attacker flashes when the attack begins.")]
    public Color attackerFlashColor = new Color(1f, 0.5f, 0.5f, 1f);

    [Tooltip("How long the attacker color flash lasts.")]
    public float flashDuration = 0.1f;

    // ─── Arc Plane Enum ───────────────────────────────────────────────────────

    public enum ArcPlane
    {
        Vertical,
        Horizontal,
        Diagonal
    }

    // ─── Attack Execution ─────────────────────────────────────────────────────

    protected override void PerformAttack(ICombatant attacker)
    {
        StartCoroutine(ArcRoutine(attacker));
    }

    IEnumerator ArcRoutine(ICombatant attacker)
    {
        // --- Setup sphere ---
        GameObject sphere = CreateSphere();
        Renderer sphereRenderer = sphere.GetComponent<Renderer>();
        Material sphereMatInstance = sphereRenderer.material;

        // --- Flash attacker ---
        StartCoroutine(FlashAttacker());

        // --- Track already-hit targets to prevent multi-hit ---
        HashSet<ICombatant> alreadyHit = new HashSet<ICombatant>();

        // --- Arc traversal ---
        float totalDegrees = attackArc;
        float degreesTraversed = 0f;

        while (degreesTraversed < totalDegrees)
        {
            float progress = degreesTraversed / totalDegrees;

            // Update sphere position along arc
            sphere.transform.position = GetArcPosition(degreesTraversed);

            // Fade in / out alpha
            float alpha = CalculateAlpha(progress);
            Color c = sphereColor;
            c.a = alpha;
            sphereMatInstance.SetColor("_BaseColor", c);

            // Hit detection at current position
            DetectHits(sphere.transform.position, attacker, alreadyHit);

            // Advance arc with easing
            float easedSpeed = attackSpeed * CalculateEase(progress);
            degreesTraversed += easedSpeed * Time.deltaTime;

            yield return null;
        }

        // --- Clean up ---
        Destroy(sphere);
    }

    // ─── Arc Math ─────────────────────────────────────────────────────────────

    Vector3 GetArcPosition(float degrees)
    {
        float halfArc = attackArc / 2f;
        float angle = degrees - halfArc;
        float rad = angle * Mathf.Deg2Rad;
        Vector3 localOffset = Vector3.zero;

        switch (arcPlane)
        {
            case ArcPlane.Vertical:
                localOffset = new Vector3(0f, Mathf.Cos(rad), Mathf.Sin(rad)) * attackDistance;
                break;

            case ArcPlane.Horizontal:
                localOffset = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * attackDistance;
                break;

            case ArcPlane.Diagonal:
                Vector3 horizontal = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
                Vector3 vertical = new Vector3(0f, Mathf.Cos(rad), Mathf.Sin(rad));
                localOffset = Vector3.Lerp(horizontal, vertical, diagonalTilt / 90f) * attackDistance;
                break;
        }

        return transform.position + transform.TransformDirection(localOffset);
    }

    // ─── Alpha Calculation ────────────────────────────────────────────────────

    float CalculateAlpha(float progress)
    {
        if (progress < fadeInFraction)
            return Mathf.InverseLerp(0f, fadeInFraction, progress);

        if (progress > 1f - fadeOutFraction)
            return Mathf.InverseLerp(1f, 1f - fadeOutFraction, progress);

        return 1f;
    }

    // ─── Ease Calculation ─────────────────────────────────────────────────────

    float CalculateEase(float progress)
    {
        float speedMultiplier = 1f;

        if (progress < attackEaseIn && attackEaseIn > 0f)
            speedMultiplier = Mathf.Lerp(0.1f, 1f, progress / attackEaseIn);
        else if (progress > 1f - attackEaseOut && attackEaseOut > 0f)
            speedMultiplier = Mathf.Lerp(0.1f, 1f, (1f - progress) / attackEaseOut);

        return speedMultiplier;
    }

    // ─── Hit Detection ────────────────────────────────────────────────────────

    void DetectHits(Vector3 position, ICombatant attacker, HashSet<ICombatant> alreadyHit)
    {
        Collider[] hits = Physics.OverlapSphere(position, sphereSize, targetLayers);
        foreach (var hit in hits)
        {
            ICombatant target = hit.GetComponent<ICombatant>();
            if (target == null) continue;
            if (target == attacker) continue;
            if (!target.IsAlive) continue;
            if (alreadyHit.Contains(target)) continue;

            alreadyHit.Add(target);
            target.TakeDamage(damage, attacker, position);
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    GameObject CreateSphere()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "AttackSphere";
        sphere.transform.localScale = Vector3.one * sphereSize * 2f;

        Destroy(sphere.GetComponent<Collider>());

        Renderer r = sphere.GetComponent<Renderer>();

        if (attackSphereMaterial != null)
        {
            // Use instanced copy so we can modify alpha independently per attack
            r.material = new Material(attackSphereMaterial);
        }
        else
        {
            Debug.LogWarning("MeleeAttack: No attackSphereMaterial assigned. Assign a transparent URP/Unlit material in the Inspector.");
        }

        return sphere;
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

    // ─── Gizmos ───────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        int steps = 20;
        for (int i = 0; i <= steps; i++)
        {
            float degrees = (attackArc / steps) * i;
            Vector3 pos = GetArcPosition(degrees);
            Gizmos.DrawWireSphere(pos, sphereSize);
        }
    }
}