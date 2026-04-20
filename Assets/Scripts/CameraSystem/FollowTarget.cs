using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 positionOffset = new Vector3(0f, 3f, -6f);
    public float followSmoothing = 5f;
    public Transform lookAtOverride;
    public Vector3 lookAtOffset = new Vector3(0f, 1f, 0f);
    public float rotationSmoothing = 6f;
    public float fieldOfView = 60f;
    public float fovSmoothing = 4f;


    public Vector3 DesiredPosition
    {
        get
        {
            if (target == null) return transform.position;
            return target.TransformPoint(positionOffset);
        }
    }

    public Vector3 LookAtPoint
    {
        get
        {
            Transform t = lookAtOverride != null ? lookAtOverride : target;
            if (t == null) return Vector3.zero;
            return t.position + lookAtOffset;
        }
    }
}
