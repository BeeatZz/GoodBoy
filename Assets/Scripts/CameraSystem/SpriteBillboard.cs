using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    public float rotationOffset = 0f;
    public bool snapOnCameraSwitch = true;
    public float smoothSpeed = 720f;
    private Transform _cameraTarget;
    private Quaternion _targetRotation;


    private void OnEnable()
    {
        CameraManager.OnCameraChanged += HandleCameraChanged;
    }

    private void OnDisable()
    {
        CameraManager.OnCameraChanged -= HandleCameraChanged;
    }

    private void LateUpdate()
    {
        if (_cameraTarget == null) return;
        _targetRotation = ComputeFacing(_cameraTarget);

        transform.rotation = snapOnCameraSwitch
            ? _targetRotation
            : Quaternion.RotateTowards(transform.rotation, _targetRotation,
                                       smoothSpeed * Time.deltaTime);
    }


    private void HandleCameraChanged(FixedCamera cam)
    {
        _cameraTarget = cam.transform;

        if (snapOnCameraSwitch)
            transform.rotation = ComputeFacing(_cameraTarget);
    }


    private Quaternion ComputeFacing(Transform cam)
    {
        Vector3 toCamera = cam.position - transform.position;
        toCamera.y = 0f;

        if (toCamera.sqrMagnitude < 0.0001f)
            return transform.rotation;

        Quaternion look = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
        return look * Quaternion.Euler(0f, rotationOffset, 0f);
    }
}
