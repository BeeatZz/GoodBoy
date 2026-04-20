using UnityEngine;
using System.Collections.Generic;

public class CameraZone : MonoBehaviour
{
    public FixedCamera targetCamera;
    public int priority = 0;
    public bool lockOnEnter = false;
    public List<GameObject> barriers = new List<GameObject>();
    public bool trackPlayer = true;
    public float lookSmoothing = 5.0f;
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