using UnityEngine;

/// <summary>
/// Abstract base class defining an attack's geometric path.
/// Subclasses implement different path types (arc, thrust, spin etc.)
/// Both PlayerBasicAttack and WeaponAnimator reference this to stay in sync.
/// </summary>
public abstract class AttackPath : MonoBehaviour
{
    [Header("Blade Settings")]
    [Tooltip("Offset from the attacker's position to the hilt (base) of the weapon blade.")]
    public Vector3 hiltOffset = new Vector3(0f, 0f, 0.5f);

    [Tooltip("Offset from the attacker's position to the tip of the weapon blade.")]
    public Vector3 tipOffset = new Vector3(0f, 0f, 1.5f);

    [Header("Easing")]
    [Tooltip("How much the attack eases in at the start. 0 = no ease, 1 = full ease.")]
    [Range(0f, 1f)]
    public float easeIn = 0.3f;

    [Tooltip("How much the attack eases out at the end. 0 = no ease, 1 = full ease.")]
    [Range(0f, 1f)]
    public float easeOut = 0.3f;

    /// <summary>
    /// Returns the world-space hilt position at a given progress (0-1) along the path.
    /// </summary>
    public abstract Vector3 GetHiltPosition(float progress, Transform attacker);

    /// <summary>
    /// Returns the world-space tip position at a given progress (0-1) along the path.
    /// </summary>
    public abstract Vector3 GetTipPosition(float progress, Transform attacker);

    /// <summary>
    /// Returns the total duration of the path in seconds given a speed value.
    /// </summary>
    public abstract float GetDuration(float speed);

    /// <summary>
    /// Calculates eased progress value from raw progress.
    /// </summary>
    public float GetEasedSpeed(float progress, float speed)
    {
        float speedMultiplier = 1f;

        if (progress < easeIn && easeIn > 0f)
            speedMultiplier = Mathf.Lerp(0.1f, 1f, progress / easeIn);
        else if (progress > 1f - easeOut && easeOut > 0f)
            speedMultiplier = Mathf.Lerp(0.1f, 1f, (1f - progress) / easeOut);

        return speed * speedMultiplier;
    }

    /// <summary>
    /// Draws the path in the Scene view for tuning.
    /// </summary>
    public abstract void DrawGizmos();

    void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }
}