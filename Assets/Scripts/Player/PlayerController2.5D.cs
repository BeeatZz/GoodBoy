using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -20f;

    [Header("2.5D Settings")]
    public bool allowDepthMovement = false;

    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction jumpAction;

    [Header("References")]
    public SpriteRenderer spriteRenderer;

    private CharacterController _cc;
    private FixedCamera _activeCamera;
    private Vector3 _velocity;

    private Vector3 _screenRight;
    private Vector3 _screenDepth;

    // ── Lifecycle ─────────────────────────────────────────────

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();

        // Auto-assign if missing (prevents common errors)
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _screenRight = Vector3.right;
        _screenDepth = Vector3.forward;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        jumpAction.performed += OnJump;

        // Subscribe to camera event
        CameraManager.OnCameraChanged += OnCameraChanged;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        jumpAction.performed -= OnJump;

        CameraManager.OnCameraChanged -= OnCameraChanged;
    }

    // ── Camera change ─────────────────────────────────────────

    public void OnCameraChanged(FixedCamera cam)
    {
        _activeCamera = cam;
        CacheScreenAxes();

        if (_cc != null)
            UpdateSpriteFlip(_cc.velocity);
    }

    // ── Input ────────────────────────────────────────────────

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (_cc.isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    // ── Update ───────────────────────────────────────────────

    private void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        float h = input.x;
        float v = input.y;

        Vector3 move = _screenRight * (h * moveSpeed);

        if (allowDepthMovement)
            move += _screenDepth * (v * moveSpeed);

        if (_cc.isGrounded)
            _velocity.y = Mathf.Max(_velocity.y, -0.5f);

        _velocity.y += gravity * Time.deltaTime;
        move.y = _velocity.y;

        _cc.Move(move * Time.deltaTime);

        if (Mathf.Abs(h) > 0.05f)
            UpdateSpriteFlip(move);
    }

    // ── Helpers ──────────────────────────────────────────────

    private void CacheScreenAxes()
    {
        if (_activeCamera == null) return;

        _screenRight = Vector3.ProjectOnPlane(
            _activeCamera.transform.right, Vector3.up).normalized;

        _screenDepth = Vector3.ProjectOnPlane(
            _activeCamera.transform.forward, Vector3.up).normalized;
    }

    private void UpdateSpriteFlip(Vector3 worldVelocity)
    {
        if (spriteRenderer == null) return;

        float screenDot = Vector3.Dot(worldVelocity, _screenRight);

        if (Mathf.Abs(screenDot) > 0.01f)
            spriteRenderer.flipX = screenDot < 0f;
    }
}