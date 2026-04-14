using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Optional UI — shows a progress bar for foam coverage during soaping,
/// and a rinsing indicator during the rinse phase.
/// Wire up in the Inspector; nothing breaks if left unassigned.
/// </summary>
public class BathCoverageUI : MonoBehaviour
{
    [Header("Soaping Phase")]
    public Slider coverageBar;
    public GameObject soapingPrompt;

    [Header("Rinsing Phase")]
    public GameObject rinsingPrompt;

    private void OnEnable()  => BathMinigame.OnStateChanged += HandleStateChanged;
    private void OnDisable() => BathMinigame.OnStateChanged -= HandleStateChanged;

    private void Update()
    {
        if (BathMinigame.Instance == null) return;

        if (BathMinigame.Instance.CurrentState == BathState.Soaping && coverageBar)
            coverageBar.value = FoamSystem.Instance.CoveragePercent;
    }

    private void HandleStateChanged(BathState state)
    {
        if (soapingPrompt) soapingPrompt.SetActive(state == BathState.Soaping);
        if (rinsingPrompt) rinsingPrompt.SetActive(state == BathState.Rinsing);
    }
}
