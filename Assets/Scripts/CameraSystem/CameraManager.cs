using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public static event Action<FixedCamera> OnCameraChanged;

    [Header("References")]
    public Camera mainCamera;
    public PlayerController playerController;

    [Header("Fallback")]
    public FixedCamera fallbackCamera;

    private readonly List<CameraZone> _activeZones = new();

    private CameraSnapshot _fromSnapshot;
    private CameraSnapshot _toSnapshot;
    private float _blendT;
    private float _blendDuration;
    private AnimationCurve _blendCurve;

    private bool _blending;
    private bool _followMode;
    private bool _liveTracking;

    private bool _isSubtleTracking;
    private float _trackSmoothing;
    private float _trackDeadzone;

    private FollowTarget _followTarget;
    private Transform _liveTarget;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (fallbackCamera != null)
        {
            var snap = fallbackCamera.GetSnapshot();
            mainCamera.transform.SetPositionAndRotation(snap.position, snap.rotation);
            mainCamera.fieldOfView = snap.fieldOfView;
            NotifyCameraChanged(fallbackCamera);
        }
    }

    public void ActivateZone(CameraZone zone)
    {
        if (_followMode || _liveTracking) return;
        if (!_activeZones.Contains(zone)) _activeZones.Add(zone);
        ApplyHighestPriority();
    }

    public void DeactivateZone(CameraZone zone)
    {
        _activeZones.Remove(zone);
        if (!_followMode && !_liveTracking)
        {
            StopSubtleTracking();
            ApplyHighestPriority();
        }
    }

    private void ApplyHighestPriority()
    {
        FixedCamera target;
        if (_activeZones.Count == 0)
        {
            if (fallbackCamera == null) return;
            target = fallbackCamera;
        }
        else
        {
            _activeZones.Sort((a, b) => b.priority.CompareTo(a.priority));
            target = _activeZones[0].targetCamera;
        }

        BeginBlend(target);
        NotifyCameraChanged(target);
    }

    public void StartSubtleTracking(float smoothing, float deadzone)
    {
        _isSubtleTracking = true;
        _trackSmoothing = smoothing;
        _trackDeadzone = deadzone;
    }

    public void StopSubtleTracking() => _isSubtleTracking = false;

    public void BlendToCamera(FixedCamera target)
    {
        _followMode = false;
        _liveTracking = false;
        BeginBlend(target);
        NotifyCameraChanged(target);
    }

    public void TrackLive(Transform target)
    {
        _liveTarget = target;
        _liveTracking = true;
        _blending = false;
        _followMode = false;
    }

    public void StopLiveTracking()
    {
        _liveTracking = false;
        _liveTarget = null;
    }

    public void EnterFollowMode(FollowTarget target)
    {
        _followTarget = target;
        _followMode = true;
        _blending = false;
        _liveTracking = false;
    }

    public void ExitFollowMode()
    {
        _followMode = false;
        _followTarget = null;
        ApplyHighestPriority();
    }

    private void LateUpdate()
    {
        if (_liveTracking)
        {
            if (_liveTarget != null)
                mainCamera.transform.SetPositionAndRotation(_liveTarget.position, _liveTarget.rotation);
        }
        else if (_followMode)
        {
            UpdateFollow();
        }
        else
        {
            // Update Blending
            if (_blending)
            {
                UpdateBlend();
            }

            // Update Subtle Tracking
            // CRITICAL: We only track if we aren't blending, OR if the blend is basically finished.
            if (_isSubtleTracking && (!_blending || _blendT > 0.95f))
            {
                UpdateSubtleTracking();
            }
        }
    }

    private void UpdateSubtleTracking()
    {
        if (playerController == null) return;

        // At 0.09 scale, we need to be very precise with the target point
        float heightOffset = playerController.transform.localScale.y * 0.5f;
        Vector3 targetPos = playerController.transform.position + (Vector3.up * heightOffset);
        Vector3 dir = targetPos - mainCamera.transform.position;

        // If the camera is too close, don't jitter
        if (dir.sqrMagnitude < 0.000001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);

        // If deadzone is 0, we rotate every frame. 
        // If you still see no movement, increase lookSmoothing to 15+ in the Inspector.
        if (_trackDeadzone <= 0 || Quaternion.Angle(mainCamera.transform.rotation, targetRotation) > _trackDeadzone)
        {
            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation,
                targetRotation,
                _trackSmoothing * Time.deltaTime
            );
        }
    }

    private void UpdateBlend()
    {
        _blendT += Time.deltaTime / Mathf.Max(_blendDuration, 0.001f);

        if (_blendT >= 1f)
        {
            _blendT = 1f;
            _blending = false;
        }

        float t = _blendCurve != null ? _blendCurve.Evaluate(_blendT) : _blendT;

        // Position always lerps to the camera's fixed spot
        mainCamera.transform.position = Vector3.Lerp(_fromSnapshot.position, _toSnapshot.position, t);

        // ROTATION LOGIC:
        // If subtle tracking is enabled, we STOP the blend rotation early. 
        // This lets the tracking logic take over the rotation completely.
        if (!_isSubtleTracking)
        {
            mainCamera.transform.rotation = Quaternion.Slerp(_fromSnapshot.rotation, _toSnapshot.rotation, t);
        }
        else if (_blendT < 0.5f)
        {
            // Only lerp rotation for the first half of the blend if tracking is coming up
            mainCamera.transform.rotation = Quaternion.Slerp(_fromSnapshot.rotation, _toSnapshot.rotation, t);
        }

        mainCamera.fieldOfView = Mathf.Lerp(_fromSnapshot.fieldOfView, _toSnapshot.fieldOfView, t);
    }

    private void UpdateFollow()
    {
        if (_followTarget == null) return;

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, _followTarget.DesiredPosition, _followTarget.followSmoothing * Time.deltaTime);

        Vector3 dir = _followTarget.LookAtPoint - mainCamera.transform.position;
        if (dir.sqrMagnitude > 0.00001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRot, _followTarget.rotationSmoothing * Time.deltaTime);
        }

        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, _followTarget.fieldOfView, _followTarget.fovSmoothing * Time.deltaTime);
    }

    private void BeginBlend(FixedCamera target)
    {
        _fromSnapshot = new CameraSnapshot
        {
            position = mainCamera.transform.position,
            rotation = mainCamera.transform.rotation,
            fieldOfView = mainCamera.fieldOfView
        };
        _toSnapshot = target.GetSnapshot();
        _blendDuration = target.blendDuration;
        _blendCurve = target.blendCurve;
        _blendT = 0f;
        _blending = true;
    }

    private void NotifyCameraChanged(FixedCamera target)
    {
        playerController?.OnCameraChanged(target);
        OnCameraChanged?.Invoke(target);
    }
}