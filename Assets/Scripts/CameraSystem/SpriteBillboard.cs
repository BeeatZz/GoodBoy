using UnityEngine;

/// <summary>
/// Cylindrical billboard — keeps the sprite facing the active FixedCamera's
/// horizontal look direction (Y-axis constrained, so sprites stay upright).
///
/// Targets the FixedCamera itself rather than the blending Main Camera,
/// so the facing snaps cleanly on a zone switch with no mid-blend wobble.
/// </summary>
public class SpriteBillboard : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Offset added to the base facing rotation. " +
             "Use 180 if your sprite's 'front' faces away from the camera by default.")]
    public float rotationOffset = 0f;

    [Tooltip("If enabled, instantly snaps to face the new camera on a zone switch. " +
             "If disabled, smoothly rotates toward the new facing direction.")]
    public bool snapOnCameraSwitch = true;

    [Tooltip("Rotation speed used when Snap On Camera Switch is disabled.")]
    public float smoothSpeed = 720f;

    // The camera we are currently billboarding toward
    private Transform _cameraTarget;
    private Quaternion _targetRotation;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        CameraManager.OnCameraChanged += HandleCameraChanged;
    }

    private void OnDisable()
    {
        CameraManager.OnCameraChanged -= HandleCameraChanged;
    }

    private void LateUpdate()
    {
        if (_cameraTarget == null) return;

        // Recompute every frame so the billboard stays correct
        // even when the camera is mid-blend or the sprite is moving
        _targetRotation = ComputeFacing(_cameraTarget);

        transform.rotation = snapOnCameraSwitch
            ? _targetRotation
            : Quaternion.RotateTowards(transform.rotation, _targetRotation,
                                       smoothSpeed * Time.deltaTime);
    }

    // ── Camera change ─────────────────────────────────────────────────────────

    private void HandleCameraChanged(FixedCamera cam)
    {
        _cameraTarget = cam.transform;

        // Always snap immediately on a hard camera switch regardless of smooth setting —
        // avoids the sprite visibly spinning to catch up during the camera blend
        if (snapOnCameraSwitch)
            transform.rotation = ComputeFacing(_cameraTarget);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Quaternion ComputeFacing(Transform cam)
    {
        // Direction from this sprite to the camera, flattened to the horizontal plane
        Vector3 toCamera = cam.position - transform.position;
        toCamera.y = 0f;

        if (toCamera.sqrMagnitude < 0.0001f)
            return transform.rotation;

        // Look toward the camera, then apply any authoring offset
        Quaternion look = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
        return look * Quaternion.Euler(0f, rotationOffset, 0f);
    }
}
