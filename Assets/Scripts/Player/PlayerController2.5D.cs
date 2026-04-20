using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -2f;
    public bool allowDepthMovement = false;
    public InputAction moveAction;
    public InputAction jumpAction;
    public SpriteRenderer spriteRenderer;

    private CharacterController _cc;
    private FixedCamera _activeCamera;
    private float _verticalVelocity;
    private Vector3 _screenRight;
    private Vector3 _screenDepth;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _screenRight = Vector3.right;
        _screenDepth = Vector3.forward;
        Application.targetFrameRate = 60;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        jumpAction.performed += OnJump;
        CameraManager.OnCameraChanged += OnCameraChanged;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        jumpAction.performed -= OnJump;
        CameraManager.OnCameraChanged -= OnCameraChanged;
    }

    public void OnCameraChanged(FixedCamera cam)
    {
        _activeCamera = cam;
        CacheScreenAxes();
        if (_cc != null) UpdateSpriteFlip(_cc.velocity);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (_cc.isGrounded) _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        float h = input.x;
        float v = input.y;

        Vector3 horizontalMove = _screenRight * (h * moveSpeed);
        if (allowDepthMovement) horizontalMove += _screenDepth * (v * moveSpeed);

        _cc.Move(horizontalMove * Time.deltaTime);

        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -0.002f;

        _verticalVelocity += gravity * Time.deltaTime;

        _cc.Move(new Vector3(0f, _verticalVelocity * Time.deltaTime, 0f));

        if (Mathf.Abs(h) > 0.05f) UpdateSpriteFlip(horizontalMove);
    }

    private void CacheScreenAxes()
    {
        if (_activeCamera == null) return;
        _screenRight = Vector3.ProjectOnPlane(_activeCamera.transform.right, Vector3.up).normalized;
        _screenDepth = Vector3.ProjectOnPlane(_activeCamera.transform.forward, Vector3.up).normalized;
    }

    private void UpdateSpriteFlip(Vector3 worldVelocity)
    {
        if (spriteRenderer == null) return;
        float screenDot = Vector3.Dot(worldVelocity, _screenRight);
        if (Mathf.Abs(screenDot) > 0.01f) spriteRenderer.flipX = screenDot < 0f;
    }
}