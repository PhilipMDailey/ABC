using UnityEngine;
using UnityEngine.Splines;

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

    [System.Serializable]
    public struct RollKeyframe
    {
        [Tooltip("Position along the attack arc (0 = start, 1 = end).")]
        [Range(0f, 1f)]
        public float progress;
        [Tooltip("Roll angle of the weapon at this point in degrees.")]
        public float rollAngle;
        [Tooltip("How much to ease into this keyframe's angle (0 = no ease, 1 = full ease).")]
        [Range(0f, 1f)]
        public float easeIn;
        [Tooltip("How much to ease out of this keyframe's angle (0 = no ease, 1 = full ease).")]
        [Range(0f, 1f)]
        public float easeOut;
    }

    /// <summary>
    /// Returns the roll angle in degrees at a given progress along the attack.
    /// </summary>
    // public float GetRollAngle(float progress)
    // {
    //     return rollCurve.Evaluate(progress);
    // }

    [Header("Roll")]
    [Tooltip("Keyframes defining the weapon's roll angle at specific points along the attack arc.")]
    public RollKeyframe[] rollKeyframes = new RollKeyframe[0];

    /// <summary>
    /// Returns the roll angle in degrees at a given progress along the attack,
    /// interpolating between keyframes with per-keyframe easing.
    /// </summary>
    public float GetRollAngle(float progress)
    {
        if (rollKeyframes == null || rollKeyframes.Length == 0)
            return 0f;

        // Sort keyframes by progress just in case they're out of order
        System.Array.Sort(rollKeyframes, (a, b) => a.progress.CompareTo(b.progress));

        // Before first keyframe
        if (progress <= rollKeyframes[0].progress)
            return rollKeyframes[0].rollAngle;

        // After last keyframe
        if (progress >= rollKeyframes[rollKeyframes.Length - 1].progress)
            return rollKeyframes[rollKeyframes.Length - 1].rollAngle;

        // Find the two keyframes we're between
        for (int i = 0; i < rollKeyframes.Length - 1; i++)
        {
            RollKeyframe from = rollKeyframes[i];
            RollKeyframe to = rollKeyframes[i + 1];

            if (progress >= from.progress && progress <= to.progress)
            {
                // Normalize progress between these two keyframes
                float segmentLength = to.progress - from.progress;
                float t = (progress - from.progress) / segmentLength;

                // Apply easing — ease out of the 'from' keyframe
                // and ease into the 'to' keyframe
                t = ApplyEasing(t, from.easeOut, to.easeIn);

                return Mathf.Lerp(from.rollAngle, to.rollAngle, t);
            }
        }

        return 0f;
    }

    /// <summary>
    /// Applies ease out from the start and ease in to the end of a 0-1 range.
    /// </summary>
    float ApplyEasing(float t, float easeOut, float easeIn)
    {
        // Ease out of previous keyframe (slow down leaving it)
        if (t < easeOut && easeOut > 0f)
        {
            float localT = t / easeOut;
            t = easeOut * (1f - Mathf.Cos(localT * Mathf.PI * 0.5f));
        }
        // Ease into next keyframe (slow down approaching it)
        else if (t > 1f - easeIn && easeIn > 0f)
        {
            float localT = (t - (1f - easeIn)) / easeIn;
            t = (1f - easeIn) + easeIn * Mathf.Sin(localT * Mathf.PI * 0.5f);
        }

        return t;
    }

    public override Vector3 GetHiltPosition(float progress, Transform attacker)
    {
        return SampleSplineWorldPosition(hiltSpline, progress, attacker);
    }

    public override Vector3 GetTipPosition(float progress, Transform attacker)
    {
        return SampleSplineWorldPosition(tipSpline, progress, attacker);
    }

    // public override float GetDuration(float speed)
    // {
    //     return attackDuration;
    // }
    public override float GetDuration(float speed)
    {
        // attackDuration is the base duration at speed 1
        // Higher attackSpeed = shorter duration
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