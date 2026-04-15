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

    // Fixed-camera blend state
    private CameraSnapshot _fromSnapshot;
    private CameraSnapshot _toSnapshot;
    private float _blendT;
    private float _blendDuration;
    private AnimationCurve _blendCurve;
    private bool _blending;

    // Follow mode state
    private FollowTarget _followTarget;
    private bool _followMode;

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
        if (_followMode) return;
        if (!_activeZones.Contains(zone)) _activeZones.Add(zone);
        ApplyHighestPriority();
    }

    public void DeactivateZone(CameraZone zone)
    {
        _activeZones.Remove(zone);
        if (!_followMode) ApplyHighestPriority();
    }

    public void BlendToCamera(FixedCamera target)
    {
        // If we blend to a fixed camera, we usually want to drop follow mode
        _followMode = false;
        BeginBlend(target);
        NotifyCameraChanged(target);
    }

    public void EnterFollowMode(FollowTarget target)
    {
        Debug.Log($"EnterFollowMode called — target: {(target == null ? "NULL" : target.name)}");
        _followTarget = target;
        _followMode = true;
        _blending = false; // Stop any active blends to jump into follow logic
    }

    public void ExitFollowMode()
    {
        _followMode = false;
        _followTarget = null;
        ApplyHighestPriority();
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

    private void NotifyCameraChanged(FixedCamera target)
    {
        playerController?.OnCameraChanged(target);
        OnCameraChanged?.Invoke(target);
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

    private void LateUpdate()
    {
        if (_followMode)
        {
            UpdateFollow();
            return;
        }

        if (!_blending) return;

        _blendT += Time.deltaTime / Mathf.Max(_blendDuration, 0.001f);
        if (_blendT >= 1f)
        {
            _blendT = 1f;
            _blending = false;
        }

        float t = _blendCurve.Evaluate(_blendT);
        mainCamera.transform.position = Vector3.Lerp(_fromSnapshot.position, _toSnapshot.position, t);
        mainCamera.transform.rotation = Quaternion.Slerp(_fromSnapshot.rotation, _toSnapshot.rotation, t);
        mainCamera.fieldOfView = Mathf.Lerp(_fromSnapshot.fieldOfView, _toSnapshot.fieldOfView, t);
    }

    private void UpdateFollow()
    {
        if (_followTarget == null) return;

        Vector3 desired = _followTarget.DesiredPosition;
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desired, _followTarget.followSmoothing * Time.deltaTime);

        Vector3 dir = _followTarget.LookAtPoint - mainCamera.transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRot, _followTarget.rotationSmoothing * Time.deltaTime);
        }

        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, _followTarget.fieldOfView, _followTarget.fovSmoothing * Time.deltaTime);
    }
}