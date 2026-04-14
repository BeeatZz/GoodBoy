using System.Collections.Generic;
using UnityEngine;

public class FoamSystem : MonoBehaviour
{
    public static FoamSystem Instance { get; private set; }

    [Header("Coverage Zones")]
    [Tooltip("Auto-populated from children if left empty. " +
             "Place FoamCoverageZone GameObjects across the dog body, " +
             "but NOT on the head — that is how the head is excluded.")]
    public FoamCoverageZone[] coverageZones;

    [Header("Foam Visuals")]
    [Tooltip("Prefab for a single foam spot — a sphere or sprite scaled up over time.")]
    public GameObject foamSpotPrefab;
    [Tooltip("Parent transform to keep foam spots organised in the hierarchy.")]
    public Transform  foamContainer;

    // ── Coverage ──────────────────────────────────────────────────────────────

    public float CoveragePercent
    {
        get
        {
            if (coverageZones.Length == 0) return 0f;
            int covered = 0;
            foreach (var z in coverageZones)
                if (z.IsCovered) covered++;
            return (float)covered / coverageZones.Length;
        }
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (coverageZones == null || coverageZones.Length == 0)
            coverageZones = GetComponentsInChildren<FoamCoverageZone>();
    }

    // ── Apply foam (sponge) ───────────────────────────────────────────────────

    public void ApplyFoamAtPosition(Vector3 worldPos, float radius)
    {
        foreach (var zone in coverageZones)
        {
            if (zone.IsCovered) continue;
            if (Vector3.Distance(zone.transform.position, worldPos) <= radius)
                zone.Cover(foamSpotPrefab, foamContainer);
        }
    }

    // ── Rinse foam (shower head) ──────────────────────────────────────────────

    public void RinseAtPosition(Vector3 worldPos, float radius)
    {
        foreach (var zone in coverageZones)
        {
            if (!zone.IsCovered) continue;
            if (Vector3.Distance(zone.transform.position, worldPos) <= radius)
                zone.Uncover();
        }
    }

    // ── Decay foam (dog jumps) ────────────────────────────────────────────────

    public void DecayFoam(float decayPercent)
    {
        var covered = new List<FoamCoverageZone>();
        foreach (var zone in coverageZones)
            if (zone.IsCovered) covered.Add(zone);

        // Fisher-Yates shuffle so removal is random
        for (int i = covered.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (covered[i], covered[j]) = (covered[j], covered[i]);
        }

        int toRemove = Mathf.RoundToInt(covered.Count * decayPercent);
        for (int i = 0; i < toRemove; i++)
            covered[i].Uncover();
    }

    public bool AllFoamCleared()
    {
        foreach (var zone in coverageZones)
            if (zone.IsCovered) return false;
        return true;
    }
}
