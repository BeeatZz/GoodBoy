using UnityEngine;

public class FixedCamera : MonoBehaviour
{
    public float blendDuration = 0.4f;
    public AnimationCurve blendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public CameraSnapshot GetSnapshot() => new CameraSnapshot
    {
        position   = transform.position,
        rotation   = transform.rotation,
        fieldOfView = Camera.main ? Camera.main.fieldOfView : 60f
    };
}

[System.Serializable]
public struct CameraSnapshot
{
    public Vector3    position;
    public Quaternion rotation;
    public float      fieldOfView;
}
