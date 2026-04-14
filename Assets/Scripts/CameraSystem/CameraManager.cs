using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    // Any object in the scene can subscribe to know when the active camera changes
    public static event Action<FixedCamera> OnCameraChanged;

    [Header("References")]
    public Camera mainCamera;
    public PlayerController playerController;

    [Header("Fallback")]
    [Tooltip("Active when the player is outside all zones. " +
             "Create a FixedCamera in your scene and assign it here.")]
    public FixedCamera fallbackCamera;

    private readonly List<CameraZone> _activeZones = new();
    private CameraSnapshot _fromSnapshot;
    private CameraSnapshot _toSnapshot;
    private float _blendT;
    private float _blendDuration;
    private AnimationCurve _blendCurve;
    private bool _blending;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Snap to fallback in Awake so it's ready before any CameraZone.Start() fires
        if (fallbackCamera != null)
        {
            var snap = fallbackCamera.GetSnapshot();
            mainCamera.transform.SetPositionAndRotation(snap.position, snap.rotation);
            mainCamera.fieldOfView = snap.fieldOfView;
            NotifyCameraChanged(fallbackCamera);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void ActivateZone(CameraZone zone)
    {
        if (!_activeZones.Contains(zone))
            _activeZones.Add(zone);

        ApplyHighestPriority();
    }

    public void DeactivateZone(CameraZone zone)
    {
        _activeZones.Remove(zone);
        ApplyHighestPriority();
    }

    // ── Internal ──────────────────────────────────────────────────────────────

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
}