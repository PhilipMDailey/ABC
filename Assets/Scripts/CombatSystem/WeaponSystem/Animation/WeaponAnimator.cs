// using UnityEngine;

// /// <summary>
// /// Sits on the weapon socket and owns the weapon animation state machine.
// /// Drives the weapon GameObject's transform based on the current state.
// /// </summary>
// public class WeaponAnimator : MonoBehaviour
// {
//     [Header("Weapon Reference")]
//     [Tooltip("The weapon GameObject being animated. Assigned automatically by WeaponSocket.")]
//     public Transform WeaponTransform;

//     [Header("Idle Settings")]
//     [Tooltip("Local position of the weapon when idle (held at side).")]
//     public Vector3 idlePosition = new Vector3(0.5f, -0.5f, 0.3f);

//     [Tooltip("Local rotation of the weapon when idle.")]
//     public Vector3 idleRotation = new Vector3(0f, 0f, -45f);

//     [Header("Block Settings")]
//     [Tooltip("Local position of the weapon when blocking.")]
//     public Vector3 blockPosition = new Vector3(0.3f, 0.2f, 0.4f);

//     [Tooltip("Local rotation of the weapon when blocking.")]
//     public Vector3 blockRotation = new Vector3(45f, 0f, 0f);

//     [Header("Transition Settings")]
//     [Tooltip("How fast the weapon lerps between states.")]
//     public float transitionSpeed = 10f;

//     // Current state
//     private WeaponState currentState;

//     void Start()
//     {
//         TransitionTo(new WeaponIdleState(this));
//     }

//     void Update()
//     {
//         Debug.Log($"WeaponAnimator: State={GetCurrentStateName()}, WeaponTransform={WeaponTransform?.name ?? "NULL"}");
        
//         if (WeaponTransform == null) return;
//         currentState?.OnUpdate();
//     }

//     /// <summary>
//     /// Transitions the weapon to a new animation state.
//     /// </summary>
//     public void TransitionTo(WeaponState newState)
//     {
//         currentState?.OnExit();
//         currentState = newState;
//         currentState?.OnEnter();
//     }

//     /// <summary>
//     /// Returns the current state name for debugging.
//     /// </summary>
//     public string GetCurrentStateName()
//     {
//         return currentState?.GetType().Name ?? "None";
//     }

//     /// <summary>
//     /// Triggers the attack state. Called by PlayerBasicAttack.
//     /// Returns the WeaponAttackState so PlayerBasicAttack can drive it.
//     /// </summary>
//     public WeaponAttackState TriggerAttack()
//     {
//         WeaponAttackState attackState = new WeaponAttackState(this);
//         TransitionTo(attackState);
//         return attackState;
//     }

//     /// <summary>
//     /// Triggers the block state. Called by PlayerController.
//     /// </summary>
//     public void TriggerBlock()
//     {
//         if (currentState is WeaponAttackState) return; // Can't block during attack
//         TransitionTo(new WeaponBlockState(this));
//     }

//     /// <summary>
//     /// Ends the block and returns to idle. Called by PlayerController on block release.
//     /// </summary>
//     public void EndBlock()
//     {
//         if (currentState is WeaponBlockState)
//             TransitionTo(new WeaponReturnState(this));
//     }
// }





using UnityEngine;

/// <summary>
/// Sits on the weapon socket and owns the weapon animation state machine.
/// Drives the weapon model and visual indicator based on the current state.
/// Reads attack path geometry from AttackPath during attacks.
/// </summary>
public class WeaponAnimator : MonoBehaviour
{
    [Header("Weapon Reference")]
    [Tooltip("The weapon GameObject being animated. Assigned automatically by WeaponSocket.")]
    public Transform WeaponTransform;

    [Header("Idle Settings")]
    [Tooltip("Local position of the weapon when idle.")]
    public Vector3 idlePosition = new Vector3(0.5f, -0.5f, 0.3f);

    [Tooltip("Local rotation of the weapon when idle.")]
    public Vector3 idleRotation = new Vector3(0f, 0f, -45f);

    [Header("Block Settings")]
    [Tooltip("Local position of the weapon when blocking.")]
    public Vector3 blockPosition = new Vector3(0.3f, 0.2f, 0.4f);

    [Tooltip("Local rotation of the weapon when blocking.")]
    public Vector3 blockRotation = new Vector3(45f, 0f, 0f);

    [Header("Transition Settings")]
    [Tooltip("How fast the weapon lerps between states.")]
    public float transitionSpeed = 10f;

