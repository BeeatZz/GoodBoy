using System.Collections;
using UnityEngine;


public class FoamCoverageZone : MonoBehaviour
{
    public float scrubRate = 0.4f;
    public float drainRate = 0.05f;
    public float maxScale = 1f;

    public float Progress { get; private set; }
    public bool IsCovered => Progress >= 1f;

    private GameObject _foamSpot;
    private bool _spongeOver;


    public void SetSpongeOver(bool over) => _spongeOver = over;

    public void Initialise(GameObject prefab, Transform container)
    {
        if (_foamSpot != null) return;
        _foamSpot = Instantiate(prefab, transform.position, Quaternion.identity, container);
        _foamSpot.transform.localScale = Vector3.zero;
    }


    public void Uncover(float amount = 1f)
    {
        Progress = Mathf.Max(0f, Progress - amount);
        UpdateVisual();
    }


    private void Update()
    {
        if (_spongeOver)
            Progress = Mathf.Min(1f, Progress + scrubRate * Time.deltaTime);
        else if (drainRate > 0f)
            Progress = Mathf.Max(0f, Progress - drainRate * Time.deltaTime);

        UpdateVisual();
    }


    private void UpdateVisual()
    {
        if (_foamSpot == null) return;
        float scale = Mathf.Lerp(0f, maxScale, Progress);
        _foamSpot.transform.localScale = Vector3.one * scale;
    }


    public void DestroyFoam()
    {
        Progress = 0f;
        if (_foamSpot != null)
            StartCoroutine(ShrinkAndDestroy(_foamSpot));
        _foamSpot = null;
    }

    private IEnumerator ShrinkAndDestroy(GameObject spot)
    {
        float elapsed = 0f;
        float duration = 0.3f;
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


    private void OnDrawGizmos()
    {
        Gizmos.color = IsCovered
            ? Color.blue
            : Color.Lerp(new Color(1f, 1f, 1f, 0.3f), Color.blue, Progress);
        Gizmos.DrawSphere(transform.position, 0.08f);
    }
}