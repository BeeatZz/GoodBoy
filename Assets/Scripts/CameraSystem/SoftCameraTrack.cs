using UnityEngine;

public class SoftCameraTrack : MonoBehaviour
{
    public Transform player;

    public float maxYawOffset = 12f;     
    public float rotationSpeed = 2f;

    public float leftBoundary = 0.25f;    
    public float rightBoundary = 0.75f;  

    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void LateUpdate()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(player.position);

        float targetYaw = 0;

        if (viewportPos.x < leftBoundary)
        {
            float amount = (leftBoundary - viewportPos.x) / leftBoundary;
            targetYaw = -amount * maxYawOffset;
        }

        else if (viewportPos.x > rightBoundary)
        {
            float amount = (viewportPos.x - rightBoundary) / (1f - rightBoundary);
            targetYaw = amount * maxYawOffset;
        }

        Quaternion targetRotation =
            originalRotation * Quaternion.Euler(0, targetYaw, 0);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}