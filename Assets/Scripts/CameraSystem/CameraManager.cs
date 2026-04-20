using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public static event Action<FixedCamera> OnCameraChanged;

    public Camera mainCamera;
    public PlayerController playerController;
    public FixedCamera fallbackCamera;
    public float dwellTime = 0.15f;

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
    private CameraZone _currentZone;
    private CameraZone _pendingZone;
    private Coroutine _pendingRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
        if (!_activeZones.Contains(zone))
            _activeZones.Add(zone);

        if (zone == _currentZone) return;

        bool isHigherPriority = _currentZone == null || zone.priority > _currentZone.priority;

        if (isHigherPriority || dwellTime <= 0f)
        {
            CancelPending();
            CommitZoneSwitch();
        }
        else
        {
            SchedulePending(zone);
        }
    }

    public void DeactivateZone(CameraZone zone)
    {
        _activeZones.Remove(zone);

        if (zone == _pendingZone)
        {
            CancelPending();
            return;
        }

        if (_followMode || _liveTracking) return;
        if (zone != _currentZone) return;

        _currentZone = null;
        StopSubtleTracking();
        CommitZoneSwitch();
    }

    public void ClearActiveZones()
    {
        CancelPending();
        _activeZones.Clear();
        _currentZone = null;
    }

    private void SchedulePending(CameraZone zone)
    {
        if (zone == _pendingZone) return;

        CancelPending();
        _pendingZone = zone;
        _pendingRoutine = StartCoroutine(PendingRoutine(zone));
    }

    private IEnumerator PendingRoutine(CameraZone zone)
    {
        yield return new WaitForSeconds(dwellTime);

        if (_activeZones.Contains(zone))
        {
            _pendingZone = null;
            _pendingRoutine = null;
            CommitZoneSwitch();
        }
    }

    private void CancelPending()
    {
        if (_pendingRoutine != null)
        {
            StopCoroutine(_pendingRoutine);
            _pendingRoutine = null;
        }
        _pendingZone = null;
    }

    private void CommitZoneSwitch()
    {
        FixedCamera target;

        if (_activeZones.Count == 0)
        {
            if (fallbackCamera == null) return;
            target = fallbackCamera;
            _currentZone = null;
        }
        else
        {
            _activeZones.Sort((a, b) => b.priority.CompareTo(a.priority));
            _currentZone = _activeZones[0];
            target = _currentZone.targetCamera;
        }

        BeginBlend(target);
        NotifyCameraChanged(target);
    }

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
        CommitZoneSwitch();
    }

    public bool IsFollowing => _followMode;
    public bool IsLiveTracking => _liveTracking;

    public void StartSubtleTracking(float smoothing, float deadzone)
    {
        _isSubtleTracking = true;
        _trackSmoothing = smoothing;
        _trackDeadzone = deadzone;
    }

    public void StopSubtleTracking() => _isSubtleTracking = false;

    private void BeginBlend(FixedCamera target)
    {
        _liveTracking = false;
        _liveTarget = null;

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

    private void LateUpdate()
    {
        if (_liveTracking)
        {
            if (_liveTarget != null)
                mainCamera.transform.SetPositionAndRotation(_liveTarget.position, _liveTarget.rotation);
            return;
        }

        if (_followMode)
        {
            UpdateFollow();
            return;
        }

        if (_blending)
            UpdateBlend();

        if (_isSubtleTracking && (!_blending || _blendT > 0.95f))
            UpdateSubtleTracking();
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

        mainCamera.transform.position = Vector3.Lerp(_fromSnapshot.position, _toSnapshot.position, t);

        if (!_isSubtleTracking || _blendT < 0.5f)
        {
            mainCamera.transform.rotation = Quaternion.Slerp(_fromSnapshot.rotation, _toSnapshot.rotation, t);
        }

        mainCamera.fieldOfView = Mathf.Lerp(_fromSnapshot.fieldOfView, _toSnapshot.fieldOfView, t);
    }

    private void UpdateSubtleTracking()
    {
        if (playerController == null) return;

        float heightOffset = playerController.transform.localScale.y * 0.5f;
        Vector3 targetPos = playerController.transform.position + Vector3.up * heightOffset;
        Vector3 dir = targetPos - mainCamera.transform.position;

        if (dir.sqrMagnitude < 0.000001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);

        if (_trackDeadzone <= 0 || Quaternion.Angle(mainCamera.transform.rotation, targetRotation) > _trackDeadzone)
        {
            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation,
                targetRotation,
                _trackSmoothing * Time.deltaTime);
        }
    }

    private void UpdateFollow()
    {
        if (_followTarget == null) return;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            _followTarget.DesiredPosition,
            _followTarget.followSmoothing * Time.deltaTime);

        Vector3 dir = _followTarget.LookAtPoint - mainCamera.transform.position;
        if (dir.sqrMagnitude > 0.00001f)
        {
            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation,
                Quaternion.LookRotation(dir),
                _followTarget.rotationSmoothing * Time.deltaTime);
        }

        mainCamera.fieldOfView = Mathf.Lerp(
            mainCamera.fieldOfView,
            _followTarget.fieldOfView,
            _followTarget.fovSmoothing * Time.deltaTime);
    }

}