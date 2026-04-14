using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum DogState { Idle, Jumping, NeedsCalming, Calming }

public class DogController : MonoBehaviour
{
    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite         dirtySprite;
    public Sprite         cleanSprite;

    [Header("Bounds — set to match your camera's visible world area")]
    public Vector2 screenMin = new(-4f, -3f);
    public Vector2 screenMax = new( 4f,  3f);

    [Header("Jump Settings (Puppy only)")]
    public float jumpIntervalMin = 4f;
    public float jumpIntervalMax = 8f;
    public float jumpDuration    = 0.8f;
    [Range(0f, 1f)]
    [Tooltip("Fraction of covered foam zones to remove after each jump.")]
    public float foamDecayOnJump = 0.3f;

    [Header("Calm Cooldown (Puppy only)")]
    public float calmCooldownMin = 2f;
    public float calmCooldownMax = 5f;

    [Header("Calming")]
    [Tooltip("World-space radius around the center position where dragging the dog triggers calming.")]
    public float calmRadius = 0.8f;

    public DogState CurrentState { get; private set; }

    private BathDifficulty _difficulty;
    private Vector3        _centerPosition;
    private Camera         _cam;
    private Plane          _dragPlane;
    private bool           _isDragging;
    private Vector3        _dragOffset;
    private Coroutine      _jumpCycleRoutine;

    // ── Initialise ────────────────────────────────────────────────────────────

    public void Initialise(BathDifficulty difficulty)
    {
        _difficulty     = difficulty;
        _cam            = Camera.main;
        _centerPosition = transform.position;
        _dragPlane      = new Plane(Vector3.forward, transform.position);

        spriteRenderer.sprite = dirtySprite;
        SetState(DogState.Idle);

        if (_difficulty == BathDifficulty.Puppy)
            StartJumpCycle();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (CurrentState == DogState.NeedsCalming)
            HandleCalmDragging();
    }

    // ── Calm dragging ─────────────────────────────────────────────────────────

    private void HandleCalmDragging()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = _cam.ScreenPointToRay(mouse.position.ReadValue());
            if (_dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hit = ray.GetPoint(enter);
                // Only start drag if the click lands on the dog (within 1 unit)
                if (Vector2.Distance(hit, transform.position) < 1f)
                {
                    _isDragging = true;
                    _dragOffset = transform.position - hit;
                }
            }
        }

        if (mouse.leftButton.isPressed && _isDragging)
        {
            Ray ray = _cam.ScreenPointToRay(mouse.position.ReadValue());
            if (_dragPlane.Raycast(ray, out float enter))
            {
                Vector3 pos   = ray.GetPoint(enter) + _dragOffset;
                pos.x         = Mathf.Clamp(pos.x, screenMin.x, screenMax.x);
                pos.y         = Mathf.Clamp(pos.y, screenMin.y, screenMax.y);
                pos.z         = transform.position.z;
                transform.position = pos;

                // Trigger calming when close enough to center
                float dist = Vector2.Distance(transform.position, _centerPosition);
                if (dist < calmRadius)
                    StartCoroutine(CalmRoutine());
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame)
            _isDragging = false;
    }

    // ── Jump cycle ────────────────────────────────────────────────────────────

    private void StartJumpCycle()
    {
        if (_jumpCycleRoutine != null) StopCoroutine(_jumpCycleRoutine);
        _jumpCycleRoutine = StartCoroutine(JumpCycleRoutine());
    }

    private IEnumerator JumpCycleRoutine()
    {
        while (true)
        {
            float wait = UnityEngine.Random.Range(jumpIntervalMin, jumpIntervalMax);
            yield return new WaitForSeconds(wait);

            // Only jump during the soaping phase
            if (BathMinigame.Instance.CurrentState != BathState.Soaping)
                yield break;

            yield return StartCoroutine(JumpRoutine());
        }
    }

    private IEnumerator JumpRoutine()
    {
        SetState(DogState.Jumping);

        Vector3 start = transform.position;
        Vector3 end   = new Vector3(
            UnityEngine.Random.Range(screenMin.x, screenMax.x),
            UnityEngine.Random.Range(screenMin.y, screenMax.y),
            transform.position.z);

        // Arc control point sits above the midpoint for a natural curve
        Vector3 mid = (start + end) * 0.5f +
                       Vector3.up * UnityEngine.Random.Range(1f, 2.5f);

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed           += Time.deltaTime;
            transform.position = QuadraticBezier(start, mid, end, elapsed / jumpDuration);
            yield return null;
        }
        transform.position = end;

        // Remove a fraction of foam after landing
        FoamSystem.Instance.DecayFoam(foamDecayOnJump);

        SetState(DogState.NeedsCalming);
    }

    private IEnumerator CalmRoutine()
    {
        if (CurrentState == DogState.Calming) yield break;
        SetState(DogState.Calming);
        _isDragging = false;

        // Snap the dog smoothly back to dead center
        float   elapsed = 0f;
        Vector3 start   = transform.position;
        while (elapsed < 0.3f)
        {
            elapsed           += Time.deltaTime;
            transform.position = Vector3.Lerp(start, _centerPosition, elapsed / 0.3f);
            yield return null;
        }
        transform.position = _centerPosition;
        SetState(DogState.Idle);

        // Wait a random cooldown before the next jump
        float cooldown = UnityEngine.Random.Range(calmCooldownMin, calmCooldownMax);
        yield return new WaitForSeconds(cooldown);

        StartJumpCycle();
    }

    // ── Public helpers ────────────────────────────────────────────────────────

    public void SetClean()
    {
        if (cleanSprite) spriteRenderer.sprite = cleanSprite;
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    private static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

    private void SetState(DogState state) => CurrentState = state;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_centerPosition, calmRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3((screenMin.x + screenMax.x) * 0.5f,
                        (screenMin.y + screenMax.y) * 0.5f, 0f),
            new Vector3(screenMax.x - screenMin.x,
                        screenMax.y - screenMin.y, 0f));
    }
}
