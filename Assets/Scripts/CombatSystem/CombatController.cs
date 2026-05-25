using UnityEngine;
using System.Collections;
using UnityEngine.AI;

/// <summary>
/// Sits on any combatant (player or mob) and manages their attacks,
/// hit reactions, knockback, health, and death.
/// </summary>
public class CombatController : MonoBehaviour, ICombatant
{
    [Header("Identity")]
    public string combatantName = "Combatant";

    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Attack")]
    [Tooltip("The current attack assigned to this combatant. Swap this to change attack type.")]
    public AttackBase currentAttack;

    [Header("Knockback")]
    [Tooltip("How far the character is pushed on hit in units per second.")]
    public float knockbackForce = 5f;

    [Tooltip("How long the knockback push lasts in seconds.")]
    public float knockbackDuration = 0.2f;

    [Header("Hit Indicator")]
    [Tooltip("How many times the character flashes on hit.")]
    public int hitFlashCount = 3;

    [Tooltip("Fraction of each flash cycle used for ease in (0 - 0.5).")]
    [Range(0f, 0.5f)]
    public float hitEaseIn = 0.2f;

    [Tooltip("Fraction of each flash cycle used for ease out (0 - 0.5).")]
    [Range(0f, 0.5f)]
    public float hitEaseOut = 0.2f;

    [Tooltip("Total duration of the entire flash animation in seconds.")]
    public float hitIndicatorSpeed = 0.6f;

    [Tooltip("Color of the hit flash.")]
    public Color hitFlashColor = Color.red;

    [Header("Death")]
    [Tooltip("How long the character remains stationary after death before fading.")]
    public float deathTimeout = 10f;

    [Tooltip("How long the fade out takes before the character is destroyed.")]
    public float deathFadeOut = 2f;

    [Tooltip("A pre-made transparent URP/Unlit material used for the death fade.")]
    public Material deathFadeMaterial;

    [Tooltip("The color the character transitions to upon death before fading out.")]
    public Color deathColor = Color.black;

    // [Tooltip("How far the character gets launched back on death.")]
    // public float deathLaunchForce = 5f;

    // [Tooltip("How fast the character spins on death in degrees per second.")]
    // public float deathSpinSpeed = 720f;

    [Tooltip("Force applied on death in X, Y, Z directions.")]
    public Vector3 deathForce = new Vector3(0f, 5f, 0f);

    [Tooltip("Torque applied on death to cause spinning/tumbling.")]
    public Vector3 deathTorque = new Vector3(0f, 10f, 5f);

    // ICombatant implementation
    public string CombatantName => combatantName;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => !isDead;

    private float currentHealth;
    private bool isBeingKnockedBack = false;
    private bool isDead = false;
    private Color originalColor;

