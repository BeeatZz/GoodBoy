using System.Collections.Generic;
using UnityEngine;

public class FoamSystem : MonoBehaviour
{
    public static FoamSystem Instance { get; private set; }

    public FoamCoverageZone[] coverageZones;

    public GameObject foamSpotPrefab;
    public Transform foamContainer;


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


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (coverageZones == null || coverageZones.Length == 0)
            coverageZones = GetComponentsInChildren<FoamCoverageZone>();

        foreach (var zone in coverageZones)
            zone.Initialise(foamSpotPrefab, foamContainer);
    }



    public void UpdateSpongePosition(Vector3 worldPos, float radius)
    {
        foreach (var zone in coverageZones)
        {
            bool over = Vector3.Distance(zone.transform.position, worldPos) <= radius;
            zone.SetSpongeOver(over);
        }
    }


    public void ClearSpongeContact()
    {
        foreach (var zone in coverageZones)
            zone.SetSpongeOver(false);
    }


    public void RinseAtPosition(Vector3 worldPos, float radius)
    {
        foreach (var zone in coverageZones)
        {
            if (!zone.IsCovered) continue;
            if (Vector3.Distance(zone.transform.position, worldPos) <= radius)
                zone.DestroyFoam();
        }
    }


    public void DecayFoam(float decayPercent)
    {
        var covered = new List<FoamCoverageZone>();
        foreach (var zone in coverageZones)
            if (zone.IsCovered) covered.Add(zone);

        for (int i = covered.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (covered[i], covered[j]) = (covered[j], covered[i]);
        }


        int toDecay = Mathf.RoundToInt(covered.Count * decayPercent);
        for (int i = 0; i < toDecay; i++)
            covered[i].Uncover(0.6f);
    }

    public bool AllFoamCleared()
    {
        foreach (var zone in coverageZones)
            if (zone.IsCovered) return false;
        return true;
    }
}