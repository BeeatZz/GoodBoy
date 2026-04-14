using System.Collections;
using UnityEngine;

/// <summary>
/// Place these as child GameObjects distributed across the dog's body sprite,
/// excluding the head area. FoamSystem auto-collects them from children.
/// </summary>
public class FoamCoverageZone : MonoBehaviour
{
    [Header("Foam Growth")]
    public float growDuration = 1.2f;
    [Tooltip("Maximum world-space scale of the foam spot visual.")]
    public float maxScale     = 1f;

    public bool IsCovered { get; private set; }

    private GameObject _foamSpot;
    private Coroutine  _activeRoutine;

    // ── Cover ─────────────────────────────────────────────────────────────────

    public void Cover(GameObject prefab, Transform container)
    {
        if (IsCovered) return;
        IsCovered = true;

        _foamSpot = Instantiate(prefab, transform.position, Quaternion.identity, container);
        _foamSpot.transform.localScale = Vector3.zero;

        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = StartCoroutine(GrowRoutine());
    }

    // ── Uncover ───────────────────────────────────────────────────────────────

    public void Uncover()
    {
        if (!IsCovered) return;
        IsCovered = false;

        if (_activeRoutine != null) StopCoroutine(_activeRoutine);

        if (_foamSpot != null)
        {
            StartCoroutine(ShrinkAndDestroy(_foamSpot));
            _foamSpot = null;
        }
    }

    // ── Routines ──────────────────────────────────────────────────────────────

    private IEnumerator GrowRoutine()
    {
        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / growDuration);
            if (_foamSpot) _foamSpot.transform.localScale = Vector3.one * Mathf.Lerp(0f, maxScale, t);
            yield return null;
        }
        if (_foamSpot) _foamSpot.transform.localScale = Vector3.one * maxScale;
    }

    private IEnumerator ShrinkAndDestroy(GameObject spot)
    {
        float   elapsed    = 0f;
        float   duration   = 0.3f;
        Vector3 startScale = spot.transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (spot) spot.transform.localScale =
                Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            yield return null;
        }
        if (spot) Destroy(spot);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        Gizmos.color = IsCovered ? Color.blue : new Color(1f, 1f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, 0.08f);
    }
}
