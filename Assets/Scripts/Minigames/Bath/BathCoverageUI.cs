using UnityEngine;
using UnityEngine.UI;

public class BathCoverageUI : MonoBehaviour
{
    public Slider coverageBar;
    public GameObject soapingPrompt;
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
