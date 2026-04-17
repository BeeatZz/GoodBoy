using UnityEngine;
using UnityEngine.InputSystem;


public abstract class BathTool : MonoBehaviour
{
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


    private void TrackMouse()
    {
        var mouse = Mouse.current;
        if (mouse == null || _cam == null) return;

        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, toolDepth));
        Ray   ray   = _cam.ScreenPointToRay(mouse.position.ReadValue());

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 pos = ray.GetPoint(enter);
            pos.z = toolDepth;
            transform.position = pos;
        }
    }


    protected abstract void OnHeld();
}
