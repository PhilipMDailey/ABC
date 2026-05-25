using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Visualizes mob AI state, steering vectors, and perception ranges in the Scene view.
/// Attach to any mob that has a MobBase component.
/// All visualization is editor-only and has zero runtime cost in builds.
/// </summary>
[RequireComponent(typeof(MobBase))]
public class MobDebugVisualizer : MonoBehaviour
{
    [Header("Toggle Visualizations")]
    public bool showStateName = true;
    public bool showPerceptionRanges = true;
    public bool showSeparationRadius = true;
    public bool showSteeringVectors = true;
    public bool showHealthBar = true;

    [Header("Steering Vector Colors")]
    public Color chaseColor = Color.red;
    public Color separationColor = Color.blue;
    public Color finalDirectionColor = Color.green;

    // These are set by AttackState each frame so the visualizer can draw them
    [HideInInspector] public Vector3 debugChaseDirection;
    [HideInInspector] public Vector3 debugSeparationForce;
    [HideInInspector] public Vector3 debugFinalDirection;
    [HideInInspector] public bool hasSteeringData = false;

    private MobBase mob;
    private CombatController combat;

    void Awake()
    {
        mob = GetComponent<MobBase>();
        combat = GetComponent<CombatController>();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (mob == null)
            mob = GetComponent<MobBase>();

        if (combat == null)
            combat = GetComponent<CombatController>();

        Vector3 position = transform.position;

        // ─── State Label ──────────────────────────────────────────────────────
        if (showStateName)
        {
            string stateLabel = mob != null ? mob.GetCurrentStateName() : "Unknown";
            Handles.Label(position + Vector3.up * 2.5f, stateLabel, new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.white },
                fontStyle = FontStyle.Bold,
                fontSize = 12
            });
        }

        // ─── Health Bar ───────────────────────────────────────────────────────
        if (showHealthBar && combat != null)
        {
            float healthPercent = combat.CurrentHealth / combat.MaxHealth;
            string healthLabel = $"HP: {combat.CurrentHealth:F0}/{combat.MaxHealth:F0}";

            // Background bar
            Handles.color = Color.red;
            Handles.DrawLine(
                position + Vector3.up * 2.2f + Vector3.left * 0.5f,
                position + Vector3.up * 2.2f + Vector3.right * 0.5f
            );

            // Health bar
            Handles.color = Color.green;
            Handles.DrawLine(
                position + Vector3.up * 2.2f + Vector3.left * 0.5f,
                position + Vector3.up * 2.2f + Vector3.left * 0.5f + Vector3.right * healthPercent
            );

            Handles.Label(position + Vector3.up * 2.0f, healthLabel, new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.green },
                fontSize = 10
            });
        }

        // ─── Perception Ranges ────────────────────────────────────────────────
        if (showPerceptionRanges && mob != null)
        {
            // Detection range
            Handles.color = new Color(1f, 1f, 0f, 0.1f);
            Handles.DrawSolidDisc(position, Vector3.up, mob.detectionRange);
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(position, Vector3.up, mob.detectionRange);

            // Hearing range
            Handles.color = new Color(0f, 1f, 1f, 0.1f);
            Handles.DrawSolidDisc(position, Vector3.up, mob.hearingRange);
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(position, Vector3.up, mob.hearingRange);
        }

        // ─── Separation Radius ────────────────────────────────────────────────
        if (showSeparationRadius && mob != null)
        {
            Handles.color = new Color(0f, 0f, 1f, 0.05f);
            Handles.DrawSolidDisc(position, Vector3.up, mob.separationRadius);
            Handles.color = Color.blue;
            Handles.DrawWireDisc(position, Vector3.up, mob.separationRadius);
        }

        // ─── Steering Vectors ─────────────────────────────────────────────────
        if (showSteeringVectors && hasSteeringData)
        {
            float arrowLength = 2f;

            // Chase direction
            DrawArrow(position, debugChaseDirection * arrowLength, chaseColor, "Chase");

            // Separation force
            DrawArrow(position, debugSeparationForce * arrowLength, separationColor, "Sep");

            // Final blended direction
            DrawArrow(position, debugFinalDirection * arrowLength, finalDirectionColor, "Final");
        }
    }

    void DrawArrow(Vector3 origin, Vector3 direction, Color color, string label)
    {
        if (direction.magnitude < 0.001f) return;

        Handles.color = color;
        Handles.DrawLine(origin, origin + direction);

        // Arrowhead
        Vector3 tip = origin + direction;
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.up) * 0.2f;
        Handles.DrawLine(tip, tip - direction.normalized * 0.3f + perpendicular);
        Handles.DrawLine(tip, tip - direction.normalized * 0.3f - perpendicular);

        // Label at tip
        Handles.Label(tip, label, new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = color },
            fontSize = 10
        });
    }
#endif
}