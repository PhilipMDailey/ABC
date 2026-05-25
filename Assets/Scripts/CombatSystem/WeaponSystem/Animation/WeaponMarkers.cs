using UnityEngine;

/// <summary>
/// Defines the hilt and tip positions of a weapon mesh via child marker GameObjects.
/// Add this to the weapon prefab and position the markers to match the mesh geometry.
/// </summary>
public class WeaponMarkers : MonoBehaviour
{
    [Tooltip("Marker positioned at the hilt (handle end) of the weapon.")]
    public Transform hiltMarker;
    [Tooltip("Marker positioned at the tip (blade end) of the weapon.")]
    public Transform tipMarker;

    private void OnDrawGizmos()
    {
        if (hiltMarker != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hiltMarker.position, 0.05f);
            Gizmos.DrawIcon(hiltMarker.position, "sv_label_0", true);
        }

        if (tipMarker != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(tipMarker.position, 0.05f);
            Gizmos.DrawIcon(tipMarker.position, "sv_label_3", true);
        }

        if (hiltMarker != null && tipMarker != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hiltMarker.position, tipMarker.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hiltMarker != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(hiltMarker.position, 0.05f);
        }

        if (tipMarker != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(tipMarker.position, 0.05f);
        }
    }
}