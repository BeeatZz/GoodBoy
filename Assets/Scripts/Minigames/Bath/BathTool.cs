using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Base class for all bath tools. The tool mesh follows the mouse cursor
/// on a world-space plane at the dog's depth. Subclasses implement
/// what happens when the player holds the mouse button.
/// </summary>
public abstract class BathTool : MonoBehaviour
{
    [Header("Tool Movement")]
    [Tooltip("World-space Z depth the tool moves along — should match the dog's Z position.")]
    public float toolDepth = 0f;

    protected Camera _cam;
    protected bool   _isHeld;

    protected virtual void Awake()
    {
        _cam = Camera.main;
    }

    protected virtual void Update()
    {
        TrackMouse();

        var mouse = Mouse.current;
        if (mouse == null) return;

        _isHeld = mouse.leftButton.isPressed;

        if (_isHeld) OnHeld();
    }

    // ── Mouse tracking ────────────────────────────────────────────────────────

    private void TrackMouse()
    {
        var mouse = Mouse.current;
        if (mouse == null || _cam == null) return;

        // Project mouse onto the flat plane at toolDepth
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, toolDepth));
        Ray   ray   = _cam.ScreenPointToRay(mouse.position.ReadValue());

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 pos = ray.GetPoint(enter);
            pos.z = toolDepth;
            transform.position = pos;
        }
    }

    // ── Overridable behaviour ─────────────────────────────────────────────────

    /// <summary>Called every frame the mouse button is held.</summary>
    protected abstract void OnHeld();
}