    [Header("Visual Indicator Settings")]
    [Tooltip("Material used for the attack visual indicator.")]
    public Material attackIndicatorMaterial;

    [Tooltip("Color of the attack indicator.")]
    public Color indicatorColor = new Color(1f, 0.2f, 0.2f, 0.8f);

    [Tooltip("Fraction of the path used to fade in (0 - 0.5).")]
    [Range(0f, 0.5f)]
    public float fadeInFraction = 0.2f;

    [Tooltip("Fraction of the path used to fade out (0 - 0.5).")]
    [Range(0f, 0.5f)]
    public float fadeOutFraction = 0.3f;

    [Tooltip("Radius of the visual indicator capsule.")]
    public float indicatorRadius = 0.05f;

    // Current state
    private WeaponState currentState;

    // Active indicator
    private GameObject activeIndicator;
    private Material activeIndicatorMat;

    void Start()
    {
        TransitionTo(new WeaponIdleState(this));
    }

    void Update()
    {
        if (WeaponTransform == null) return;
        currentState?.OnUpdate();
    }

    // ─── State Machine ────────────────────────────────────────────────────────

    public void TransitionTo(WeaponState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState?.OnEnter();
    }

    public string GetCurrentStateName()
    {
        return currentState?.GetType().Name ?? "None";
    }

    /// <summary>
    /// Triggers the attack state with the given path.
    /// Returns the WeaponAttackState so PlayerBasicAttack can drive completion.
    /// </summary>
    public WeaponAttackState TriggerAttack(AttackPath path)
    {
        WeaponAttackState attackState = new WeaponAttackState(this, path);
        TransitionTo(attackState);
        return attackState;
    }

    public void TriggerBlock()
    {
        if (currentState is WeaponAttackState) return;
        TransitionTo(new WeaponBlockState(this));
    }

    public void EndBlock()
    {
        if (currentState is WeaponBlockState)
            TransitionTo(new WeaponReturnState(this));
    }

    // ─── Visual Indicator ─────────────────────────────────────────────────────

    /// <summary>
    /// Creates the attack visual indicator. Called by WeaponAttackState on enter.
    /// </summary>
    public void CreateIndicator()
    {
        if (activeIndicator != null)
            DestroyIndicator();

        activeIndicator = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        activeIndicator.name = "BladeIndicator";
        Destroy(activeIndicator.GetComponent<Collider>());

        Renderer r = activeIndicator.GetComponent<Renderer>();
        if (attackIndicatorMaterial != null)
        {
            activeIndicatorMat = new Material(attackIndicatorMaterial);
            r.material = activeIndicatorMat;
        }
        else
        {
            Debug.LogWarning("WeaponAnimator: No attackIndicatorMaterial assigned.");
        }
    }

    /// <summary>
    /// Updates the indicator and weapon transform to match current blade positions.
    /// Called each frame by WeaponAttackState.
    /// </summary>
    public void UpdateAttackTransforms(Vector3 hiltWorld, Vector3 tipWorld, float progress)
    {
        // Update weapon model
        if (WeaponTransform != null)
        {
            WeaponTransform.position = hiltWorld;
            Vector3 bladeDirection = (tipWorld - hiltWorld).normalized;
            if (bladeDirection != Vector3.zero)
                WeaponTransform.rotation = Quaternion.LookRotation(bladeDirection);
        }

        // Update indicator
        if (activeIndicator != null)
        {
            activeIndicator.transform.position = (hiltWorld + tipWorld) / 2f;

            Vector3 bladeDirection = (tipWorld - hiltWorld).normalized;
            if (bladeDirection != Vector3.zero)
                activeIndicator.transform.rotation = Quaternion.LookRotation(bladeDirection) * Quaternion.Euler(90f, 0f, 0f);

            float bladeLength = Vector3.Distance(hiltWorld, tipWorld);
            activeIndicator.transform.localScale = new Vector3(indicatorRadius * 2f, bladeLength / 2f, indicatorRadius * 2f);

            // Fade in / out
            if (activeIndicatorMat != null)
            {
                float alpha = CalculateAlpha(progress);
                Color c = indicatorColor;
                c.a = alpha;
                activeIndicatorMat.SetColor("_BaseColor", c);
            }
        }
    }

    /// <summary>
    /// Destroys the active indicator. Called by WeaponAttackState on exit.
    /// </summary>
    public void DestroyIndicator()
    {
        if (activeIndicator != null)
        {
            Destroy(activeIndicator);
            activeIndicator = null;
            activeIndicatorMat = null;
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
}