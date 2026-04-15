using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class CameraZone : MonoBehaviour
{
    public FixedCamera targetCamera;
    public int priority = 0;

    [Header("Locking Mechanism")]
    public bool lockOnEnter = false;
    public List<GameObject> barriers = new List<GameObject>();

    [Header("Subtle Tracking (Unbeatable Style)")]
    public bool trackPlayer = true;
    [Range(0.1f, 20f)] public float lookSmoothing = 5.0f;
    [Tooltip("For tiny scales, keep this near 0 or 1.")]
    public float deadzoneAngle = 1f;

    private Collider _col;
    private bool _hasLocked = false;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"<color=cyan>CameraZone:</color> Player detected in {gameObject.name}");
            HandlePlayerEntry();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraManager.Instance.DeactivateZone(this);
        }
    }

    public void HandlePlayerEntry()
    {
        CameraManager.Instance.ActivateZone(this);

        if (trackPlayer)
            CameraManager.Instance.StartSubtleTracking(lookSmoothing, deadzoneAngle);
        else
            CameraManager.Instance.StopSubtleTracking();

        if (lockOnEnter && !_hasLocked)
        {
            ToggleBarriers(true);
            _hasLocked = true;
        }
    }

    public void ToggleBarriers(bool state)
    {
        foreach (GameObject wall in barriers)
        {
            if (wall != null) wall.SetActive(state);
        }
    }
}