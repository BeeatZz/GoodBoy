using UnityEngine;

public class ShampooSponge : BathTool
{
    [Header("Foam Application")]
    [Tooltip("World-space radius in which foam zones are covered per application tick.")]
    public float foamRadius = 0.4f;

    [Tooltip("Minimum seconds between foam applications while dragging.")]
    public float applicationRate = 0.05f;

    private float _nextApplicationTime;

    protected override void OnHeld()
    {
        if (BathMinigame.Instance.CurrentState != BathState.Soaping) return;
        if (Time.time < _nextApplicationTime) return;

        FoamSystem.Instance.ApplyFoamAtPosition(transform.position, foamRadius);
        _nextApplicationTime = Time.time + applicationRate;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, foamRadius);
    }
}
