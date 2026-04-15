using UnityEngine;

public class QuiverEffect : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How far the object moves from its center.")]
    public float intensity = 0.1f;

    [Tooltip("How fast the quiver vibrates.")]
    public float speed = 50f;

    [Tooltip("Should it shake constantly?")]
    public bool isQuivering = true;

    private Vector3 _originalPosition;
    private Vector3 _targetPosition;

    void Start()
    {
        // Store the starting position so it doesn't drift away
        _originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (isQuivering)
        {
            DoQuiver();
        }
        else
        {
            // Smoothly return to center when stopped
            transform.localPosition = Vector3.Lerp(transform.localPosition, _originalPosition, Time.deltaTime * 5f);
        }
    }

    void DoQuiver()
    {
        // Generate a random point within a sphere multiplied by intensity
        _targetPosition = _originalPosition + Random.insideUnitSphere * intensity;

        // Move the object toward that random point based on speed
        transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPosition, Time.deltaTime * speed);
    }
}