    // Events
    public event System.Action<float, ICombatant> OnDamageReceived;
    public event System.Action<ICombatant> OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;

        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null)
        {
            r.material = new Material(r.material);
            originalColor = r.material.GetColor("_BaseColor");
        }
    }

    // ─── Attack ───────────────────────────────────────────────────────────────

    public bool TryAttack()
    {
        if (currentAttack == null)
        {
            Debug.LogWarning($"{combatantName}: No attack assigned to CombatController.");
            return false;
        }

        if (!IsAlive) return false;
        return currentAttack.TryAttack(this);
    }

    // ─── ICombatant ───────────────────────────────────────────────────────────

    public void TakeDamage(float amount, ICombatant source, Vector3 impactPosition)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnDamageReceived?.Invoke(amount, source);

        Debug.Log($"{combatantName} took {amount} damage. Health: {currentHealth}/{maxHealth}");

        StartCoroutine(HitFlashRoutine());

        if (!isBeingKnockedBack)
            StartCoroutine(KnockbackRoutine(impactPosition));

        UpdateHealthColor();

        if (currentHealth <= 0f)
            Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null)
        {
            r.SetPropertyBlock(null);
            r.material.SetColor("_BaseColor", deathColor);
        }

        OnDeath?.Invoke(this);
        Debug.Log($"{combatantName} has died.");

        MobBase mob = GetComponent<MobBase>();
        if (mob != null)
            MobManager.Instance?.UnregisterMob(mob);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        // Disable CharacterController
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        if (currentAttack != null)
            currentAttack.enabled = false;

        // Hand off to Rigidbody for death physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;

            // Calculate launch direction away from player
            GameObject playerObj = GameObject.FindWithTag("Player");
            Vector3 awayFromPlayer = playerObj != null
                ? (transform.position - playerObj.transform.position).normalized
                : -transform.forward;

            // Apply force relative to the direction away from player
            Vector3 force = transform.TransformDirection(deathForce);
            force += awayFromPlayer * deathForce.magnitude;
            rb.AddForce(force, ForceMode.Impulse);
            rb.AddTorque(deathTorque, ForceMode.Impulse);
        }

        // StartCoroutine(DeathLaunchRoutine());
        StartCoroutine(DeathRoutine());
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthColor();
    }

    // ─── Death Routine ────────────────────────────────────────────────────────
    
    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathTimeout);

        Renderer r = GetComponentInChildren<Renderer>();

        // Capture the position of the corpse before shrinking into oblivion.
        Vector3 originalPosition = transform.position;
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < deathFadeOut)
        {
            float progress = Mathf.Lerp(1f, 0f, elapsed / deathFadeOut);

            // Shrink
            transform.localScale = originalScale * progress;

            // Keep grounded — offset Y by the difference in height caused by scaling
            float originalHeight = originalScale.y;
            float currentHeight = originalHeight * progress;
            float heightDifference = (originalHeight - currentHeight) / 2f;
            transform.position = new Vector3(
                transform.position.x,
                originalPosition.y - heightDifference,
                transform.position.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }


    // ─── Health Color ─────────────────────────────────────────────────────────

    void UpdateHealthColor()
    {
        Renderer r = GetComponentInChildren<Renderer>();
        if (r == null) return;

        float healthPercent = currentHealth / maxHealth;
        Color targetColor = Color.Lerp(Color.black, originalColor, healthPercent);

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);
        block.SetColor("_BaseColor", targetColor);
        r.SetPropertyBlock(block);
    }

    // ─── Hit Flash ────────────────────────────────────────────────────────────

    IEnumerator HitFlashRoutine()
    {
        Renderer r = GetComponentInChildren<Renderer>();
        if (r == null) yield break;

        float flashDuration = hitIndicatorSpeed / hitFlashCount;
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        for (int i = 0; i < hitFlashCount; i++)
        {
            float intensity = Mathf.Lerp(1f, 0.2f, (float)i / Mathf.Max(hitFlashCount - 1, 1));
            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                float progress = elapsed / flashDuration;
                float alpha = CalculateFlashAlpha(progress) * intensity;

                Color flashColor = Color.Lerp(originalColor, hitFlashColor, alpha);

                r.GetPropertyBlock(block);
                block.SetColor("_BaseColor", flashColor);
                r.SetPropertyBlock(block);

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Restore health-based color after flash
        UpdateHealthColor();
    }

    float CalculateFlashAlpha(float progress)
    {
        if (progress < hitEaseIn && hitEaseIn > 0f)
            return Mathf.Lerp(0f, 1f, progress / hitEaseIn);

        if (progress > 1f - hitEaseOut && hitEaseOut > 0f)
            return Mathf.Lerp(1f, 0f, (progress - (1f - hitEaseOut)) / hitEaseOut);

        return 1f;
    }

    // ─── Knockback ────────────────────────────────────────────────────────────

    IEnumerator KnockbackRoutine(Vector3 impactPosition)
    {
        isBeingKnockedBack = true;

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector3 knockbackSource = playerObj != null ? playerObj.transform.position : impactPosition;
        Vector3 knockbackDirection = (transform.position - knockbackSource);
        knockbackDirection.y = 0f;
        knockbackDirection.Normalize();

        // Use NavMeshAgent if available, otherwise fall back to transform
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        CharacterController controller = GetComponent<CharacterController>();

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            if (isDead) yield break;

            float progress = elapsed / knockbackDuration;
            float easedForce = Mathf.Lerp(knockbackForce, 0f, progress);
            Vector3 movement = knockbackDirection * easedForce * Time.deltaTime;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.Move(movement);
            }
            else if (controller != null)
            {
                controller.Move(movement);
            }
            else
            {
                transform.position += movement;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        isBeingKnockedBack = false;
    }
}