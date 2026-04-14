using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraZone : MonoBehaviour
{
    public FixedCamera targetCamera;

    public int priority = 0;

    private Collider _col;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;
    }

    private void Start()
    {
        Collider[] hits = Physics.OverlapBox(
            _col.bounds.center,
            _col.bounds.extents,
            transform.rotation);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                CameraManager.Instance.ActivateZone(this);
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            CameraManager.Instance.ActivateZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            CameraManager.Instance.DeactivateZone(this);
    }
}