using UnityEngine;

/// <summary>
/// Data component placed on a GameObject in the scene (usually a child of the player
/// or a separate camera rig). CameraManager reads this when in follow mode.
/// </summary>
public class FollowTarget : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;

    [Tooltip("Offset from the target in the target's local space.")]
    public Vector3 positionOffset = new Vector3(0f, 3f, -6f);

    [Tooltip("How quickly the camera position catches up to the target. Higher = snappier.")]
    public float followSmoothing = 5f;

    [Header("Look At")]
    [Tooltip("If set, the camera always looks at this transform instead of the target.")]
    public Transform lookAtOverride;

    [Tooltip("Offset applied to the look-at point in world space.")]
    public Vector3 lookAtOffset = new Vector3(0f, 1f, 0f);

    [Tooltip("How quickly the camera rotation catches up. Higher = snappier.")]
    public float rotationSmoothing = 6f;

    [Header("Field of View")]
    public float fieldOfView = 60f;
    public float fovSmoothing = 4f;

    // ── Helpers used by CameraManager ─────────────────────────────────────────

    public Vector3 DesiredPosition
    {
        get
        {
            if (target == null) return transform.position;
            return target.TransformPoint(positionOffset);
        }
    }

    public Vector3 LookAtPoint
    {
        get
        {
            Transform t = lookAtOverride != null ? lookAtOverride : target;
            if (t == null) return Vector3.zero;
            return t.position + lookAtOffset;
        }
    }
}
