using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Basic mob melee attack.
/// A small cube sweeps forward from the mob, detecting hits along its path.
/// Fades in and out over the duration of the attack.
/// </summary>
public class MobBasicMeleeAttack : MobAttackBase
{
    [Header("Cube Settings")]
    [Tooltip("Material used for the attack visual indicator.")]
    public Material attackIndicatorMaterial;

    [Tooltip("Size of the attack cube indicator.")]
    public Vector3 cubeSize = new Vector3(0.4f, 0.4f, 0.4f);

    [Tooltip("How far forward the cube travels from the mob.")]
    public float attackDistance = 2f;

    [Tooltip("Duration of the forward sweep in seconds.")]
    public float sweepDuration = 0.3f;

    [Tooltip("Color of the attack cube.")]
    public Color cubeColor = new Color(1f, 0.2f, 0.2f, 0.8f);

    [Header("Fade Settings")]
    [Tooltip("Fraction of the sweep used to fade in (0 - 0.5).")]
    [Range(0f, 0.5f)]
    public float fadeInFraction = 0.2f;

    [Tooltip("Fraction of the sweep used to fade out (0 - 0.5).")]
    [Range(0f, 0.5f)]
    public float fadeOutFraction = 0.3f;

    [Header("Hit Settings")]
    [Tooltip("Layer mask for detecting valid targets.")]
    public LayerMask targetLayers;

    protected override void PerformAttack(ICombatant attacker)
    {
        StartCoroutine(SweepRoutine(attacker));
    }

    IEnumerator SweepRoutine(ICombatant attacker)
    {
        GameObject cube = CreateCube();
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        Material cubeMat = cubeRenderer.material;

        HashSet<ICombatant> alreadyHit = new HashSet<ICombatant>();

        float elapsed = 0f;
        Vector3 startPosition = transform.position + transform.forward * 0.5f;
        Vector3 endPosition = transform.position + transform.forward * attackDistance;

        while (elapsed < sweepDuration)
        {
            float progress = elapsed / sweepDuration;

            // Move cube forward along sweep
            cube.transform.position = Vector3.Lerp(startPosition, endPosition, progress);

            // Fade in / out
            float alpha = CalculateAlpha(progress);
            Color c = cubeColor;
            c.a = alpha;
            cubeMat.SetColor("_BaseColor", c);

            // Hit detection
            DetectHits(cube.transform.position, attacker, alreadyHit);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(cube);
    }

    void DetectHits(Vector3 position, ICombatant attacker, HashSet<ICombatant> alreadyHit)
    {
        Collider[] hits = Physics.OverlapBox(position, cubeSize / 2f, transform.rotation, targetLayers);
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

    float CalculateAlpha(float progress)
    {
        if (progress < fadeInFraction)
            return Mathf.InverseLerp(0f, fadeInFraction, progress);

        if (progress > 1f - fadeOutFraction)
            return Mathf.InverseLerp(1f, 1f - fadeOutFraction, progress);

        return 1f;
    }

    GameObject CreateCube()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "MobAttackCube";
        cube.transform.localScale = cubeSize;

        Destroy(cube.GetComponent<Collider>());

        Renderer r = cube.GetComponent<Renderer>();
        if (attackIndicatorMaterial != null)
            r.material = new Material(attackIndicatorMaterial);
        else
            Debug.LogWarning("MobMeleeAttack: No attackIndicatorMaterial assigned.");

        return cube;
    }

    void OnDrawGizmosSelected()
    {
        // Show attack reach
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + transform.forward * attackDistance, cubeSize);
    }
}