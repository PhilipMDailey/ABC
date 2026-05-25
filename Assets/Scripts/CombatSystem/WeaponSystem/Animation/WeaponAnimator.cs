// using UnityEngine;

// /// <summary>
// /// Sits on the weapon socket and owns the weapon animation state machine.
// /// Drives the weapon model and visual indicator based on the current state.
// /// Reads attack path geometry from AttackPath during attacks.
// /// </summary>
// public class WeaponAnimator : MonoBehaviour
// {
//     [Header("Weapon Reference")]
//     [Tooltip("The weapon GameObject being animated. Assigned automatically by WeaponSocket.")]
//     public Transform WeaponTransform;

//     [Header("Idle Settings")]
//     [Tooltip("Local position of the weapon when idle.")]
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

//     [Header("Visual Indicator Settings")]
//     [Tooltip("Material used for the attack visual indicator.")]
//     public Material attackIndicatorMaterial;

//     [Tooltip("Color of the attack indicator.")]
//     public Color indicatorColor = new Color(1f, 0.2f, 0.2f, 0.8f);

//     [Tooltip("Fraction of the path used to fade in (0 - 0.5).")]
//     [Range(0f, 0.5f)]
//     public float fadeInFraction = 0.2f;

//     [Tooltip("Fraction of the path used to fade out (0 - 0.5).")]
//     [Range(0f, 0.5f)]
//     public float fadeOutFraction = 0.3f;

//     [Tooltip("Radius of the visual indicator capsule.")]
//     public float indicatorRadius = 0.05f;

//     [Header("Debug")]
//     public bool showAttackIndicator = true;

//     // Current state
//     private WeaponState currentState;

//     // Active indicator
//     private GameObject activeIndicator;
//     private Material activeIndicatorMat;

//     void Start()
//     {
//         TransitionTo(new WeaponIdleState(this));
//     }

//     void Update()
//     {
//         if (WeaponTransform == null) return;
//         currentState?.OnUpdate();
//     }

//     // ─── State Machine ────────────────────────────────────────────────────────

//     public void TransitionTo(WeaponState newState)
//     {
//         currentState?.OnExit();
//         currentState = newState;
//         currentState?.OnEnter();
//     }

//     public string GetCurrentStateName()
//     {
//         return currentState?.GetType().Name ?? "None";
//     }

//     /// <summary>
//     /// Triggers the attack state with the given path.
//     /// Returns the WeaponAttackState so PlayerBasicAttack can drive completion.
//     /// </summary>
//     public WeaponAttackState TriggerAttack(AttackPath path)
//     {
//         WeaponAttackState attackState = new WeaponAttackState(this, path);
//         TransitionTo(attackState);
//         return attackState;
//     }

//     public void TriggerBlock()
//     {
//         if (currentState is WeaponAttackState) return;
//         TransitionTo(new WeaponBlockState(this));
//     }

//     public void EndBlock()
//     {
//         if (currentState is WeaponBlockState)
//             TransitionTo(new WeaponReturnState(this));
//     }

//     // ─── Visual Indicator ─────────────────────────────────────────────────────

//     /// <summary>
//     /// Creates the attack visual indicator. Called by WeaponAttackState on enter.
//     /// </summary>
//     public void CreateIndicator()
//     {
//         if (activeIndicator != null)
//             DestroyIndicator();

//         activeIndicator = GameObject.CreatePrimitive(PrimitiveType.Capsule);
//         activeIndicator.name = "BladeIndicator";
//         Destroy(activeIndicator.GetComponent<Collider>());

//         Renderer r = activeIndicator.GetComponent<Renderer>();
//         r.enabled = showAttackIndicator;

//         if (attackIndicatorMaterial != null)
//         {
//             activeIndicatorMat = new Material(attackIndicatorMaterial);
//             r.material = activeIndicatorMat;
//         }
//         else
//         {
//             Debug.LogWarning("WeaponAnimator: No attackIndicatorMaterial assigned.");
//         }
//     }

//     /// <summary>
//     /// Updates the indicator and weapon transform to match current blade positions.
//     /// Called each frame by WeaponAttackState.
//     /// </summary>
//     public void UpdateAttackTransforms(Vector3 hiltWorld, Vector3 tipWorld, float progress)
//     {
//         if (WeaponTransform != null)
//         {
//             WeaponMarkers markers = WeaponTransform.GetComponent<WeaponMarkers>();
//             Vector3 bladeDirection = (tipWorld - hiltWorld).normalized;

//             if (bladeDirection != Vector3.zero)
//             {
//                 // Base rotation to align blade with path
//                 Quaternion bladeRotation = Quaternion.LookRotation(bladeDirection);

//                 // Apply roll around blade axis if a SplineAttackPath is in use
//                 SplineAttackPath splinePath = GetComponentInParent<PlayerBasicAttack>()
//                     ?.attackPath as SplineAttackPath;
//                 if (splinePath != null)
//                 {
//                     float rollAngle = splinePath.GetRollAngle(progress);
//                     Quaternion rollRotation = Quaternion.AngleAxis(rollAngle, bladeDirection);
//                     bladeRotation = rollRotation * bladeRotation;
//                 }

//                 WeaponTransform.rotation = bladeRotation;
//             }

