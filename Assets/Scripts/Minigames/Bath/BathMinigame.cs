using System;
using UnityEngine;

public enum BathDifficulty { Adult, Puppy }
public enum BathState     { Soaping, Rinsing, Complete }

public class BathMinigame : MonoBehaviour
{
    public static BathMinigame Instance { get; private set; }

    public static event Action<BathState> OnStateChanged;

    [Header("References")]
    public MinigameController minigameController;
    public DogController      dogController;
    public FoamSystem         foamSystem;
    public ShampooSponge      shampooSponge;
    public ShowerHead         showerHead;

    [Header("Settings")]
    public BathDifficulty difficulty = BathDifficulty.Adult;

    [Range(0f, 1f)]
    [Tooltip("Fraction of body zones that must be covered before rinsing unlocks.")]
    public float foamCoverageRequired = 0.9f;

    public BathState CurrentState { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        dogController.Initialise(difficulty);
        SetState(BathState.Soaping);
    }

    private void Update()
    {
        // Transition to rinsing once coverage threshold is met
        if (CurrentState == BathState.Soaping &&
            foamSystem.CoveragePercent >= foamCoverageRequired)
        {
            SetState(BathState.Rinsing);
        }
    }

    // ── Called by ShowerHead when all foam is cleared ─────────────────────────

    public void OnRinsingComplete()
    {
        if (CurrentState != BathState.Rinsing) return;
        SetState(BathState.Complete);
        dogController.SetClean();
        minigameController.Complete();
    }

    // ── State machine ─────────────────────────────────────────────────────────

    private void SetState(BathState state)
    {
        CurrentState = state;
        OnStateChanged?.Invoke(state);

        // Tools are shown/hidden based on the current phase
        shampooSponge.gameObject.SetActive(state == BathState.Soaping);
        showerHead.gameObject.SetActive(state == BathState.Rinsing);
    }
}
