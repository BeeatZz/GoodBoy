using System;
using UnityEngine;

public enum BathDifficulty { Adult, Puppy }
public enum BathState     { Soaping, Rinsing, Complete }

public class BathMinigame : MonoBehaviour
{
    public static BathMinigame Instance { get; private set; }

    public static event Action<BathState> OnStateChanged;

    public MinigameController minigameController;
    public DogController      dogController;
    public FoamSystem         foamSystem;
    public ShampooSponge      shampooSponge;
    public ShowerHead         showerHead;
    public BathDifficulty difficulty = BathDifficulty.Adult;
    [Range(0f, 1f)]
    public float foamCoverageRequired = 0.9f;

    public BathState CurrentState { get; private set; }
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
        if (CurrentState == BathState.Soaping &&
            foamSystem.CoveragePercent >= foamCoverageRequired)
        {
            SetState(BathState.Rinsing);
        }
    }


    public void OnRinsingComplete()
    {
        if (CurrentState != BathState.Rinsing) return;
        SetState(BathState.Complete);
        dogController.SetClean();
        minigameController.Complete();
    }


    private void SetState(BathState state)
    {
        CurrentState = state;
        OnStateChanged?.Invoke(state);

        shampooSponge.gameObject.SetActive(state == BathState.Soaping);
        showerHead.gameObject.SetActive(state == BathState.Rinsing);
    }
}