//             if (markers != null && markers.hiltMarker != null)
//             {
//                 Vector3 markerOffset = WeaponTransform.position - markers.hiltMarker.position;
//                 WeaponTransform.position = hiltWorld + markerOffset;
//             }
//             else
//             {
//                 WeaponTransform.position = hiltWorld;
//             }
//         }

//         // Update indicator
//         if (activeIndicator != null && showAttackIndicator)
//         {
//             activeIndicator.transform.position = (hiltWorld + tipWorld) / 2f;
//             Vector3 bladeDirection = (tipWorld - hiltWorld).normalized;
//             if (bladeDirection != Vector3.zero)
//                 activeIndicator.transform.rotation = Quaternion.LookRotation(bladeDirection)
//                     * Quaternion.Euler(90f, 0f, 0f);
//             float bladeLength = Vector3.Distance(hiltWorld, tipWorld);
//             activeIndicator.transform.localScale = new Vector3(
//                 indicatorRadius * 2f, bladeLength / 2f, indicatorRadius * 2f);

//             if (activeIndicatorMat != null)
//             {
//                 float alpha = CalculateAlpha(progress);
//                 Color c = indicatorColor;
//                 c.a = alpha;
//                 activeIndicatorMat.SetColor("_BaseColor", c);
//             }
//         }
//     }

//     /// <summary>
//     /// Destroys the active indicator. Called by WeaponAttackState on exit.
//     /// </summary>
//     public void DestroyIndicator()
//     {
//         if (activeIndicator != null)
//         {
//             Destroy(activeIndicator);
//             activeIndicator = null;
//             activeIndicatorMat = null;
//         }
//     }

//     float CalculateAlpha(float progress)
//     {
//         if (progress < fadeInFraction)
//             return Mathf.InverseLerp(0f, fadeInFraction, progress);

//         if (progress > 1f - fadeOutFraction)
//             return Mathf.InverseLerp(1f, 1f - fadeOutFraction, progress);

//         return 1f;
//     }
// }




using UnityEngine;
using System.Collections.Generic;

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
    [Tooltip("Radius of the indicator spheres.")]
    public float indicatorRadius = 0.05f;

    [Header("Debug")]
    public bool showAttackIndicator = true;

    // Current state
    private WeaponState currentState;

    // Indicator spheres
    private List<GameObject> indicatorSpheres = new List<GameObject>();

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
    /// Clears any existing indicator spheres. Called by WeaponAttackState on enter.
    /// </summary>
    public void CreateIndicator()
    {
        DestroyIndicator();
    }

    /// <summary>
    /// Updates the weapon transform and spawns indicator spheres at the current
    /// hilt and tip positions. Called each frame by WeaponAttackState.
    /// </summary>
    public void UpdateAttackTransforms(Vector3 hiltWorld, Vector3 tipWorld, float progress)
    {
        if (WeaponTransform != null)
        {
            WeaponMarkers markers = WeaponTransform.GetComponent<WeaponMarkers>();
            Vector3 bladeDirection = (tipWorld - hiltWorld).normalized;

            if (bladeDirection != Vector3.zero)
            {
                Quaternion bladeRotation = Quaternion.LookRotation(bladeDirection);

                SplineAttackPath splinePath = GetComponentInParent<PlayerBasicAttack>()
                    ?.attackPath as SplineAttackPath;
                if (splinePath != null)
                {
                    float rollAngle = splinePath.GetRollAngle(progress);
                    Quaternion rollRotation = Quaternion.AngleAxis(rollAngle, bladeDirection);
                    bladeRotation = rollRotation * bladeRotation;
                }

                WeaponTransform.rotation = bladeRotation;
            }

            if (markers != null && markers.hiltMarker != null)
            {
                Vector3 markerOffset = WeaponTransform.position - markers.hiltMarker.position;
                WeaponTransform.position = hiltWorld + markerOffset;
            }
            else
            {
                WeaponTransform.position = hiltWorld;
            }
        }

        if (showAttackIndicator)
            SpawnIndicatorSpheres(hiltWorld, tipWorld);
    }

    /// <summary>
    /// Spawns a red sphere at the hilt and a green sphere at the tip
    /// to show every sampled hit point along the arc.
    /// </summary>
    void SpawnIndicatorSpheres(Vector3 hiltWorld, Vector3 tipWorld)
    {
        // Hilt sphere — red
        GameObject hiltSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hiltSphere.name = "HiltPoint";
        hiltSphere.transform.position = hiltWorld;
        hiltSphere.transform.localScale = Vector3.one * indicatorRadius;
        Destroy(hiltSphere.GetComponent<Collider>());
        hiltSphere.GetComponent<Renderer>().material.color = Color.red;
        indicatorSpheres.Add(hiltSphere);

        // Tip sphere — green
        GameObject tipSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tipSphere.name = "TipPoint";
        tipSphere.transform.position = tipWorld;
        tipSphere.transform.localScale = Vector3.one * indicatorRadius;
        Destroy(tipSphere.GetComponent<Collider>());
        tipSphere.GetComponent<Renderer>().material.color = Color.green;
        indicatorSpheres.Add(tipSphere);
    }

    /// <summary>
    /// Destroys all indicator spheres. Called by WeaponAttackState on exit.
    /// </summary>
    public void DestroyIndicator()
    {
        foreach (GameObject sphere in indicatorSpheres)
            if (sphere != null) Destroy(sphere);

        indicatorSpheres.Clear();
    }
}