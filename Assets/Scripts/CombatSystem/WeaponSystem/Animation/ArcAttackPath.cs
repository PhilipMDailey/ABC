using UnityEngine;

/// <summary>
/// Attack path that sweeps through a configurable arc.
/// Supports vertical, horizontal, and diagonal arc planes.
/// </summary>
public class ArcAttackPath : AttackPath
{
    [Header("Arc Settings")]
    [Tooltip("Total degrees the weapon travels through during the attack.")]
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

    [Tooltip("How far the weapon orbits from the attacker's center.")]
    public float attackDistance = 1.5f;

    public enum ArcPlane
    {
        Vertical,
        Horizontal,
        Diagonal
    }

    public override Vector3 GetHiltPosition(float progress, Transform attacker)
    {
        return GetPositionAtProgress(progress, hiltOffset, attacker);
    }

    public override Vector3 GetTipPosition(float progress, Transform attacker)
    {
        return GetPositionAtProgress(progress, tipOffset, attacker);
    }

    public override float GetDuration(float speed)
    {
        return attackArc / speed;
    }

    Vector3 GetPositionAtProgress(float progress, Vector3 offset, Transform attacker)
    {
        float degrees = progress * attackArc;
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

        Vector3 bladeOffset = attacker.TransformDirection(offset);
        return attacker.position + attacker.TransformDirection(localOffset) + bladeOffset;
    }

    public override void DrawGizmos()
    {
        if (Application.isPlaying) return;

        int steps = 20;
        for (int i = 0; i <= steps; i++)
        {
            float progress = (float)i / steps;
            Vector3 hiltPos = GetPositionAtProgress(progress, hiltOffset, transform);
            Vector3 tipPos = GetPositionAtProgress(progress, tipOffset, transform);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hiltPos, tipPos);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hiltPos, 0.05f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(tipPos, 0.05f);
        }
    }
}