using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Attack path defined by two splines — one for the hilt and one for the tip.
/// Splines are evaluated in local space relative to the attacker,
/// giving full artistic control over the blade's path through space.
/// </summary>
public class SplineAttackPath : AttackPath
{
    [Header("Spline Settings")]
    [Tooltip("Spline defining the path of the hilt (base of the blade) through the attack.")]
    public SplineContainer hiltSpline;
    [Tooltip("Spline defining the path of the tip of the blade through the attack.")]
    public SplineContainer tipSpline;
    [Tooltip("How long the attack takes in seconds.")]
    public float attackDuration = 0.4f;

    [Header("Weapon Markers")]
    [Tooltip("WeaponMarkers component on the equipped weapon prefab. Set at runtime by WeaponSocket.")]
    public WeaponMarkers weaponMarkers;

    public override Vector3 GetHiltPosition(float progress, Transform attacker)
    {
        return SampleSplineWorldPosition(hiltSpline, progress, attacker);
    }

    public override Vector3 GetTipPosition(float progress, Transform attacker)
    {
        return SampleSplineWorldPosition(tipSpline, progress, attacker);
    }

    /// <summary>
    /// Applies the marker's local offset from the weapon pivot to the spline position,
    /// so the hilt/tip tracks along the spline rather than the weapon's origin.
    /// </summary>
    public override float GetDuration(float speed)
    {
        return attackDuration;
    }

    Vector3 SampleSplineWorldPosition(SplineContainer splineContainer, float progress, Transform attacker)
    {
        if (splineContainer == null)
        {
            Debug.LogWarning("SplineAttackPath: Spline is not assigned.");
            return attacker.position;
        }

        return splineContainer.EvaluatePosition(progress);
    }

    public override void DrawGizmos()
    {
        if (hiltSpline == null || tipSpline == null) return;

        int steps = 20;
        for (int i = 0; i <= steps; i++)
        {
            float progress = (float)i / steps;
            Vector3 hiltPos = hiltSpline.EvaluatePosition(progress);
            Vector3 tipPos = tipSpline.EvaluatePosition(progress);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hiltPos, tipPos);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hiltPos, 0.03f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(tipPos, 0.03f);
        }
    }
}